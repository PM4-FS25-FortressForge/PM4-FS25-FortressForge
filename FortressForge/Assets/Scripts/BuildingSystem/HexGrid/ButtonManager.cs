using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;

public class ButtonManager : MonoBehaviour
{
    public List<Button> buildingButtons;
    public List<GameObject> availableBuildings;
    
    public PlayerController playerController;

    void Start()
    {
        for (int i = 0; i < buildingButtons.Count; i++)
        {
            buildingButtons[i].onClick.AddListener(() => SelectBuilding(i));
        }
    }

    void SelectBuilding(int index)
    {
        if (index >= 0 && index < availableBuildings.Count)
        {
            playerController.PreviewSelectedBuilding(availableBuildings[index]);
        }
    }
}