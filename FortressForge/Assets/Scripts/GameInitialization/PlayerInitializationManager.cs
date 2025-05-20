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

        private void Init()
        {
            if (!ValidateConfiguration()) return;

            var hoverController = GetHoverController();
            if (hoverController == null) return;

            int playerId     = Owner.ClientId;
            
            // Start-Punkt auswählen
            
            int gridId       = GetGridIdForPlayer(playerId);
            HexGridData grid = GetGridById(gridId);
            if (grid == null) return;

            InitializeBuildView(grid, hoverController);
            InitializeEconomy(grid);

            if (IsClientInitialized && IsOwner)
            {
                InitializeClientView(grid, gridId);
            }
        }

        /// <summary>Stellt sicher, dass die Konfiguration gesetzt ist.</summary>
        private bool ValidateConfiguration()
        {
            if (_gameStartConfiguration == null)
            {
                Debug.LogError("GameStartConfiguration is not set.");
                return false;
            }
            return true;
        }

        /// <summary>Sucht den Hover-Controller auf dem GlobalObjectManager.</summary>
        private HexTileHoverController GetHoverController()
        {
            var globalObj = GameObject.Find("GlobalObjectManager");
            if (globalObj == null)
            {
                Debug.LogError("GlobalObjectManager not found!");
                return null;
            }

            var controller = globalObj.GetComponent<HexTileHoverController>();
            if (controller == null)
            {
                Debug.LogError("HexTileHoverController not found!");
                return null;
            }

            return controller;
        }

        /// <summary>Ermittelt anhand der PlayerId das zugehörige GridId.</summary>
        private int GetGridIdForPlayer(int playerId)
        {
            return _gameSessionStartConfiguration.GridPlayerIdTuples
                   .First(gpit => gpit.PlayerId == playerId)
                   .HexGridId;
        }

        /// <summary>Gibt das HexGridData mit der angegebenen Id zurück.</summary>
        private HexGridData GetGridById(int gridId)
        {
            return HexGridManager.Instance.AllGrids
                   .FirstOrDefault(grid => grid.Id == gridId);
        }

        /// <summary>Initialisiert nur die Build-View für das ausgewählte Grid.</summary>
        private void InitializeBuildView(HexGridData grid, HexTileHoverController hoverController)
        {
            var buildView = gameObject.GetComponent<BuildViewController>();
            buildView.Init(
                new List<HexGridData> { grid },
                _gameStartConfiguration,
                HexGridManager.Instance,
                hoverController
            );
        }

        /// <summary>Initialisiert EconomySync und EconomyController auf dem Server.</summary>
        private void InitializeEconomy(HexGridData grid)
        {
            var economySync       = gameObject.GetComponent<EconomySync>();
            var economyController = gameObject.AddComponent<EconomyController>();

            // Nur auf Serverseite initialisieren
            if (IsServerInitialized && !IsClientInitialized)
            {
                economySync.Init(grid.EconomySystem);
                economyController.Init(grid.EconomySystem);
            }
        }

        /// <summary>Setzt Kamera, Overlay-Views, Markierung u.a. für den Client.</summary>
        private void InitializeClientView(HexGridData grid, int gridId)
        {
            grid.MarkGridAsOwned();

            // Kamera auf den Grid-Ursprung setzen
            Vector3 origin = _gameSessionStartConfiguration.HexGridOrigins[gridId];
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("Main camera not found!");
                return;
            }
            mainCam.GetComponent<CameraController>().SetTargetPosition(origin);

            // Obere Overlay-Leiste (Ressourcen)
            var overlay      = GameObject.Find("BuildingOverlay");
            var topOverlay   = overlay.GetComponent<TopOverlayViewGenerator>();
            var economySync  = gameObject.GetComponent<EconomySync>();
            topOverlay.Init(economySync);

            // Untere Overlay-Leiste (Gebäude-Auswahl)
            var bottomOverlay = overlay.GetComponent<BottomOverlayViewGenerator>();
            bottomOverlay.Init(_gameStartConfiguration.availableBuildings, gameObject.GetComponent<BuildViewController>());
        }
    }
}