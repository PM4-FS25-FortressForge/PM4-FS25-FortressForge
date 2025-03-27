using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;

public class ButtonManager : MonoBehaviour
{
    public List<Button> buildingButtons;
    public List<BaseBuilding> availableBuildings;
    
    public PlayerController playerController;

    void Start()
    {
        for (int i = 0; i < buildingButtons.Count; i++)
        {
            buildingButtons[i].onClick.AddListener(() => SelectBuilding(index));
        }
    }

    void SelectBuilding(int index)
    {
        if (index >= 0 && index < availableBuildings.Count)
        {
            playerController.SetSelectedBuilding(availableBuildings[index]);
        }
    }
}