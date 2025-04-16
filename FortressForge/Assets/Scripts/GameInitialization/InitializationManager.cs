using FortressForge.BuildingSystem;
using FortressForge.Economy;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.BuildingData;
using FortressForge.HexGrid.BuildManager;
using FortressForge.HexGrid.View;
using UnityEngine;
using UnityEngine.UI;

namespace FortressForge
{
    public class InitializationManager : MonoBehaviour
    {
        [Header("Game Start Configuration")]
        [SerializeField] private GameStartConfiguration _config;
        [SerializeField] private Dropdown _buildingDropdown;
        
        private void Start()
        {
            // Initialize the Terrain from _config
            Instantiate(_config.Terrain);
            
            BuildingManager buildingManager = new BuildingManager();
            
            EconomyManager economyManager = gameObject.AddComponent<EconomyManager>();
            economyManager.Init(buildingManager);
            
            HexGridManager hexGridManager = gameObject.AddComponent<HexGridManager>();
            hexGridManager.InitializeHexGridForPlayers(_config);
            
            InitializeHexGridViews(_config, hexGridManager);
            
            HexGridHoverController hexGridHoverController = gameObject.AddComponent<HexGridHoverController>();
            
            BuildViewController buildViewController = gameObject.AddComponent<BuildViewController>();
            buildViewController.Init(hexGridManager.AllGrids[0], economyManager.EconomySystem, buildingManager, hexGridHoverController);
            
            ButtonManager buttonManager = gameObject.AddComponent<ButtonManager>(); 
            buttonManager.Init(_buildingDropdown, _config.availableBuildings, buildViewController);
            
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