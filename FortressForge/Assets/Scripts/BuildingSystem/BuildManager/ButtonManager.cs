using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FortressForge.BuildingSystem.BuildManager
{
    public class ButtonManager : MonoBehaviour
    {
        public List<Button> buildingButtons = new();
        public List<BaseBuildingTemplate> availableBuildings = new();
    
        [FormerlySerializedAs("playerController")] public BuildViewController buildViewController;

        void Start()
        {
            for (int i = 0; i < buildingButtons.Count; i++)
            {
                var index = i; // TODO: This is needed, check out why
                buildingButtons[i].onClick.AddListener(() => SelectBuilding(index));
            }
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