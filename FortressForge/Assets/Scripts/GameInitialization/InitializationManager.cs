using FishNet.Object;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.Economy;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.View;
using UnityEngine;
using UnityEngine.UI;

namespace FortressForge
{
    public class InitializationManager : NetworkBehaviour
    {
        [Header("Game Start Configuration")]
        [SerializeField] private GameStartConfiguration _config;
        [SerializeField] private Dropdown _buildingDropdown;
        
        public override void OnStartClient()
        {
            // Initialize the Terrain from _config
            Instantiate(_config.Terrain);
            
            BuildingManager buildingManager = new BuildingManager();
            
            EconomyManager economyManager = gameObject.AddComponent<EconomyManager>();
            economyManager.Init(buildingManager);
            
            HexGridManager hexGridManager = gameObject.AddComponent<HexGridManager>();
            hexGridManager.InitializeHexGridForPlayers(_config);
            
            InitializeHexGridViews(_config, hexGridManager);
            
            BuildViewController buildViewController = gameObject.GetComponent<BuildViewController>();
            buildViewController.Init(hexGridManager.AllGrids, economyManager.EconomySystem, buildingManager, _config);
            _buildingDropdown = FindObjectOfType<Dropdown>();

            BuildMenuController buildMenuController = gameObject.AddComponent<BuildMenuController>(); 
            buildMenuController.Init(_buildingDropdown, _config.availableBuildings, buildViewController);
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