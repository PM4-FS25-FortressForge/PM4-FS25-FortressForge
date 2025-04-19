using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.Economy;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FortressForge.BuildingSystem.BuildManager
{
    public class BuildViewController : NetworkBehaviour, BuildActions.IPreviewModeActions
    {
        private BaseBuildingTemplate SelectedBuildingTemplate => _availableBuildings[_selectedBuildingIndex];
        private bool IsPreviewMode => _selectedBuildingIndex != -1;
        
        private HexGridData _hexGridData;
        private EconomySystem _economySystem;
        private BuildingManager _buildingManager;
        
        private GameObject _previewBuilding;
        private MeshRenderer _previewBuildingMeshRenderer;
        private readonly List<HexTileCoordinate> _currentBuildTargets = new();

        private BuildActions _input;
        private int _selectedBuildingIndex = -1;
        private List<BaseBuildingTemplate> _availableBuildings;
        private HexTileData _hoveredHexTile;

        public void Init(HexGridData hexGridData, EconomySystem economySystem, BuildingManager buildingManager, List<BaseBuildingTemplate> availableBuildings)
        {
            _availableBuildings = availableBuildings;
            _hexGridData = hexGridData;
            _economySystem = economySystem;
            _buildingManager = buildingManager;
            
            _hexGridData.OnHoverTileChanged += OnHexTileChanged;
        }

        public void PreviewSelectedBuilding(int buildingIndex)
        {
            if (_previewBuilding != null)
                Destroy(_previewBuilding);
            
            _selectedBuildingIndex = buildingIndex;
            _previewBuilding = SpawnLocal(_availableBuildings[_selectedBuildingIndex].BuildingPrefab);
            _previewBuildingMeshRenderer = _previewBuilding.GetComponentInChildren<MeshRenderer>();
        }
        
        #region Input Callbacks
        
        private void OnHexTileChanged(HexTileData hexTileData)
        {
            if (_previewBuilding == null) return;
            _hoveredHexTile = hexTileData.IsMouseTarget ? hexTileData : null;
            MovePreviewObject(hexTileData);
        }
        
        private void OnDestroy()
        {
            _hexGridData.OnHoverTileChanged -= OnHexTileChanged;
            ExitBuildMode();
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
        /// Called when the player performs the place action. Attempts to place the building.
        /// </summary>
        public void OnPlaceAction(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && IsPreviewMode)
                TryBuyAndPlaceBuilding();
        }

        /// <summary>
        /// Called when the player exits build mode.
        /// </summary>
        public void OnExitBuildMode(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && IsPreviewMode)
                ExitBuildMode();
        }

        /// <summary>
        /// Rotates the preview building when triggered by the player.
        /// </summary>
        public void OnRotateBuilding(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && IsPreviewMode)
                RotateBuilding(60f); // Or whatever angle you want for rotation
        }
        
        #endregion

        /// <summary>
        /// Moves the preview object to the currently hovered hex tile position.
        /// </summary>
        /// <param name="hexTileData"></param>
        private void MovePreviewObject(HexTileData hexTileData)
        {
            ClearPreviousBuildTargets();
            
            // If this tile is not a valid target, do nothing
            if (!hexTileData.IsMouseTarget) return;
            
            HexTileCoordinate currentlyHoveredHexTileCoordinate = hexTileData.HexTileCoordinate;

            Vector3 snappedPos = currentlyHoveredHexTileCoordinate.GetWorldPosition(_hexGridData.TileRadius, _hexGridData.TileHeight);

            Vector3 avgPos = GetAveragePosition(SelectedBuildingTemplate.ShapeData);
            _previewBuilding.transform.position = snappedPos + avgPos;

            MarkNewTilesAsBuildTargets(currentlyHoveredHexTileCoordinate, SelectedBuildingTemplate.ShapeData);
        }

        private void MarkNewTilesAsBuildTargets(HexTileCoordinate target, List<HexTileCoordinate> buildingShape)
        {
            foreach (HexTileCoordinate hexTileCoordinate in buildingShape)
            {
                if (!_hexGridData.TileMap.TryGetValue(hexTileCoordinate + target, out var tileData)) 
                    continue;
                
                tileData.IsBuildTarget = true;
                _currentBuildTargets.Add(hexTileCoordinate + target);
            }
            
            _previewBuildingMeshRenderer.enabled = true;
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
            _previewBuildingMeshRenderer.enabled = false;
        }

        /// <summary>
        /// Attempts to place a building at the hovered tile if the placement is valid.
        /// </summary>
        private void TryBuyAndPlaceBuilding()
        {
            HexTileData currentlyHoveredTile = _hoveredHexTile;
            if (currentlyHoveredTile == null) return;
            
            HexTileCoordinate hexCoord = currentlyHoveredTile.HexTileCoordinate;

            // Check if the building can be placed
            if (!_economySystem.CheckForSufficientResources(SelectedBuildingTemplate.GetBuildCost())
                || !_hexGridData.ValidateBuildingPlacement(hexCoord, SelectedBuildingTemplate))
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
            _hexGridData.PlaceBuilding(hexCoord, SelectedBuildingTemplate);
            _economySystem.PayResource(SelectedBuildingTemplate.GetBuildCost());
            BaseBuildingTemplate copy = Instantiate(SelectedBuildingTemplate);
            _buildingManager.AddBuilding(copy);
            
            PlaceServerBuilding(_selectedBuildingIndex, 
                hexCoord.GetWorldPosition(_hexGridData.TileRadius, _hexGridData.TileHeight) + GetAveragePosition(SelectedBuildingTemplate.ShapeData),
                _previewBuilding.transform.rotation);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void PlaceServerBuilding(int buildingIndex, Vector3 transformPosition, Quaternion transformRotation)
        {
            SpawnNetworked(_availableBuildings[buildingIndex].BuildingPrefab, transformPosition, transformRotation, gameObject.transform);
        }
        
        /// <summary>
        /// Exits the building preview mode and cleans up the preview object.
        /// </summary>
        private void ExitBuildMode()
        {
            if (!IsPreviewMode) return;
        
            Destroy(_previewBuilding);
            _selectedBuildingIndex = -1;
            ClearPreviousBuildTargets();
        }

        /// <summary>
        /// Rotates the preview object around the Y-axis by the given angle.
        /// </summary>
        private void RotateBuilding(float angle)
        {
            if (!IsPreviewMode || _previewBuilding == null) return;
        
            // Get the current rotation around the Y-axis
            Vector3 currentRotation = _previewBuilding.transform.eulerAngles;

            // Increment the current rotation by the specified angle around the Y-axis
            currentRotation.y += angle;

            // Apply the new rotation while keeping other axes unchanged
            _previewBuilding.transform.rotation = Quaternion.Euler(currentRotation);
        
            // Apply rotation to the preview building tiles
            SelectedBuildingTemplate.ShapeData = RotateByAngle(SelectedBuildingTemplate.ShapeData, (int) angle);
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
        
        private static GameObject SpawnLocal(GameObject prefabToSpawn, Transform parent=null)
        {
            GameObject obj = Instantiate(prefabToSpawn, parent);
            if (obj.TryGetComponent(out NetworkObject netObj))
            {
                netObj.enabled = false;
            }
            return obj;
        }

        private static GameObject SpawnNetworked(GameObject prefabToSpawn, Vector3 transformPosition=new(), Quaternion transformRotation=new(), Transform parent=null)
        {
            GameObject obj = Instantiate(prefabToSpawn, transformPosition, transformRotation, parent);
            InstanceFinder.ServerManager.Spawn(obj);
            return obj;
        }
    }
}
