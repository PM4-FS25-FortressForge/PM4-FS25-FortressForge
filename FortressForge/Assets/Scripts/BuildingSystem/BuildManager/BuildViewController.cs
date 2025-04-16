using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Resources;
using FortressForge.BuildingSystem;
using FortressForge.Economy;
using FortressForge.HexGrid.BuildingData;
using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FortressForge.HexGrid.BuildManager
{
    public class BuildViewController : MonoBehaviour, BuildActions.IPreviewModeActions
    {
        private HexGridHoverController _hexGridHoverController;
        private HexGridData _hexGridData;
        private EconomySystem _economySystem;
        private BuildingManager _buildingManager;
        
        private BaseBuildingTemplate _selectedBuildingTemplate;
        private GameObject _previewBuilding;
        private readonly List<HexTileCoordinate> _currentBuildTargets = new();

        private bool _isPreviewMode = false;
        private BuildActions _input;
    
        public void Init(HexGridData hexGridData, EconomySystem economySystem, BuildingManager buildingManager, HexGridHoverController hexGridHoverController)
        {
            _hexGridData = hexGridData;
            _economySystem = economySystem;
            _buildingManager = buildingManager;
            _hexGridHoverController = hexGridHoverController;
        }

        public void PreviewSelectedBuilding(BaseBuildingTemplate building)
        {
            _selectedBuildingTemplate = Instantiate(building);

            if (_previewBuilding != null)
                Destroy(_previewBuilding);

            _previewBuilding = Instantiate(_selectedBuildingTemplate.BuildingPrefab);
            _isPreviewMode = true;
        }

        /// <summary>
        /// Initializes the input handler on Awake.
        /// </summary>
        private void Awake()
        {
            _input = new BuildActions();
        }

        /// <summary>
        /// Enables input actions and sets callbacks.
        /// </summary>
        private void OnEnable()
        {
            _input.PreviewMode.SetCallbacks(this);
            _input.PreviewMode.Enable();
        }

        /// <summary>
        /// Disables input actions and clears callbacks.
        /// </summary>
        private void OnDisable()
        {
            _input.PreviewMode.Disable();
            _input.PreviewMode.SetCallbacks(null); // Cleanup
        }

        /// <summary>
        /// Moves the preview building each frame if in preview mode.
        /// </summary>
        private void Update()
        {
            if (_isPreviewMode)
            {
                MovePreviewObject();
            }
        }

        /// <summary>
        /// Called when the player performs the place action. Attempts to place the building.
        /// </summary>
        public void OnPlaceAction(InputAction.CallbackContext context)
        {
            if (context.performed && _isPreviewMode)
                TryBuyAndPlaceBuilding();
        }

        /// <summary>
        /// Called when the player exits build mode.
        /// </summary>
        public void OnExitBuildMode(InputAction.CallbackContext context)
        {
            if (context.performed && _isPreviewMode)
                ExitBuildMode();
        }

        /// <summary>
        /// Rotates the preview building when triggered by the player.
        /// </summary>
        public void OnRotateBuilding(InputAction.CallbackContext context)
        {
            if (context.performed)
                RotateObject(60f); // Or whatever angle you want for rotation
        }

        /// <summary>
        /// Moves the preview object to the currently hovered hex tile position.
        /// </summary>
        private void MovePreviewObject()
        {
            HexTileView currentlyHoveredTile = _hexGridHoverController.CurrentlyHoveredTile;
            if (currentlyHoveredTile == null) return;
            HexTileCoordinate currentlyHoveredHexTileCoordinate =
                currentlyHoveredTile.TileData.HexTileCoordinate;

            Vector3 snappedPos =
                currentlyHoveredHexTileCoordinate.GetWorldPosition(_hexGridData.TileRadius,
                    _hexGridData.TileHeight);

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
            HexTileView currentlyHoveredTile = _hexGridHoverController.CurrentlyHoveredTile;
            if (currentlyHoveredTile == null) return;
            HexTileCoordinate hexCoord = 
                currentlyHoveredTile.TileData.HexTileCoordinate;

            // Check if the building can be placed
            if (!_economySystem.CheckForSufficientResources(_selectedBuildingTemplate.GetBuildCost())
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
        /// Exits the building preview mode and cleans up the preview object.
        /// </summary>
        private void ExitBuildMode()
        {
            if (!_isPreviewMode) return;
        
            _isPreviewMode = false;
            Destroy(_previewBuilding);
            _selectedBuildingTemplate = null;
            ClearPreviousBuildTargets();
        }

        /// <summary>
        /// Rotates the preview object around the Y-axis by the given angle.
        /// </summary>
        private void RotateObject(float angle)
        {
            if (!_isPreviewMode || _previewBuilding == null) return;
        
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
