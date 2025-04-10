using System.Collections.Generic;
using System.Collections.ObjectModel;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.HexTile;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildManager
{
    public class BuildViewController : MonoBehaviour
    {
        private readonly List<BaseBuildingTemplate> _placedBuildings = new();
        public ReadOnlyCollection<BaseBuildingTemplate> PlacedBuildings { get; private set; }

        private HexGridView _hexGridView;
        private HexGridData _hexGridData;

        private BaseBuildingTemplate _selectedBuildingTemplate;
        private GameObject _previewBuilding;

        private bool _isPreviewMode = false;
        
        public void Init(HexGridView hexGridView, HexGridData hexGridData)
        {
            PlacedBuildings = _placedBuildings.AsReadOnly();
            
            _hexGridView = hexGridView;
            _hexGridData = hexGridData;
        }

        public void PreviewSelectedBuilding(BaseBuildingTemplate building)
        {
            _selectedBuildingTemplate = Instantiate(building);

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
            if (_hexGridView.GetCurrentlyHoveredHexTileCoordinate() != default)
            {
                Vector3 snappedPos = _hexGridView.GetCurrentlyHoveredHexTileCoordinate().GetWorldPosition(_hexGridData.TileRadius, _hexGridData.TileHeight);
                _previewBuilding.transform.position = snappedPos;
            }
        }

        /// <summary>
        /// Attempts to place a building at the hovered tile if the placement is valid.
        /// </summary>
        private void TryPlaceBuilding()
        {
            HexTileCoordinate hexCoord = _hexGridView.GetCurrentlyHoveredHexTileCoordinate();

            if (_hexGridData.ValidateBuildingPlacement(hexCoord, _selectedBuildingTemplate) && hexCoord != default)
            {
                // Place the final building at the correct position
                PlaceBuilding();
            }
        }

        private void PlaceBuilding()
        {
            Instantiate(_selectedBuildingTemplate.BuildingPrefab, _previewBuilding.transform.position, _previewBuilding.transform.rotation);
            BaseBuildingTemplate copy = Instantiate(_selectedBuildingTemplate);
            _placedBuildings.Add(copy);
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