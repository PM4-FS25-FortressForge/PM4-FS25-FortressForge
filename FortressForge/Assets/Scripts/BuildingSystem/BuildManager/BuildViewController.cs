using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.HexTile;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.BuildingSystem.BuildManager
{
    public class BuildViewController : MonoBehaviour
    {
        public HexGridView HexGridView;
        public HexGridData HexGridData;

        private BaseBuildingTemplate _selectedBuildingTemplate;
        private GameObject _previewBuilding;
        private List<BaseBuildingTemplate> _placedBuildings = new();

        private bool _isPreviewMode = false;

        public void PreviewSelectedBuilding(BaseBuildingTemplate building)
        {
            _selectedBuildingTemplate = Instantiate(building);
            _selectedBuildingTemplate.ShapeData = new List<HexTileCoordinate>
            {
                new HexTileCoordinate(0,0,0) // TODO: Adjust so this is taken from  (shapeData) directly 
            };

            if (_previewBuilding != null)
            {
                Destroy(_previewBuilding);
            }

            // Instantiate the building's prefab for preview
            _previewBuilding = Instantiate(_selectedBuildingTemplate.BuildingPrefab);
            _isPreviewMode = true;
        }

        void Update() // TODO Input Controller benutzen
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

        /// <summary>
        /// Moves the preview object to the currently hovered hex tile position.
        /// </summary>
        private void MovePreviewObject()
        {
            if (HexGridView.GetCurrentlyHoveredHexTileCoordinate() != default)
            {
                Vector3 snappedPos = HexGridView.GetCurrentlyHoveredHexTileCoordinate().GetWorldPosition(HexGridData.TileRadius, HexGridData.TileHeight);
                _previewBuilding.transform.position = snappedPos;
            }
        }

        /// <summary>
        /// Attempts to place a building at the hovered tile if the placement is valid.
        /// </summary>
        private void TryPlaceBuilding()
        {
            HexTileCoordinate hexCoord = HexGridView.GetCurrentlyHoveredHexTileCoordinate();

            if (HexGridData.ValidateBuildingPlacement(hexCoord, _selectedBuildingTemplate) && hexCoord != default)
            {
                // Place the final building at the correct position
                Instantiate(_selectedBuildingTemplate.BuildingPrefab, _previewBuilding.transform.position, _previewBuilding.transform.rotation);
                BaseBuildingTemplate copy = Instantiate(_selectedBuildingTemplate);
                _placedBuildings.Add(copy);
                ExitBuildMode();
            }
        }

        /// <summary>
        /// Exits build mode and clears the preview state.
        /// </summary>
        private void ExitBuildMode()
        {
            // If placement is invalid, destroy the preview
            _isPreviewMode = false;
            Destroy(_previewBuilding);
            _selectedBuildingTemplate = null;
        }
    
        /// <summary>
        /// Rotates the preview object around the Y-axis by the given angle.
        /// </summary>
        private void RotateObject(float angle)  //TODO: use shapeData to rotate correctly in hex grid
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