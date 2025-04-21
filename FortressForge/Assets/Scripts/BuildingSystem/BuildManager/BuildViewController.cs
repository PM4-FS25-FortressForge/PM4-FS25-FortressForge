using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.Economy;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;
using NUnit.Framework;
using FortressForge.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FortressForge.BuildingSystem.BuildManager
{
    public class BuildViewController : NetworkBehaviour, BuildActions.IPreviewModeActions
    {
        private BaseBuildingTemplate SelectedBuildingTemplate => AvailableBuildings[_selectedBuildingIndex];
        private List<BaseBuildingTemplate> AvailableBuildings => _config.availableBuildings;
        private bool IsPreviewMode => _selectedBuildingIndex != -1;
        
        private List<HexGridData> _hexGridDatas;
        private EconomySystem _economySystem;
        private BuildingManager _buildingManager;
        private GameStartConfiguration _config;
        
        private GameObject _previewBuilding;
        private MeshRenderer _previewBuildingMeshRenderer;
        private readonly List<HexTileCoordinate> _currentBuildTargets = new();

        private UIClickChecker _uiClickChecker;
        private BuildActions _input;
        private int _selectedBuildingIndex = -1;
        private HexTileData _hoveredHexTile;

        public void Init(List<HexGridData> hexGridData, EconomySystem economySystem, BuildingManager buildingManager, GameStartConfiguration config)
        {
            _hexGridDatas = hexGridData;
            _economySystem = economySystem;
            _buildingManager = buildingManager;
            _config = config;
            
            _hexGridDatas.ForEach(gridData => gridData.OnHoverTileChanged += OnHexTileChanged);
        }

        public void PreviewSelectedBuilding(int buildingIndex)
        {
            if (_previewBuilding != null)
                Destroy(_previewBuilding);
            
            _selectedBuildingIndex = buildingIndex;
            _previewBuilding = SpawnLocal(AvailableBuildings[_selectedBuildingIndex].BuildingPrefab);
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
            _hexGridDatas.ForEach(gridData => gridData.OnHoverTileChanged -= OnHexTileChanged);
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

            _uiClickChecker = new UIClickChecker();
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
            if (context.performed && IsPreviewMode && !_uiClickChecker.IsClickOnOverlay())
                TryBuyAndPlaceBuilding();
        }

        /// <summary>
        /// Called when the player exits build mode.
        /// </summary>
        public void OnExitBuildMode(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && IsPreviewMode && !_uiClickChecker.IsClickOnOverlay())
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

            Vector3 snappedPos = currentlyHoveredHexTileCoordinate.GetWorldPosition(_config.Radius, _config.TileHeight);

            Vector3 avgPos = GetAveragePosition(SelectedBuildingTemplate.ShapeData);
            _previewBuilding.transform.position = snappedPos + avgPos;

            MarkNewTilesAsBuildTargets(currentlyHoveredHexTileCoordinate, SelectedBuildingTemplate.ShapeData);
        }

        private void MarkNewTilesAsBuildTargets(HexTileCoordinate target, List<HexTileCoordinate> buildingShape)
        {
            foreach (HexTileCoordinate hexTileCoordinate in buildingShape)
            {
                HexTileCoordinate worldCoord = hexTileCoordinate + target;

                foreach (var grid in _hexGridDatas)
                {
                    if (grid.TileMap.TryGetValue(worldCoord, out var tileData))
                    {
                        tileData.IsBuildTarget = true;
                        _currentBuildTargets.Add(worldCoord);
                        break; // tile found, stop searching other grids
                    }
                }
            }

            _previewBuildingMeshRenderer.enabled = true;
        }

        /// <summary>
        /// Attempts to place a building at the hovered tile if the placement is valid.
        /// </summary>
        private void ClearPreviousBuildTargets()
        {
            foreach (HexTileCoordinate hexTileCoordinate in _currentBuildTargets)
            {
                foreach (var hexGridData in _hexGridDatas)
                {
                    if (hexGridData.TileMap.TryGetValue(hexTileCoordinate, out var tileData))
                    {
                        tileData.IsBuildTarget = false;
                        break; // Found the tile, no need to check other grids
                    }
                }
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

            // Find the grid that allows placement
            HexGridData targetGrid = _hexGridDatas
                .FirstOrDefault(grid => grid.ValidateBuildingPlacement(hexCoord, SelectedBuildingTemplate));

            bool hasResources = _economySystem.CheckForSufficientResources(SelectedBuildingTemplate.GetBuildCost());

            if (targetGrid == null || !hasResources)
            {
                Debug.Log("Placement failed");
                return;
            }

            PlaceBuilding(targetGrid, hexCoord);
            Debug.Log("Placement succeeded");
        }


        private void PlaceBuilding(HexGridData hexGridData, HexTileCoordinate hexCoord)
        {
            hexGridData.PlaceBuilding(hexCoord, SelectedBuildingTemplate);
            _economySystem.PayResource(SelectedBuildingTemplate.GetBuildCost());
            BaseBuildingTemplate copy = Instantiate(SelectedBuildingTemplate);
            _buildingManager.AddBuilding(copy);
            
            PlaceServerBuilding(_selectedBuildingIndex, 
                hexCoord.GetWorldPosition(_config.Radius, _config.TileHeight) + GetAveragePosition(SelectedBuildingTemplate.ShapeData),
                _previewBuilding.transform.rotation);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void PlaceServerBuilding(int buildingIndex, Vector3 transformPosition, Quaternion transformRotation)
        {
            SpawnNetworked(AvailableBuildings[buildingIndex].BuildingPrefab, transformPosition, transformRotation, gameObject.transform);
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
            SelectedBuildingTemplate.ShapeData = RotateByAngle(SelectedBuildingTemplate.ShapeData, (int)angle);
        }

        private Vector3 GetAveragePosition(List<HexTileCoordinate> hexTileCoordinates)
        {
            // Calculate the average position of the hex tile coordinates
            Vector3 averagePosition = Vector3.zero;
            foreach (HexTileCoordinate hexTileCoordinate in hexTileCoordinates)
            {
                averagePosition += hexTileCoordinate.GetWorldPosition(_config.Radius, _config.TileHeight);
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