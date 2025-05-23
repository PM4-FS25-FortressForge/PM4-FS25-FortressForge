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
        private PreviewController _previewController;

        private int _selectedBuildingIndex = -1;
        private BaseBuildingTemplate SelectedBuildingTemplate => AvailableBuildings[_selectedBuildingIndex];
        private List<BaseBuildingTemplate> AvailableBuildings => _config.availableBuildings;

        private BuildActions _input;
        private HexTileHoverController _hexTileHoverController;

        public static event Action OnExitBuildModeEvent;

        public void Init(List<HexGridData> ownedHexGridData, GameStartConfiguration config,
            HexGridManager hexGridManager, HexTileHoverController hoverController,
            PreviewController previewController)
        {
            _ownedHexGridDatas = ownedHexGridData;
            _config = config;
            _hexGridManager = hexGridManager;
            _hexTileHoverController = hoverController;
            _previewController = previewController;
        }

        /// <summary>
        /// Handles selection of the building item. By showing the preview building and marking the tiles
        /// on the targeted tile as build targets.
        /// </summary>
        /// <param name="buildingIndex"></param>
        public void PreviewSelectedBuilding(int buildingIndex)
        {
            if (!IsOwner) return;

            _selectedBuildingIndex = buildingIndex;
            
            _previewController.PreviewBuilding(SelectedBuildingTemplate);
        }

        #region Input Callbacks

        private void OnDestroy()
        {
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
            if (context.performed && _previewController.IsPreviewMode)
            {
                var coord = _previewController.HoveredHexTile.HexTileCoordinate;
                TryPlaceBuildingServerRpc(_selectedBuildingIndex, coord, _previewController.CurrentPreviewBuildingRotation);
            }
        }

        public void OnExitBuildMode(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && _previewController.IsPreviewMode)
                ExitBuildMode();
        }

        public void OnRotateBuilding(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && _previewController.IsPreviewMode)
                _previewController.RotatePreviewBuilding(60f);
        }

        #endregion

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
            if (!_previewController.IsPreviewMode) return;

            OnExitBuildModeEvent?.Invoke();

            _selectedBuildingIndex = -1;
            // Delay destruction to end of frame so any queued events don't break
            StartCoroutine(_previewController.RemovePreviewBuildingEndOfFrame());
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