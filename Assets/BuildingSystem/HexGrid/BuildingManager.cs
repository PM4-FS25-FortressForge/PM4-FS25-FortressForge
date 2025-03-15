using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private Vector3 origin;
    [SerializeField] private int initialRadius;
    [SerializeField] private int initialHeight;
    
    [SerializeField] private GameObject hexGridCellPrefab;
    [SerializeField] private Material occupiedMaterial;
    [SerializeField] private Material unoccupiedMaterial;
    
    private BaseBuilding coreBuilding;
    
    private HexGrid_ hexGrid;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hexGrid = new HexGrid_(origin, initialRadius, initialHeight);
        //occupiedMaterial = Resources.Load<Material>("Materials/OccupiedHexagonMaterial");
        //unoccupiedMaterial = Resources.Load<Material>("Materials/UnoccupiedHexagonMaterial");
        coreBuilding = Resources.Load<BaseBuilding>("BuildingSystem/ScriptableObjects/CoreBuilding");
        
        // place hex hexGridCellPrefab at each hex cell position
        foreach (var hexCell in hexGrid.GetHexMap().Values)
        {
            Vector3 hexCellPosition = hexGrid.HexCoordinatesToWorldCoordinates(hexCell.q, hexCell.r, hexCell.h);
            GameObject go = Instantiate(hexGridCellPrefab, hexCellPosition + hexGridCellPrefab.transform.position, hexGridCellPrefab.transform.rotation, this.transform);
            
            if (hexCell.isOccupied) {
                go.GetComponent<MeshRenderer>().material.color = occupiedMaterial.color;
            } else {
                go.GetComponent<MeshRenderer>().material.color = unoccupiedMaterial.color;
            }
        }
        // Place CoreBuilding at the center of the grid
        PlaceBuilding(coreBuilding, hexGrid.GetHexMap()[(0, 0, 0)]);
    }
    
    void PlaceBuilding(BaseBuilding building, HexCell hexCell)
    {
        // TODO implement placement validation
        
        // Set hex cell to occupied
        foreach (ShapeData shapeData in building.shapeData) {
            // Get the hex cell at the shapeData's position
            HexCell shapeHexCell = hexGrid.GetHexMap()[(hexCell.q + shapeData.q, hexCell.r + shapeData.r, hexCell.h + shapeData.h)];
            shapeHexCell.isOccupied = true;
        }
    }
    
    void ResetHexgrid()
    {
        foreach (var hexCell in hexGrid.GetHexMap().Values)
        {
            if (hexCell.isOccupied) {
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
