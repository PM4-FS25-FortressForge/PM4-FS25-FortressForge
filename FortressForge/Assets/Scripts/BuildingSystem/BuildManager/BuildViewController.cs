using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.HexTile;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildManager
{
    public class BuildViewController : MonoBehaviour
    {
        public HexGridView hexGridView;
        public HexGridData hexGridData;

        private BaseBuildingTemplate _selectedBuildingTemplate;
        private GameObject _previewBuilding;
        private List<BaseBuildingTemplate> _placedBuildings = new();

        private bool _isPreviewMode = false;

        public void PreviewSelectedBuilding(GameObject building)
        {
            _selectedBuildingTemplate = new ResourceBuildingTemplate();
            _selectedBuildingTemplate.buildingPrefab = building;
            _selectedBuildingTemplate.shapeData = new List<HexTileCoordinate>
            {
                new HexTileCoordinate(0,0,0) // TODO: Adjust so this is taken from building directly
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
            
                if (Input.GetMouseButtonDown(0)) // First click to place // TODO consider using onclick events
                {
                    TryPlaceBuilding();
                }
                else if (Input.GetMouseButtonDown(1)) // Right click to cancel
                {
                    ExitBuildMode();
                }
                else if (Input.GetKeyDown(KeyCode.R)) // Rotate
                {
                    RotateObject(60f);
                }
            }
        }

        private void MovePreviewObject()
        {
            Vector3 worldPos = hexGridView.GetMouseWorldPosition();

            var hexCoord = new HexTileCoordinate(worldPos);

            Vector3 snappedPos = hexCoord.GetWorldPosition();
        
            _previewBuilding.transform.position = snappedPos;
        }

        private void TryPlaceBuilding()
        {
            Vector3 worldPos = _previewBuilding.transform.position;
            HexTileCoordinate hexCoord = new HexTileCoordinate(worldPos);

            if (hexGridData.ValidateBuildingPlacement(hexCoord, _selectedBuildingTemplate))
            {
                // Place the final building at the correct position
                Instantiate(_selectedBuildingTemplate.buildingPrefab, _previewBuilding.transform.position, _previewBuilding.transform.rotation);
            
                _placedBuildings.Add(_selectedBuildingTemplate);
                hexGridView.UpdateHexGridView();
            }
        }

        private void ExitBuildMode()
        {
            // If placement is invalid, destroy the preview
            _isPreviewMode = false;
            Destroy(_previewBuilding);
            _selectedBuildingTemplate = null;
        }
    
        private void RotateObject(float angle)
        {
            if (_previewBuilding != null)
            {
                // Get the current rotation around the Y-axis
                Vector3 currentRotation = _previewBuilding.transform.eulerAngles;

                // Increment the current rotation by the specified angle around the Y-axis
                currentRotation.y += angle;

                // Apply the new rotation while keeping other axes unchanged
                _previewBuilding.transform.rotation = Quaternion.Euler(currentRotation);
            }
        }
    }
}