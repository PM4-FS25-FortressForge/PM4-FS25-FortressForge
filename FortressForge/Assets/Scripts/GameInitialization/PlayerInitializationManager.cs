using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildManager;
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
        [Header("Game Start Configuration")]
        [SerializeField] private GameStartConfiguration _config;
        
        public override void OnStartClient()
        {
            // Initialize Client for each player so ObserverRpc can be used
            Init();
        }
        
        public override void OnStartServer()
        {
            // Initialize Server for each player so ObserverRpc can be used. This gets called on each joining client on server side.
            // This is a bit of a hack, for optimization but higher complexity you could differentiate between server and client more.
            Init();
        }

        private void Init()
        {
            if (_config == null)
            {
                Debug.LogError("GameStartConfiguration is not set.");
                return;
            }

            // Take the clientId from the owner of this object, also called the playerId
            int clientId = Owner.ClientId;
            // Use ClientId to get the corresponding HexGridData
            HexGridData selectedGrid = HexGridManager.Instance.AllGrids[clientId];
            
            // initialize the grid view so allgrids is set
            BuildViewController buildViewController = gameObject.GetComponent<BuildViewController>();
            // We only initialize the view for the selected grid,
            // theoretically you could add multiple grids per player here. But EconomySystem is only one per player. So there mustn't be overlaps.
            buildViewController.Init(new List<HexGridData>{ selectedGrid }, 
                selectedGrid.EconomySystem, selectedGrid.BuildingManager, _config, HexGridManager.Instance);
            
            // After creating EconomySystem
            var economySync = gameObject.GetComponent<EconomySync>();
            EconomyController economyController = gameObject.AddComponent<EconomyController>();
            
            // Only the server needs to initialize the EconomySystem
            if (IsServerInitialized && !IsClientInitialized)
            {
                economySync.Init(selectedGrid.EconomySystem);
                economyController.Init(selectedGrid.EconomySystem);
            }
            
            // Initialize view only on clients, server doesn't need the individual views
            if (IsClientInitialized && IsOwner)
            {
                TopOverlayViewGenerator topOverlayViewGenerator = FindFirstObjectByType<UIDocument>().GetComponent<TopOverlayViewGenerator>();
                topOverlayViewGenerator.Init(economySync);

                BottomOverlayViewGenerator bottomOverlayViewGenerator = FindFirstObjectByType<UIDocument>().GetComponent<BottomOverlayViewGenerator>();
                bottomOverlayViewGenerator.Init(_config.availableBuildings, buildViewController);
            }
        }
    }
}