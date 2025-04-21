using FishNet;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.Economy;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.View;
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
            // Only initialize the client if it's your own
            Init();
        }
        
        public override void OnStartServer()
        {
            // Initialize same as client, to have access to the same data, and be flexible with data access
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
            
            BuildingManager buildingManager = new BuildingManager();
            
            EconomyManager economyManager = gameObject.AddComponent<EconomyManager>();
            economyManager.Init(buildingManager);
            
            var hexGridManager = HexGridManager.Instance;
            
            BuildViewController buildViewController = gameObject.GetComponent<BuildViewController>();
            buildViewController.Init(hexGridManager.AllGrids, economyManager.EconomySystem, buildingManager, _config);
            var buildingDropdown = FindObjectOfType<Dropdown>();

            TopOverlayViewGenerator topOverlayViewGenerator = FindFirstObjectByType<UIDocument>().GetComponent<TopOverlayViewGenerator>();
            topOverlayViewGenerator.Init(economyManager.EconomySystem);
            
            BottomOverlayViewGenerator bottomOverlayViewGenerator = FindFirstObjectByType<UIDocument>().GetComponent<BottomOverlayViewGenerator>();
            bottomOverlayViewGenerator.Init(_config.availableBuildings, buildViewController);

            // Initialize view only on the main client, server and other clients don't need the individual views
            if (!IsClient || !IsOwner) return;
            
            InitializeHexGridViews(_config, hexGridManager);
        }

        private void InitializeHexGridViews(GameStartConfiguration config, HexGridManager hexGridManager)
        {
            foreach (var data in hexGridManager.AllGrids)
            {
                HexGridView hexGridView = new GameObject("HexGridView_" + data.Id).AddComponent<HexGridView>();
                hexGridView.transform.SetParent(transform);
                hexGridView.Initialize(config.TilePrefab, data);
            }
        }
    }
}