using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.Economy;
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
        private int _selectedBuildingIndex = -1;
        private HexTileData _hoveredHexTile;
        private BaseBuildingTemplate _selectedBuildingTemplate;
        
        private bool IsPreviewMode => _selectedBuildingIndex != -1;
        private List<BaseBuildingTemplate> AvailableBuildings => _config.availableBuildings;

        private BuildActions _input;

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

        private void Awake() => _input = new BuildActions();

        private void OnEnable()
        {
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
            Vector3 snappedPos = targetCoord.GetWorldPosition(_config.Radius, _config.TileHeight);
            List<HexTileCoordinate> rotatedShape = GetRotatedShape(_selectedBuildingTemplate.ShapeData, _currentPreviewBuildingRotation);
            Vector3 avgPos = GetAveragePosition(rotatedShape);

            _previewBuilding.transform.position = snappedPos + avgPos;
            MarkNewTilesAsBuildTargets(targetCoord, rotatedShape);
        }

        private void MarkNewTilesAsBuildTargets(HexTileCoordinate origin, List<HexTileCoordinate> shape)
        {
            foreach (HexTileCoordinate offset in shape)
            {
                HexTileCoordinate worldCoord = offset + origin;

                foreach (var grid in _ownedHexGridDatas)
                {
                    if (grid.TileMap.TryGetValue(worldCoord, out var tileData))
                    {
                        tileData.IsBuildTarget = true;
                        _currentBuildTargets.Add(worldCoord);
                        break;
                    }
                }
            }

            _previewBuildingMeshRenderer.enabled = true;
        }

        private void ClearPreviousBuildTargets()
        {
            foreach (HexTileCoordinate coord in _currentBuildTargets)
            {
                foreach (var grid in _ownedHexGridDatas)
                {
                    if (grid.TileMap.TryGetValue(coord, out var tileData))
                    {
                        tileData.IsBuildTarget = false;
                        break;
                    }
                }
            }

            _currentBuildTargets.Clear();
            if (_previewBuildingMeshRenderer)
                _previewBuildingMeshRenderer.enabled = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void TryPlaceBuildingServerRpc(int buildingIndex, HexTileCoordinate coord, float rotation)
        {
            BaseBuildingTemplate template = AvailableBuildings[buildingIndex];
            List<HexTileCoordinate> rotatedShape = GetRotatedShape(template.ShapeData, rotation);

            HexGridData targetGrid = _ownedHexGridDatas
                .FirstOrDefault(grid => grid.ValidateBuildingPlacement(coord, rotatedShape));

            if (targetGrid == null || !targetGrid.EconomySystem.CheckForSufficientResources(template.GetBuildCost()))
            {
                Debug.Log("Server: Invalid placement or insufficient resources.");
                return;
            }

            targetGrid.PlaceBuilding(coord, rotatedShape);
            targetGrid.EconomySystem.PayResource(template.GetBuildCost());
            targetGrid.BuildingManager.AddBuilding(template);

            Vector3 pos = coord.GetWorldPosition(_config.Radius, _config.TileHeight) + GetAveragePosition(rotatedShape);
            Quaternion rot = Quaternion.Euler(0f, rotation, 0f) * template.BuildingPrefab.transform.rotation;
            SpawnNetworked(template.BuildingPrefab, pos, rot, transform);

            UpdateGridClientRpc(buildingIndex, coord, targetGrid.Id, rotation);
        }

        [ObserversRpc]
        private void UpdateGridClientRpc(int buildingIndex, HexTileCoordinate coord, int hexGridId, float rotation)
        {
            BaseBuildingTemplate template = AvailableBuildings[buildingIndex];
            List<HexTileCoordinate> rotatedShape = GetRotatedShape(template.ShapeData, rotation);
            _hexGridManager.AllGrids[hexGridId].PlaceBuilding(coord, rotatedShape);
        }

        private void ExitBuildMode()
        {
            if (!IsPreviewMode) return;

            Destroy(_previewBuilding);
            _selectedBuildingIndex = -1;
            ClearPreviousBuildTargets();
        }

        private void RotatePreviewBuilding(float angle)
        {
            if (!IsPreviewMode || _previewBuilding == null) return;

            _currentPreviewBuildingRotation = (_currentPreviewBuildingRotation + angle) % 360f;
            _previewBuilding.transform.rotation = Quaternion.Euler(0f, _currentPreviewBuildingRotation, 0f) * _selectedBuildingTemplate.BuildingPrefab.transform.rotation;

            MovePreviewObject(_hoveredHexTile.HexTileCoordinate);
        }

        private Vector3 GetAveragePosition(List<HexTileCoordinate> hexTileCoordinates)
        {
            Vector3 avg = Vector3.zero;
            foreach (var coord in hexTileCoordinates)
            {
                avg += coord.GetWorldPosition(_config.Radius, _config.TileHeight);
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

        private static GameObject SpawnNetworked(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            GameObject obj = Instantiate(prefab, pos, rot, parent);
            InstanceFinder.ServerManager.Spawn(obj);
            return obj;
        }
    }
}
