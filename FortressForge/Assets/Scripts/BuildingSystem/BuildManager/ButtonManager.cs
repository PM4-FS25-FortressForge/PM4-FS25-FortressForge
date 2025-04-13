using System.Collections.Generic;
using System.Linq;
using FortressForge.HexGrid.BuildingData;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FortressForge.HexGrid.BuildManager
{
    public class ButtonManager : MonoBehaviour
    {
        private Dropdown _dropdown;
        private List<BaseBuildingTemplate> _availableBuildings;
    
        private BuildViewController  _buildViewController;
        
        public void Init(Dropdown dropdown, List<BaseBuildingTemplate> availableBuildings, BuildViewController buildViewController)
        {
            _dropdown = dropdown;
            _availableBuildings = availableBuildings;
            _buildViewController = buildViewController;
            
            // Initialize the dropdown with available buildings
            _dropdown.ClearOptions();
            _dropdown.AddOptions(_availableBuildings.Select(b => b.name).ToList());
            _dropdown.onValueChanged.AddListener(SelectBuilding);
        }

        void SelectBuilding(int index)
        {
            if (index >= _availableBuildings.Count)
            {
                Debug.LogError("Index out of range.");
                return;
            }

            _buildViewController.PreviewSelectedBuilding(_availableBuildings[index]);
        }
    }
}