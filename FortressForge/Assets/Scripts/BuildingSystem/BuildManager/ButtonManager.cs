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
        public Dropdown dropdown;
        public List<BaseBuildingTemplate> availableBuildings;
    
        public BuildViewController buildViewController;
        
        public void Init()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(availableBuildings.Select(b => b.name).ToList());
            dropdown.onValueChanged.AddListener(SelectBuilding);
        }

        void SelectBuilding(int index)
        {
            if (index >= availableBuildings.Count)
            {
                Debug.LogError("Index out of range.");
                return;
            }

            buildViewController.PreviewSelectedBuilding(availableBuildings[index]);
        }
    }
}