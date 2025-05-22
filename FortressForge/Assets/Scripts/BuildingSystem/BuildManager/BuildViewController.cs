using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using UnityEngine;
using UnityEngine.InputSystem;


namespace FortressForge.BuildingSystem.BuildManager
{
    public class BuildViewController : NetworkBehaviour, BuildActions.IPreviewModeActions
    {
        private List<HexGridData> _ownedHexGridDatas = new();
        private GameStartConfiguration _config;
        private HexGridManager _hexGridManager;
        private GameObject _previewBuilding;
        private MeshRenderer _previewBuildingMeshRenderer;
        private float _currentPreviewBuildingRotation = 0f;
        private readonly List<HexTileCoordinate> _currentBuildTargets = new();
        private HexTileData _hoveredHexTile;

        private int _selectedBuildingIndex = -1;
        private BaseBuildingTemplate SelectedBuildingTemplate => AvailableBuildings[_selectedBuildingIndex];
        private bool IsPreviewMode => _selectedBuildingIndex != -1;
        private List<BaseBuildingTemplate> AvailableBuildings => _config.availableBuildings;

        private BuildActions _input;
        private HexTileHoverController _hexTileHoverController;

        public static event Action OnExitBuildModeEvent;

        public void Init(List<HexGridData> hexGridData, GameStartConfiguration config,
            HexGridManager hexGridManager, HexTileHoverController hoverController)
        {
            _ownedHexGridDatas = hexGridData;
            _config = config;
            _hexGridManager = hexGridManager;
            _hexTileHoverController = hoverController;

            _hexTileHoverController.OnHoverTileChanged += OnHexTileChanged;
        }

        /// <summary>
        /// Handles selection of the building item. By showing the preview building and marking the tiles
        /// on the targeted tile as build targets.
        /// </summary>
        /// <param name="buildingIndex"></param>
        public void PreviewSelectedBuilding(int buildingIndex)
        {
            if (!IsOwner) return;

            if (_previewBuilding != null)
                Destroy(_previewBuilding);

            _selectedBuildingIndex = buildingIndex;

            _previewBuilding = SpawnLocal(SelectedBuildingTemplate.BuildingPrefab);
            _previewBuildingMeshRenderer = _previewBuilding.GetComponentInChildren<MeshRenderer>();
            if (_previewBuildingMeshRenderer == null)
            {
                Debug.LogError("Preview building prefab does not have a MeshRenderer component.");
                return;
            }
            
            var collider = _previewBuilding.GetComponentInChildren<Collider>();
            if (collider != null)
                collider.enabled = false;
            
            // Place the preview building and rotate it correctly.
            RotatePreviewBuilding(0);
        }

        #region Input Callbacks

        private void OnHexTileChanged(HexTileData hexTileData)
        {
            if (_previewBuilding == null) return;
            _hoveredHexTile = hexTileData;

            if (_hoveredHexTile == null)
            {
                ClearPreviousBuildTargets();
                return;
            }

            MovePreviewObject(_hoveredHexTile.HexTileCoordinate);
        }

        private void OnDestroy()
        {
            _hexTileHoverController.OnHoverTileChanged -= OnHexTileChanged;
            ExitBuildMode();
        }

        private void OnEnable()
        {
            _input = new BuildActions();
            _input.PreviewMode.SetCallbacks(this);
            _input.PreviewMode.Enable();
        }

        private void OnDisable()
        {
            _input.PreviewMode.Disable();
            _input.PreviewMode.SetCallbacks(null);
        }

        public void OnPlaceAction(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && IsPreviewMode && _hoveredHexTile != null)
            {
                var coord = _hoveredHexTile.HexTileCoordinate;
                TryPlaceBuildingServerRpc(_selectedBuildingIndex, coord, _currentPreviewBuildingRotation);
            }
        }

        public void OnExitBuildMode(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && IsPreviewMode)
                ExitBuildMode();
        }

        public void OnRotateBuilding(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && IsPreviewMode)
                RotatePreviewBuilding(60f);
        }

        #endregion

        /// <summary>
        /// Moves the preview building to the target coordinate,
        /// by removing the previous build targets and marking the new ones
        /// and moving the prefab to the correct position.
        /// </summary>
        /// <param name="targetCoord"></param>
        private void MovePreviewObject(HexTileCoordinate targetCoord)
        {
            ClearPreviousBuildTargets();
            Vector3 snappedPos = targetCoord.GetWorldPosition(_config.GridRadius, _config.TileHeight);
            List<HexTileCoordinate> rotatedShape = HexTileHelper.GetRotatedShape(SelectedBuildingTemplate.ShapeData, 
                _currentPreviewBuildingRotation);
            Vector3 avgPos = HexTileHelper.GetAveragePosition(rotatedShape, _config.TileSize, _config.TileHeight);

            _previewBuilding.transform.position = snappedPos + avgPos;
            _previewBuildingMeshRenderer.enabled = true;
            MarkNewTilesAsBuildTargets(targetCoord, rotatedShape);
        }

        private void MarkNewTilesAsBuildTargets(HexTileCoordinate origin, List<HexTileCoordinate> shape)
        {
            foreach (var offset in shape)
            {
                var worldCoord = offset + origin;

                // Take any grid and mark the tile as a build target
                var tile = _hexGridManager.GetHexTileDataOrCreate(worldCoord);

                if (tile != null)
                {
                    tile.IsBuildTarget = true;
                    _currentBuildTargets.Add(worldCoord);
                }
            }
        }

        private void ClearPreviousBuildTargets()
        {
            foreach (HexTileCoordinate coord in _currentBuildTargets)
            {
                var tile = _hexGridManager.GetHexTileDataOrCreate(coord);
                if (tile == null) continue;

                tile.IsBuildTarget = false;
            }

            _currentBuildTargets.Clear();
            _previewBuildingMeshRenderer.enabled = false;
        }

        /// <summary>
        /// Tries to place the building on the server.
        /// This checks if the building can be placed on the target tile and if there are sufficient resources.
        /// For anti cheat reasons, this is done on the server.
        /// </summary>
        /// <param name="buildingIndex"></param>
        /// <param name="coord"></param>
        /// <param name="rotation"></param>
        [ServerRpc(RequireOwnership = false)]
        private void TryPlaceBuildingServerRpc(int buildingIndex, HexTileCoordinate coord, float rotation)
        {
            BaseBuildingTemplate template = AvailableBuildings[buildingIndex];
            (var shapeData, var isStackableList) = HexTileHelper.ExtractShapeInformation(template.ShapeDataEntries);

            List<HexTileCoordinate> rotatedShape = HexTileHelper.GetRotatedShape(shapeData, rotation);
            List<HexTileCoordinate> globalRotatedShape = rotatedShape.Select(tile => tile + coord).ToList();

            HexGridData targetGrid = _ownedHexGridDatas
                .FirstOrDefault(grid => grid.ValidateBuildingPlacement(coord, rotatedShape));

            if (targetGrid == null || !targetGrid.EconomySystem.CheckForSufficientResources(template.GetBuildCost()))
            {
                Debug.Log("Server: Invalid placement or insufficient resources.");
                return;
            }

            // Spawn the building prefab at the calculated position and rotation
            Vector3 pos = coord.GetWorldPosition(_config.GridRadius, _config.TileHeight) + 
                          HexTileHelper.GetAveragePosition(rotatedShape, _config.TileSize, _config.TileHeight);
            Quaternion rot = Quaternion.Euler(0f, rotation, 0f) * template.BuildingPrefab.transform.rotation;
            GameObject prefab = SpawnNetworked(template.BuildingPrefab, pos, rot, targetGrid.Id);
            if (prefab == null)
            {
                Debug.LogError("Failed to spawn building prefab.");
                return;
            }
            // Give ownership to the player who placed the building.
            // Enforces that only the owner can interact with the placed buildings.
            // Because of FishNet's ownership system, we need to do this directly inside the serverrpc method
            NetworkObject netObj = prefab.GetComponent<NetworkObject>();
            if (netObj != null && base.Owner.IsValid)
            {
                netObj.GiveOwnership(base.Owner);
            }
            if (template.GetType() == typeof(WeaponBuildingTemplate))
            { // Quick n dirty init for prefab
                prefab.GetComponent<WeaponInputHandler>().Init(targetGrid);
            }

            targetGrid.MarkBuildingTiles(coord, rotatedShape, isStackableList);
            targetGrid.EconomySystem.PayResource(template.GetBuildCost());
            
            // Add reference to the building manager for later use.
            List<HexTileData> tileDatas = globalRotatedShape
                .Select(coord => targetGrid.TileMap[coord])
                .ToList();
            targetGrid.BuildingManager.AddBuilding(new BuildingData(prefab, tileDatas, template));

            SyncPlacedBuildingToClientsRpc(buildingIndex, coord, targetGrid.Id, rotation, prefab);
        }
        
        [ObserversRpc]
        private void SyncPlacedBuildingToClientsRpc(int buildingIndex, HexTileCoordinate coord, int hexGridId,
            float rotation,
            GameObject prefab)
        {
            HexGridData targetGrid = _hexGridManager.AllGrids.FirstOrDefault(grid => grid.Id == hexGridId);
            BaseBuildingTemplate template = AvailableBuildings[buildingIndex];
            var (shapeData, isStackableList) = HexTileHelper.ExtractShapeInformation(template.ShapeDataEntries);
            List<HexTileCoordinate> rotatedShape = HexTileHelper.GetRotatedShape(shapeData, rotation);
            List<HexTileCoordinate> globalRotatedShape = rotatedShape.Select(tile => tile + coord).ToList();
            targetGrid.MarkBuildingTiles(coord, rotatedShape, isStackableList);

            // Add local reference to building manager for later use.
            List<HexTileData> tileDatas = globalRotatedShape
                .Select(coord => targetGrid.TileMap[coord])
                .ToList();
            var buildingData = new BuildingData(prefab, tileDatas, template);

            var tileData = prefab.AddComponent<BuildingView>();
            tileData.Init(buildingData, _config);

            // Repeated steps from serverrpc dont need to be repeated server side
            if (!IsServerInitialized)
            {
                targetGrid.BuildingManager.AddBuilding(buildingData);
                targetGrid.MarkBuildingTiles(coord, rotatedShape, isStackableList);
                
                if (template is WeaponBuildingTemplate)
                {
                    var handler = prefab.GetComponent<WeaponInputHandler>();
                    if (handler != null)
                        handler.Init(targetGrid);
                }
            }
        }

        /// <summary>
        /// Remove the preview building and clear the build targets.
        /// The preview building is destroyed at the end of the frame because InputActions and OnHover events,
        /// can be run in parallel and create race conditions.
        /// </summary>
        protected void ExitBuildMode()
        {
            if (!IsPreviewMode) return;

            OnExitBuildModeEvent?.Invoke();

            _selectedBuildingIndex = -1;
            // Delay destruction to end of frame so any queued events don't break
            if (_previewBuilding != null)
                StartCoroutine(RemovePreviewBuildingEndOfFrame());
        }
        
        private IEnumerator RemovePreviewBuildingEndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            if (_previewBuilding != null)
            {
                Destroy(_previewBuilding);
                _previewBuilding = null;
            }
            ClearPreviousBuildTargets();
        }

        /// <summary>
        /// Handles the rotation of the preview building.
        /// This includes rotating the preview building, and the tiles underneath it.
        /// </summary>
        /// <param name="angle"></param>
        protected void RotatePreviewBuilding(float angle)
        {
            if (!IsPreviewMode || _previewBuilding == null) return;

            _currentPreviewBuildingRotation = (_currentPreviewBuildingRotation + angle) % 360f;
            _previewBuilding.transform.rotation = Quaternion.Euler(0f, _currentPreviewBuildingRotation, 0f) *
                                                  SelectedBuildingTemplate.BuildingPrefab.transform.rotation;
            if (_hoveredHexTile == null) return;

            MovePreviewObject(_hoveredHexTile.HexTileCoordinate);
        }
        
        // Spawning methods

        private static GameObject SpawnLocal(GameObject prefab, Transform parent = null)
        {
            GameObject obj = Instantiate(prefab, parent);
            if (obj.TryGetComponent(out NetworkObject netObj))
                netObj.enabled = false;
            return obj;
        }

        private static GameObject SpawnNetworked(GameObject prefab, Vector3 pos, Quaternion rot, int gridId)
        {
            Transform parent = GameObject.Find("BuildingContainer_Grid_" + gridId).transform;
            return SpawnNetworked(prefab, pos, rot, parent);
        }
        
        private static GameObject SpawnNetworked(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
        {
            if (prefab.GetComponent<NetworkObject>() == null)
            {
                Debug.LogError("Prefab does not have a NetworkObject component.");
                return null;
            }

            GameObject obj = Instantiate(prefab, pos, rot, parent);
            InstanceFinder.ServerManager.Spawn(obj);
            
            return obj;
        }
    }
}