using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Resources;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.HexTile;
using FortressForge.Economy;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildManager
{
    public class BuildViewController : MonoBehaviour
    {

        private HexGridView _hexGridView;
        private HexGridData _hexGridData;
        private EconomySystem _economySystem;
        private BuildingManager _buildingManager;

        private BaseBuildingTemplate _selectedBuildingTemplate;
        private GameObject _previewBuilding;
        private readonly List<HexTileCoordinate> _currentBuildTargets = new();

        private bool _isPreviewMode = false;
        
        public void Init(HexGridView hexGridView, HexGridData hexGridData, EconomySystem economySystem, BuildingManager buildingManager)
        {
            _hexGridView = hexGridView;
            _hexGridData = hexGridData;
            _economySystem = economySystem;
            _buildingManager = buildingManager;
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
                    TryBuyAndPlaceBuilding();
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
            var currentlyHoveredHexTileCoordinate = _hexGridView.GetCurrentlyHoveredHexTileCoordinate();
            if (currentlyHoveredHexTileCoordinate == default) return; // TODO default is wrong, use null, adjust after action changes
            
            Vector3 snappedPos = currentlyHoveredHexTileCoordinate.GetWorldPosition(_hexGridData.TileRadius, _hexGridData.TileHeight);
                
            Vector3 avgPos = GetAveragePosition(_selectedBuildingTemplate.ShapeData);
            _previewBuilding.transform.position = snappedPos + avgPos;
            
            MarkNewTilesAsBuildTargets(currentlyHoveredHexTileCoordinate, _selectedBuildingTemplate.ShapeData); 
        }

        private void MarkNewTilesAsBuildTargets(HexTileCoordinate target, List<HexTileCoordinate> buildingShape)
        {
            ClearPreviousBuildTargets();

            foreach (HexTileCoordinate hexTileCoordinate in buildingShape)
            {
                if (!_hexGridData.TileMap.TryGetValue(hexTileCoordinate + target, out var tileData)) 
                    continue;
                tileData.IsBuildTarget = true;
                _currentBuildTargets.Add(hexTileCoordinate + target);
            }
        }

        /// <summary>
        /// Attempts to place a building at the hovered tile if the placement is valid.
        /// </summary>
        private void ClearPreviousBuildTargets()
        {
            // Clear previous build targets
            foreach (HexTileCoordinate hexTileCoordinate in _currentBuildTargets)
            {
                _hexGridData.TileMap[hexTileCoordinate].IsBuildTarget = false;
            }

            _currentBuildTargets.Clear();
        }

        /// <summary>
        /// Attempts to place a building at the hovered tile if the placement is valid.
        /// </summary>
        private void TryBuyAndPlaceBuilding()
        {
            HexTileCoordinate hexCoord = _hexGridView.GetCurrentlyHoveredHexTileCoordinate();

            // Check if the building can be placed
            if (hexCoord == default // TODO default is wrong, use null, adjust after action changes
                || !_economySystem.CheckForSufficientResources(_selectedBuildingTemplate.GetBuildCost())
                || !_hexGridData.ValidateBuildingPlacement(hexCoord, _selectedBuildingTemplate))
            {
                Debug.Log("Placement failed");
                return;
            }
                
            // Place the final building at the correct position
            PlaceBuilding(hexCoord);
            Debug.Log("Placement succeeded");
        }

        private void PlaceBuilding(HexTileCoordinate hexCoord)
        {
            _hexGridData.PlaceBuilding(hexCoord, _selectedBuildingTemplate);
            _economySystem.PayResource(_selectedBuildingTemplate.GetBuildCost());
            Instantiate(_selectedBuildingTemplate.BuildingPrefab, _previewBuilding.transform.position, _previewBuilding.transform.rotation);
            BaseBuildingTemplate copy = Instantiate(_selectedBuildingTemplate);
            _buildingManager.AddBuilding(copy);
        }

        /// <summary>
        /// Exits build mode and clears the preview state.
        /// </summary>
        private void ExitBuildMode()
        {
            _isPreviewMode = false;
            Destroy(_previewBuilding);
            _selectedBuildingTemplate = null;
            ClearPreviousBuildTargets();
        }
    
        /// <summary>
        /// Rotates the preview object around the Y-axis by the given angle.
        /// </summary>
        private void RotateObject(float angle)  //TODO: use shapeData to rotate correctly in hex grid
        {
            if (_previewBuilding == null) return;
            
            // Get the current rotation around the Y-axis
            Vector3 currentRotation = _previewBuilding.transform.eulerAngles;

            // Increment the current rotation by the specified angle around the Y-axis
            currentRotation.y += angle;

            // Apply the new rotation while keeping other axes unchanged
            _previewBuilding.transform.rotation = Quaternion.Euler(currentRotation);
            
            // Apply rotation to the preview building tiles
            _selectedBuildingTemplate.ShapeData = RotateByAngle(_selectedBuildingTemplate.ShapeData, (int) angle);
        }

        private Vector3 GetAveragePosition(List<HexTileCoordinate> hexTileCoordinates)
        {
            // Calculate the average position of the hex tile coordinates
            Vector3 averagePosition = Vector3.zero;
            foreach (HexTileCoordinate hexTileCoordinate in hexTileCoordinates)
            {
                averagePosition += hexTileCoordinate.GetWorldPosition(_hexGridData.TileRadius, _hexGridData.TileHeight);
            }
            
            averagePosition /= hexTileCoordinates.Count;
            return averagePosition;
        }

        private List<HexTileCoordinate> RotateByAngle(List<HexTileCoordinate> hexTileCoordinates, int angle)
        {
            var rotatedHexTileCoordinates = new List<HexTileCoordinate>(hexTileCoordinates.Count);
            foreach (var hexTileCoordinate in hexTileCoordinates)
            {
                int t = (((-angle / 60) % 6) + 6) % 6; // make sure it's between 0 and 5, also handles negative angles

                int q = hexTileCoordinate.Q;
                int r = hexTileCoordinate.R;

                for (int i = 0; i < t; i++)
                {
                    int temp = q;
                    q = -r;
                    r = temp + r;
                }
                
                rotatedHexTileCoordinates.Add(new HexTileCoordinate(q, r, hexTileCoordinate.H));
            }
            
            return rotatedHexTileCoordinates;
        }
    }
}