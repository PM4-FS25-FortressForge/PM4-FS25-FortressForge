using UnityEngine;

public class BuildingPlacingSystem : MonoBehaviour
{
    [SerializeField] private GameObject origin;
    [SerializeField] private GameObject hexGridCellPrefab;
    [SerializeField] private int initialRadius;
    [SerializeField] private int initialHeight;
    private HexGrid hexGrid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hexGrid = new HexGrid(origin.transform.position, initialRadius, initialHeight);
        // place hex hexGridCellPrefab at each hex cell position
        foreach (var hexCell in hexGrid.GetHexMap().Values)
        {
            Vector3 hexCellPosition = hexGrid.HexCoordinatesToWorldCoordinates(hexCell.Q, hexCell.R, hexCell.H);
            Instantiate(hexGridCellPrefab, hexCellPosition + hexGridCellPrefab.transform.position, hexGridCellPrefab.transform.rotation, this.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
