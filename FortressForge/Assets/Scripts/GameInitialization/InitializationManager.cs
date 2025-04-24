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
    public class InitializationManager : NetworkBehaviour
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
            
            // Always select first grid for the player for now
            HexGridData selectedGrid = HexGridManager.Instance.AllGrids[0];
            
            BuildViewController buildViewController = gameObject.GetComponent<BuildViewController>();
            buildViewController.Init(HexGridManager.Instance.AllGrids, selectedGrid.EconomySystem, selectedGrid.BuildingManager, _config);
            
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
            if (!IsClientInitialized || !IsOwner) return;
            
            TopOverlayViewGenerator topOverlayViewGenerator = FindFirstObjectByType<UIDocument>().GetComponent<TopOverlayViewGenerator>();
            topOverlayViewGenerator.Init(economySync);
            
            BottomOverlayViewGenerator bottomOverlayViewGenerator = FindFirstObjectByType<UIDocument>().GetComponent<BottomOverlayViewGenerator>();
            bottomOverlayViewGenerator.Init(_config.availableBuildings, buildViewController);
        }
    }
}