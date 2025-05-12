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
    public class PlayerInitializationManager : NetworkBehaviour
    {
        [Header("Game Start Configuration")] [SerializeField]
        private GameStartConfiguration _gameStartConfiguration;

        [SerializeField] private GameSessionStartConfiguration _gameSessionStartConfiguration;

        public override void OnStartClient()
        {
            // Initialize Client for each player so ObserverRpc can be used
            StartCoroutine(WaitForInitialization());
        }

        public override void OnStartServer()
        {
            // Initialize Server for each player so ObserverRpc can be used. This gets called on each joining client on server side.
            // This is a bit of a hack, for optimization but higher complexity you could differentiate between server and client more.
            Init();
        }

        /// <summary>
        /// Waits for the initialization of the HexGridManager and the AllGrids list.
        /// </summary>
        /// <returns>IEnumerator</returns>
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
        /// Initializes the player and the grid view.
        /// </summary>
        private void Init()
        {
            if (_gameStartConfiguration == null)
            {
                Debug.LogError("GameStartConfiguration is not set.");
                return;
            }

            // Currently takes the playerId from the owner of this object
            int playerId = Owner.ClientId;

            // Take the clientId from the owner of this object, also called the playerId
            int gridId = _gameSessionStartConfiguration.GridPlayerIdTuples
                .First(gpit => gpit.PlayerId == playerId).HexGridId;
            HexGridData selectedGrid = HexGridManager.Instance.AllGrids[gridId];

            // initialize the grid view so allgrids is set
            BuildViewController buildViewController = gameObject.GetComponent<BuildViewController>();
            // We only initialize the view for the selected grid,
            // theoretically you could add multiple grids per player here. But EconomySystem is only one per player. So there mustn't be overlaps.
            buildViewController.Init(new List<HexGridData> { selectedGrid },
                _gameStartConfiguration, HexGridManager.Instance);

            // After creating EconomySystem
            var economySync = gameObject.GetComponent<EconomySync>();
            EconomyController economyController = gameObject.AddComponent<EconomyController>();

            // Only the server needs to initialize the EconomySystem
            if (IsServerInitialized && !IsClientInitialized)
            { // TODO this is a bit of a hack, but it works for now
                economySync.Init(selectedGrid.EconomySystem);
                economyController.Init(selectedGrid.EconomySystem);
            }

            // Initialize view only on clients, server doesn't need the individual views
            if (IsClientInitialized && IsOwner)
            {
                Vector3 gridOrigin = _gameSessionStartConfiguration.HexGridOrigins[gridId];

                Camera mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("Main camera not found!");
                    return;
                }

                mainCamera.GetComponent<CameraController>()
                    .SetTargetPosition(gridOrigin);
                
                GameObject buildingOverlay = GameObject.Find("BuildingOverlay");
                TopOverlayViewGenerator topOverlayViewGenerator = buildingOverlay.GetComponent<TopOverlayViewGenerator>();
                topOverlayViewGenerator.Init(economySync);

                BottomOverlayViewGenerator bottomOverlayViewGenerator = buildingOverlay.GetComponent<BottomOverlayViewGenerator>();
                bottomOverlayViewGenerator.Init(_gameStartConfiguration.availableBuildings, buildViewController);
            }
        }
    }
}