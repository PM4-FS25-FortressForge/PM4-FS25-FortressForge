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
    /// <summary>
    /// Controls the building placement view and input handling in build mode.
    /// Handles previewing, rotating, and placing buildings, and synchronizes placement across the network.
    /// </summary>
    public class BuildViewController : NetworkBehaviour, BuildActions.IPreviewModeActions
    {
        /// <summary>
        /// List of hex grids owned by the player.
        /// </summary>
        private List<HexGridData> _ownedHexGridDatas = new();

        /// <summary>
        /// Game start configuration reference.
        /// </summary>
        private GameStartConfiguration _config;

        /// <summary>
        /// Reference to the hex grid manager.
        /// </summary>
        private HexGridManager _hexGridManager;

        /// <summary>
        /// Reference to the preview controller for building placement.
        /// </summary>
        private PreviewController _previewController;

        /// <summary>
        /// Index of the currently selected building.
        /// </summary>
        private int _selectedBuildingIndex = -1;

        /// <summary>
        /// Gets the currently selected building template.
        /// </summary>
        private BaseBuildingTemplate SelectedBuildingTemplate => AvailableBuildings[_selectedBuildingIndex];

        /// <summary>
        /// Gets the list of available building templates.
        /// </summary>
        private List<BaseBuildingTemplate> AvailableBuildings => _config.availableBuildings;

        /// <summary>
        /// Input actions for build mode.
        /// </summary>
        private BuildActions _input;

        /// <summary>
        /// Reference to the tile hover controller.
        /// </summary>
        private HexTileHoverController _hexTileHoverController;

        /// <summary>
        /// Event triggered when exiting build mode.
        /// </summary>
        public static event Action OnExitBuildModeEvent;

        /// <summary>
        /// Initializes the build view controller with required dependencies.
        /// </summary>
        /// <param name="ownedHexGridData">List of grids owned by the player.</param>
        /// <param name="config">Game start configuration.</param>
        /// <param name="hexGridManager">Hex grid manager instance.</param>
        /// <param name="hoverController">Tile hover controller.</param>
        /// <param name="previewController">Preview controller for building placement.</param>
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
        /// <param name="buildingIndex">Index of the selected building template.</param>
        public void PreviewSelectedBuilding(int buildingIndex)
        {
            if (!IsOwner) return;

            _selectedBuildingIndex = buildingIndex;
            _previewController.PreviewBuilding(SelectedBuildingTemplate);
        }

        #region Input Callbacks

        /// <summary>
        /// Cleans up when the controller is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            ExitBuildMode();
        }

        /// <summary>
        /// Enables input actions and sets callbacks.
        /// </summary>
        private void OnEnable()
        {
            _input = new BuildActions();
            _input.PreviewMode.SetCallbacks(this);
            _input.PreviewMode.Enable();
        }

        /// <summary>
        /// Disables input actions and removes callbacks.
        /// </summary>
        private void OnDisable()
        {
            _input.PreviewMode.Disable();
            _input.PreviewMode.SetCallbacks(null);
        }

        /// <summary>
        /// Handles the place action input. Attempts to place the building if in preview mode.
        /// </summary>
        /// <param name="context">Input action context.</param>
        public void OnPlaceAction(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && _previewController.IsPreviewMode)
            {
                var coord = _previewController.HoveredHexTile.HexTileCoordinate;
                TryPlaceBuildingServerRpc(_selectedBuildingIndex, coord, _previewController.CurrentPreviewBuildingRotation);
            }
        }

        /// <summary>
        /// Handles the exit build mode input.
        /// </summary>
        /// <param name="context">Input action context.</param>
        public void OnExitBuildMode(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed && _previewController.IsPreviewMode)
                ExitBuildMode();
        }

        /// <summary>
        /// Handles the rotate building input.
        /// </summary>
        /// <param name="context">Input action context.</param>
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
        /// <param name="buildingIndex">Index of the building template.</param>
        /// <param name="coord">Target tile coordinate.</param>
        /// <param name="rotation">Rotation of the building.</param>
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
        
        /// <summary>
        /// Synchronizes the placed building to all clients.
        /// Handles local instantiation and initialization of the building view.
        /// </summary>
        /// <param name="buildingIndex">Index of the building template.</param>
        /// <param name="coord">Tile coordinate.</param>
        /// <param name="hexGridId">ID of the hex grid.</param>
        /// <param name="rotation">Rotation of the building.</param>
        /// <param name="prefab">Instantiated building prefab.</param>
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

        /// <summary>
        /// Spawns a networked building prefab at the given position and rotation, under the specified grid parent.
        /// </summary>
        /// <param name="prefab">Prefab to spawn.</param>
        /// <param name="pos">World position.</param>
        /// <param name="rot">Rotation.</param>
        /// <param name="gridId">Grid ID for parenting.</param>
        /// <returns>The spawned GameObject, or null if failed.</returns>
        private static GameObject SpawnNetworked(GameObject prefab, Vector3 pos, Quaternion rot, int gridId)
        {
            Transform parent = GameObject.Find("BuildingContainer_Grid_" + gridId).transform;
            return SpawnNetworked(prefab, pos, rot, parent);
        }
        
        /// <summary>
        /// Spawns a networked building prefab at the given position and rotation, under the specified parent.
        /// </summary>
        /// <param name="prefab">Prefab to spawn.</param>
        /// <param name="pos">World position.</param>
        /// <param name="rot">Rotation.</param>
        /// <param name="parent">Parent transform.</param>
        /// <returns>The spawned GameObject, or null if failed.</returns>
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