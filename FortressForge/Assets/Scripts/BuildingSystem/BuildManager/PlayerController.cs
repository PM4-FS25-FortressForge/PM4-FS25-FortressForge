using UnityEngine;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.Serializables;

public class PlayerController : MonoBehaviour
{
    public HexGridView hexGridView;
    public HexGridData hexGridData;

    private BaseBuilding _selectedBuilding;
    private GameObject _previewBuilding;

    private bool _isPreviewMode = false;

    public void PreviewSelectedBuilding(GameObject building)
    {
        _selectedBuilding = new ResourceBuilding();
        _selectedBuilding.buildingPrefab = building;
        _selectedBuilding.shapeData = new System.Collections.Generic.List<ShapeData>
        {
            new ShapeData { r = 0, q = 0, h = 0 } // TODO: Adjust so this is taken from building directly
        };

        if (_previewBuilding != null)
        {
            Destroy(_previewBuilding);
        }

        // Instantiate the building's prefab for preview
        _previewBuilding = Instantiate(_selectedBuilding.buildingPrefab);
        _isPreviewMode = true;
    }

    void Update()
    {
        // Move preview
        if (_isPreviewMode && _previewBuilding != null)
        {
            MovePreviewObject();
            
            if (Input.GetMouseButtonDown(0)) // First click to place
            {
                TryPlaceBuilding();
            }
        }
    }

    private void MovePreviewObject()
    {
        Vector3 worldPos = hexGridView.GetMouseWorldPosition();

        var hexCoord = new HexTileCoordinates(worldPos);
        Debug.Log(hexCoord);

        Vector3 snappedPos = hexCoord.GetWorldPosition();
        Debug.Log(snappedPos);
        _previewBuilding.transform.position = snappedPos;
    }

    private void TryPlaceBuilding()
    {
        Vector3 worldPos = _previewBuilding.transform.position;
        HexTileCoordinates hexCoord = new HexTileCoordinates(worldPos);

        if (hexGridData.ValidateBuildingPlacement(hexCoord, _selectedBuilding))
        {
            // Place the final building at the correct position
            Instantiate(_selectedBuilding.buildingPrefab, _previewBuilding.transform.position, Quaternion.identity);
            
            _isPreviewMode = false;
            Destroy(_previewBuilding);
            _selectedBuilding = null;
        }
        else
        {
            // If placement is invalid, destroy the preview
            _isPreviewMode = false;
            Destroy(_previewBuilding);
            _selectedBuilding = null;
        }
    }   
}