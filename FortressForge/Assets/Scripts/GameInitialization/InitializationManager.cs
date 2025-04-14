using System.Collections.Generic;
using FortressForge.Economy;
using FortressForge.GameInitialization;
using FortressForge.HexGrid.BuildingData;
using FortressForge.HexGrid.BuildManager;
using FortressForge.HexGrid;
using FortressForge.HexGrid.HoverController;
using FortressForge.Serializables;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
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
            
            gameObject.AddComponent<HexGridHoverController>();
            
            CreateBuildViewControllerForPlayer(_config.PlayerId, hexGridManager, economyManager.EconomySystem, buildingManager);
        }
        
        private void CreateBuildViewControllerForPlayer(string playerId, HexGridManager hexGridManager, EconomySystem economySystem, BuildingManager buildingManager)
        {
            foreach (var grid in hexGridManager.AllGrids)
            {
                if (grid.data.PlayerIds.Contains(playerId))
                {
                    BuildViewController buildViewController = gameObject.AddComponent<BuildViewController>();
                    buildViewController.Init(grid.view, grid.data, economySystem, buildingManager);
                    
                    ButtonManager buttonManager = gameObject.AddComponent<ButtonManager>(); 
                    buttonManager.Init(_buildingDropdown, _config.availableBuildings, buildViewController);
                    break;  // TODO remove this with multiplayer integration and make sure multiple buildview controller work as intended
                }
            }
        }
    }
}