using System.Collections.Generic;
using System.Linq;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FortressForge.BuildingSystem.BuildManager
{
    public class ButtonManager : MonoBehaviour
    {
        public Dropdown Dropdown;
        public List<BaseBuildingTemplate> AvailableBuildings;
    
        public BuildViewController  BuildViewController;
        
        public void Init()
        {
            Dropdown.ClearOptions();
            Dropdown.AddOptions(AvailableBuildings.Select(b => b.name).ToList());
            Dropdown.onValueChanged.AddListener(SelectBuilding);
        }

        void SelectBuilding(int index)
        {
            if (index >= AvailableBuildings.Count)
            {
                Debug.LogError("Index out of range.");
                return;
            }

            BuildViewController.PreviewSelectedBuilding(AvailableBuildings[index]);
        }
    }
}