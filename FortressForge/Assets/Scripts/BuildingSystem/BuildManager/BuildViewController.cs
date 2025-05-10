using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

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
        private int _selectedBuildingIndex = -1;
        private HexTileData _hoveredHexTile;
        private BaseBuildingTemplate _selectedBuildingTemplate;
        
        private bool IsPreviewMode => _selectedBuildingIndex != -1;
        private List<BaseBuildingTemplate> AvailableBuildings => _config.availableBuildings;

        private BuildActions _input;
        
        public static event Action OnExitBuildModeEvent;

        public void Init(List<HexGridData> hexGridData, GameStartConfiguration config,
            HexGridManager hexGridManager)
        {
            _ownedHexGridDatas = hexGridData;
            _config = config;
            _hexGridManager = hexGridManager;

            _hexGridManager.AllGrids.ForEach(gridData => gridData.OnHoverTileChanged += OnHexTileChanged);
        }

        public void PreviewSelectedBuilding(int buildingIndex)
        {
            if (!IsOwner) return;

            if (_previewBuilding != null)
                Destroy(_previewBuilding);

            _selectedBuildingIndex = buildingIndex;
            _selectedBuildingTemplate = AvailableBuildings[buildingIndex];

            _previewBuilding = SpawnLocal(_selectedBuildingTemplate.BuildingPrefab);
            _previewBuildingMeshRenderer = _previewBuilding.GetComponentInChildren<MeshRenderer>();
            var collider = _previewBuilding.GetComponentInChildren<Collider>();
            if (collider != null)
                collider.enabled = false;
            RotatePreviewBuilding(0);
        }

        #region Input Callbacks

        private void OnHexTileChanged(HexTileData hexTileData)
        {
            if (_previewBuilding == null) return;
            _hoveredHexTile = hexTileData.IsMouseTarget ? hexTileData : null;

            if (_hoveredHexTile == null)
            {
                ClearPreviousBuildTargets();
                return;
            }

            MovePreviewObject(_hoveredHexTile.HexTileCoordinate);
        }

        private void OnDestroy()
        {
            _ownedHexGridDatas.ForEach(gridData => gridData.OnHoverTileChanged -= OnHexTileChanged);
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

        private void MovePreviewObject(HexTileCoordinate targetCoord)
        {
            ClearPreviousBuildTargets();
            Vector3 snappedPos = targetCoord.GetWorldPosition(_config.GridRadius, _config.TileHeight);
            List<HexTileCoordinate> rotatedShape = GetRotatedShape(_selectedBuildingTemplate.ShapeData, _currentPreviewBuildingRotation);
            Vector3 avgPos = GetAveragePosition(rotatedShape);

            _previewBuilding.transform.position = snappedPos + avgPos;
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

            _previewBuildingMeshRenderer.enabled = true;
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
            if (_previewBuildingMeshRenderer)
                _previewBuildingMeshRenderer.enabled = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void TryPlaceBuildingServerRpc(int buildingIndex, HexTileCoordinate coord, float rotation)
        {
            BaseBuildingTemplate template = AvailableBuildings[buildingIndex];
            (var shapeData, var isStackableList) = ExtractShapeInformation(template.ShapeDataEntries);

            List<HexTileCoordinate> rotatedShape = GetRotatedShape(shapeData, rotation);
            List<HexTileCoordinate> globalRotatedShape = rotatedShape.Select(tile => tile + coord).ToList();

            HexGridData targetGrid = _ownedHexGridDatas
                .FirstOrDefault(grid => grid.ValidateBuildingPlacement(coord, rotatedShape));

            if (targetGrid == null || !targetGrid.EconomySystem.CheckForSufficientResources(template.GetBuildCost()))
            {
                Debug.Log("Server: Invalid placement or insufficient resources.");
                return;
            }

            targetGrid.MarkBuildingTiles(coord, rotatedShape, isStackableList);
            targetGrid.EconomySystem.PayResource(template.GetBuildCost());

            Vector3 pos = coord.GetWorldPosition(_config.GridRadius, _config.TileHeight) + GetAveragePosition(rotatedShape);
            Quaternion rot = Quaternion.Euler(0f, rotation, 0f) * template.BuildingPrefab.transform.rotation;
            GameObject prefab =  SpawnNetworked(template.BuildingPrefab, pos, rot);
            
            // Add reference to building manager for later use.
            List<HexTileData> tileDatas = globalRotatedShape
                .Select(coord => targetGrid.TileMap[coord])
                .ToList();
            targetGrid.BuildingManager.AddBuilding(new BuildingData(prefab, tileDatas, template));
            
            SyncPlacedBuildingToClientsRpc(buildingIndex, coord, targetGrid.Id, rotation, prefab);
        }

        [ObserversRpc]
        private void SyncPlacedBuildingToClientsRpc(int buildingIndex, HexTileCoordinate coord, int hexGridId, float rotation,
            GameObject prefab)
        {
            BaseBuildingTemplate template = AvailableBuildings[buildingIndex];
            (var shapeData, var isStackableList) = ExtractShapeInformation(template.ShapeDataEntries);
            HexGridData targetGrid = _hexGridManager.AllGrids[hexGridId];
            List<HexTileCoordinate> rotatedShape = GetRotatedShape(shapeData, rotation);
            List<HexTileCoordinate> globalRotatedShape = rotatedShape.Select(tile => tile + coord).ToList();
            
            // Add local reference to building manager for later use.
            List<HexTileData> tileDatas = globalRotatedShape
                .Select(coord => targetGrid.TileMap[coord])
                .ToList();
            var buildingData = new BuildingData(prefab, tileDatas, template);
            
            var tileData = prefab.AddComponent<BuildingView>();
            tileData.Init(buildingData, _config);
            
            targetGrid.BuildingManager.AddBuilding(buildingData);
            
            _hexGridManager.AllGrids[hexGridId].MarkBuildingTiles(coord, rotatedShape, isStackableList);
        }

        private void ExitBuildMode()
        {
            if (!IsPreviewMode) return;
            
            OnExitBuildModeEvent?.Invoke();

            Destroy(_previewBuilding);
            _selectedBuildingIndex = -1;
            ClearPreviousBuildTargets();
        }

        private void RotatePreviewBuilding(float angle)
        {
            if (!IsPreviewMode || _previewBuilding == null) return;

            _currentPreviewBuildingRotation = (_currentPreviewBuildingRotation + angle) % 360f;
            _previewBuilding.transform.rotation = Quaternion.Euler(0f, _currentPreviewBuildingRotation, 0f) * _selectedBuildingTemplate.BuildingPrefab.transform.rotation;
            if (_hoveredHexTile == null) return;
            
            MovePreviewObject(_hoveredHexTile.HexTileCoordinate);
        }

        private Vector3 GetAveragePosition(List<HexTileCoordinate> hexTileCoordinates)
        {
            Vector3 avg = Vector3.zero;
            foreach (var coord in hexTileCoordinates)
            {
                avg += coord.GetWorldPosition(_config.GridRadius, _config.TileHeight);
            }
            return avg / hexTileCoordinates.Count;
        }

        private static List<HexTileCoordinate> GetRotatedShape(List<HexTileCoordinate> originalShape, float angle)
        {
            int steps = (((int)(-angle / 60)) % 6 + 6) % 6;
            var rotated = new List<HexTileCoordinate>(originalShape.Count);

            foreach (var hex in originalShape)
            {
                int q = hex.Q;
                int r = hex.R;

                for (int i = 0; i < steps; i++)
                {
                    int temp = q;
                    q = -r;
                    r = temp + r;
                }

                rotated.Add(new HexTileCoordinate(q, r, hex.H));
            }

            return rotated;
        }

        private static GameObject SpawnLocal(GameObject prefab, Transform parent = null)
        {
            GameObject obj = Instantiate(prefab, parent);
            if (obj.TryGetComponent(out NetworkObject netObj))
                netObj.enabled = false;
            return obj;
        }

        private GameObject SpawnNetworked(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            if (parent == null)
            {
                int gridId = _ownedHexGridDatas[0].Id;
                parent = GameObject.Find("BuildingContainer_Grid_" + gridId).transform;
            }
            GameObject obj = Instantiate(prefab, pos, rot, parent);
            InstanceFinder.ServerManager.Spawn(obj);
            return obj;
        }
        
        private static (List<HexTileCoordinate> coordinates, List<bool> isStackable) ExtractShapeInformation(List<HexTileEntry> shapeDataEntries)
        {
            List<HexTileCoordinate> coordinates = new List<HexTileCoordinate>();
            List<bool> isStackable = new List<bool>();

            foreach (var entry in shapeDataEntries)
            {
                coordinates.Add(entry.Coordinate);
                isStackable.Add(entry.IsStackable);
            }

            return (coordinates, isStackable);
        }
    }
}
