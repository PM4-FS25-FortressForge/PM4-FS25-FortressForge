using System.Collections.Generic;
using UnityEngine;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.Serializables;

public class BuildViewController : MonoBehaviour
{
    public HexGridView hexGridView;
    public HexGridData hexGridData;

    private BaseBuildingTemplate _selectedBuildingTemplate;
    private GameObject _previewBuilding;

    private bool _isPreviewMode = false;

    public void PreviewSelectedBuilding(GameObject building)
    {
        _selectedBuildingTemplate = new ResourceBuildingTemplate();
        _selectedBuildingTemplate.buildingPrefab = building;
        _selectedBuildingTemplate.shapeData = new List<HexTileCoordinates>
        {
            new HexTileCoordinates(0,0,0) // TODO: Adjust so this is taken from building directly
        };

        if (_previewBuilding != null)
        {
            Destroy(_previewBuilding);
        }

        // Instantiate the building's prefab for preview
        _previewBuilding = Instantiate(_selectedBuildingTemplate.buildingPrefab);
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

        Vector3 snappedPos = hexCoord.GetWorldPosition();
        
        _previewBuilding.transform.position = snappedPos;
    }

    private void TryPlaceBuilding()
    {
        Vector3 worldPos = _previewBuilding.transform.position;
        HexTileCoordinates hexCoord = new HexTileCoordinates(worldPos);

        if (hexGridData.ValidateBuildingPlacement(hexCoord, _selectedBuildingTemplate))
        {
            // Place the final building at the correct position
            Instantiate(_selectedBuildingTemplate.buildingPrefab, _previewBuilding.transform.position, Quaternion.identity);
            
            _isPreviewMode = false;
            Destroy(_previewBuilding);
            _selectedBuildingTemplate = null;
        }
        else
        {
            // If placement is invalid, destroy the preview
            _isPreviewMode = false;
            Destroy(_previewBuilding);
            _selectedBuildingTemplate = null;
        }
    }   
}