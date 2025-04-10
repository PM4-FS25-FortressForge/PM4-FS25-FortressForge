using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.BuildingSystem.HexGrid;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FortressForge
{
    public class InitializationManager : MonoBehaviour
    {
        [Header("Available Buildings")] [SerializeField]
        private List<BaseBuildingTemplate> availableBuildings = new List<BaseBuildingTemplate>();
        
        [Header("Building Dropdown button")] [SerializeField]
        private Dropdown buildingDropdown; 
        
        [Header("HexGridManager")] [SerializeField]
        private HexGridManager hexGridManager;
        
        private void Start()
        {
            // Add ButtonManager, EconomyManager and BuildViewController // TODO currently this is only done for one player
            var buildingManager = new BuildingManager();
            
            var economyManager = gameObject.AddComponent<Economy.EconomyManager>();
            economyManager.Init(buildingManager);
            
            var grid1 = hexGridManager.AllGrids[0]; // TODO tmp assignment
            
            var buildViewController = gameObject.AddComponent<BuildViewController>();
            buildViewController.Init(grid1.view, grid1.data, economyManager.EconomySystem, buildingManager);
            
            
            var buttonManager = gameObject.AddComponent<ButtonManager>(); 
            buttonManager.Init(buildingDropdown, availableBuildings, buildViewController);
        }
    }
}