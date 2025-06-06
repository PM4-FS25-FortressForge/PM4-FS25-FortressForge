using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.CameraControll;
using FortressForge.Economy;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;
using FortressForge.Networking;
using FortressForge.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Handles the initialization of player-specific systems and grid views in a networked game session.
    /// </summary>
    public class PlayerInitializationManager : NetworkBehaviour
    {
        [Header("Game Start Configuration")]
        [SerializeField]
        private GameStartConfiguration _gameStartConfiguration;

        [SerializeField]
        private GameSessionStartConfiguration _gameSessionStartConfiguration;

        /// <summary>
        /// Called when the client starts. Begins initialization coroutine.
        /// </summary>
        public override void OnStartClient()
        {
            // Initialize Client for each player so ObserverRpc can be used
            StartCoroutine(WaitForInitialization());
        }

        /// <summary>
        /// Called when the server starts. Initializes player and grid view for each player.
        /// </summary>
        public override void OnStartServer()
        {
            // Initialize Server for each player so ObserverRpc can be used.
            // This is a bit of a hack; for optimization but higher complexity you could differentiate between server and client more.
            Init();
        }

        /// <summary>
        /// Waits for the HexGridManager and its AllGrids list to be initialized before proceeding.
        /// </summary>
        /// <returns>Coroutine enumerator.</returns>
        private IEnumerator WaitForInitialization()
        {
            yield return new WaitUntil(() =>
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "LobbyScene" &&
                    HexGridManager.Instance != null &&
                    HexGridManager.Instance.AllGrids != null &&
                    HexGridManager.Instance.AllGrids.Count > 0
                , new TimeSpan(0, 0, 10),
                () => Debug.Log("HexGridManager is not initialized after 10 seconds."));
            Init();
        }

        /// <summary>
        /// Initializes the player, their grid, economy, and UI overlays.
        /// </summary>
        private void Init()
        {
            // Validate configuration
            if (!ValidateConfiguration()) return;

            // Find global object manager and required controllers
            var globalObjectGameObject = FindGlobalObjectManager();
            if (globalObjectGameObject == null) return;

            var hoverController = GetComponentOrLog<HexTileHoverController>(globalObjectGameObject);
            if (hoverController == null) return;

            var previewController = GetComponentOrLog<PreviewController>(globalObjectGameObject);
            if (previewController == null) return;

            // Get the grid assigned to this player
            int playerId = Owner.ClientId;
            var selectedGrid = GetPlayerGrid(playerId);
            if (selectedGrid == null) return;

            // Initialize build view and economy systems
            InitializeBuildView(selectedGrid, hoverController, previewController);
            InitializeEconomy(selectedGrid);

            // If this is the client and the owner, initialize client-specific UI and camera
            if (IsClientInitialized && IsOwner)
                InitializeClientView(selectedGrid);
        }

        /// <summary>
        /// Checks if the game start configuration is set.
        /// </summary>
        /// <returns>True if configuration is valid, false otherwise.</returns>
        private bool ValidateConfiguration()
        {
            if (_gameStartConfiguration == null)
            {
                Debug.LogError("GameStartConfiguration is not set.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Finds the global object manager GameObject in the scene.
        /// </summary>
        /// <returns>The GameObject if found, otherwise null.</returns>
        private GameObject FindGlobalObjectManager()
        {
            var obj = GameObject.Find("GlobalObjectManager");
            if (obj == null)
                Debug.LogError("GlobalObjectManager not found!");
            return obj;
        }

        /// <summary>
        /// Gets a component of type T from the given GameObject, logging an error if not found.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="obj">GameObject to search.</param>
        /// <returns>The component if found, otherwise null.</returns>
        private T GetComponentOrLog<T>(GameObject obj) where T : Component
        {
            var comp = obj.GetComponent<T>();
            if (comp == null)
                Debug.LogError(typeof(T).Name + " not found!");
            return comp;
        }

        /// <summary>
        /// Retrieves the grid assigned to the given player ID.
        /// </summary>
        /// <param name="playerId">The player's ID.</param>
        /// <returns>The assigned HexGridData, or null if not found.</returns>
        private HexGridData GetPlayerGrid(int playerId)
        {
            var tuple = _gameSessionStartConfiguration.GridPlayerIdTuples
                .FirstOrDefault(gpit => gpit.PlayerId == playerId);
            if (tuple == default)
            {
                Debug.LogError($"No grid assigned for playerId {playerId}.");
                return null;
            }
            int gridId = tuple.HexGridId;
            var grid = HexGridManager.Instance.AllGrids.FirstOrDefault(g => g.Id == gridId);
            if (grid == null)
                Debug.LogError($"No grid found with Id {gridId}.");
            return grid;
        }

        /// <summary>
        /// Initializes the build view controller for the player.
        /// </summary>
        /// <param name="selectedGrid">The player's assigned grid.</param>
        /// <param name="hoverController">The hex tile hover controller.</param>
        /// <param name="previewController">The preview controller.</param>
        private void InitializeBuildView(HexGridData selectedGrid, HexTileHoverController hoverController, PreviewController previewController)
        {
            var buildViewController = gameObject.GetComponent<BuildViewController>();
            buildViewController.Init(
                new List<HexGridData> { selectedGrid },
                _gameStartConfiguration,
                HexGridManager.Instance,
                hoverController,
                previewController
            );
        }

        /// <summary>
        /// Initializes the economy system for the player's grid.
        /// </summary>
        /// <param name="selectedGrid">The player's assigned grid.</param>
        private void InitializeEconomy(HexGridData selectedGrid)
        {
            var economySync = gameObject.GetComponent<EconomySync>();
            var economyController = gameObject.AddComponent<EconomyController>();
            if (IsServerInitialized && !IsClientInitialized)
            {
                economySync.Init(selectedGrid.EconomySystem);
                economyController.Init(selectedGrid.EconomySystem);
            }
        }

        /// <summary>
        /// Initializes client-specific UI overlays and camera for the player.
        /// </summary>
        /// <param name="selectedGrid">The player's assigned grid.</param>
        private void InitializeClientView(HexGridData selectedGrid)
        {
            // Mark grid as owned by this player
            selectedGrid.MarkGridAsOwned();
            int gridId = selectedGrid.Id;
            Vector3 gridOrigin = _gameSessionStartConfiguration.HexGridOrigins[gridId];

            // Move camera to the grid's origin
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main camera not found!");
                return;
            }
            mainCamera.GetComponent<CameraController>().SetTargetPosition(gridOrigin);

            // Initialize top overlay UI with economy sync
            var topOverlay = GameObject.Find("TopOverlay");
            var topOverlayViewGenerator = topOverlay.GetComponent<TopOverlayViewGenerator>();
            var economySync = gameObject.GetComponent<EconomySync>();
            topOverlayViewGenerator.Init(economySync);

            // Initialize bottom overlay UI with available buildings and build view controller
            var buildingOverlay = GameObject.Find("BuildingOverlay");
            var bottomOverlayViewGenerator = buildingOverlay.GetComponent<BottomOverlayViewGenerator>();
            var buildViewController = gameObject.GetComponent<BuildViewController>();
            bottomOverlayViewGenerator.Init(_gameStartConfiguration.availableBuildings, buildViewController);
        }
    }
}