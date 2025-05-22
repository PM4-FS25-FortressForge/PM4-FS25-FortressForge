using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildManager
{
    /// <summary>
    /// Handles the visualisation of a single previewed building.
    /// </summary>
    public class PreviewController : MonoBehaviour
    {
        private HexGridManager _hexGridManager;
        private GameStartConfiguration _config;
        private HexTileHoverController _hexTileHoverController;
        
        private BaseBuildingTemplate _selectedBuildingTemplate;
        private GameObject _previewBuilding;
        private MeshRenderer _previewBuildingMeshRenderer;
        private float _currentPreviewBuildingRotation;
        private readonly List<HexTileCoordinate> _currentBuildTargetTiles = new();
        
        private HexTileData _hoveredHexTile;
        
        public bool IsPreviewMode => _previewBuilding != null;
        public HexTileData HoveredHexTile => _hoveredHexTile;
        public float CurrentPreviewBuildingRotation => _currentPreviewBuildingRotation;
        
        public void Init(GameStartConfiguration config,
            HexGridManager hexGridManager, HexTileHoverController hoverController)
        {
            _config = config;
            _hexGridManager = hexGridManager;
            _hexTileHoverController = hoverController;

            _hexTileHoverController.OnHoverTileChanged += OnHexTileChanged;
        }

        private void OnDestroy()
        {
            _hexTileHoverController.OnHoverTileChanged -= OnHexTileChanged;
        }

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

        /// <summary>
        /// Handles selection of the building item. By showing the preview building and marking the tiles
        /// on the targeted tile as build targets.
        /// </summary>
        /// <param name="buildingIndex"></param>
        public void PreviewBuilding(BaseBuildingTemplate buildingTemplate)
        {
            if (_previewBuilding != null)
                Destroy(_previewBuilding);

            _selectedBuildingTemplate = buildingTemplate;
            _previewBuilding = SpawnLocal(buildingTemplate.BuildingPrefab);
            _previewBuildingMeshRenderer = _previewBuilding.GetComponentInChildren<MeshRenderer>();
            _currentPreviewBuildingRotation = 0f;
            
            if (_previewBuildingMeshRenderer == null)
            {
                Debug.LogError("Preview building prefab does not have a MeshRenderer component.");
                return;
            }
            
            var collider = _previewBuilding.GetComponentInChildren<Collider>();
            if (collider != null)
                collider.enabled = false;
            
            // Place the preview building and rotate it correctly.
            if (_hoveredHexTile != null)
                MovePreviewObject(_hoveredHexTile.HexTileCoordinate);
        }

        /// <summary>
        /// Handles the rotation of the preview building.
        /// This includes rotating the preview building, and the tiles underneath it.
        /// </summary>
        /// <param name="angle"></param>
        public void RotatePreviewBuilding(float angle)
        {
            if (!IsPreviewMode || _previewBuilding == null) return;

            _currentPreviewBuildingRotation = (_currentPreviewBuildingRotation + angle) % 360f;
            _previewBuilding.transform.rotation = Quaternion.Euler(0f, _currentPreviewBuildingRotation, 0f) * 
                                                  _selectedBuildingTemplate.BuildingPrefab.transform.rotation;
            if (_hoveredHexTile == null) return;

            MovePreviewObject(_hoveredHexTile.HexTileCoordinate);
        }
        
        public IEnumerator RemovePreviewBuildingEndOfFrame()
        {
            if (_previewBuilding == null) yield break;
            
            yield return new WaitForEndOfFrame();

            if (_previewBuilding != null)
            {
                Destroy(_previewBuilding);
                _previewBuilding = null;
            }
            ClearPreviousBuildTargets();
        }

        private void ClearPreviousBuildTargets()
        {
            foreach (HexTileCoordinate coord in _currentBuildTargetTiles)
            {
                var tile = _hexGridManager.GetHexTileDataOrCreate(coord);
                if (tile == null) continue;

                tile.IsBuildTarget = false;
            }

            _currentBuildTargetTiles.Clear();
            _previewBuildingMeshRenderer.enabled = false;
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
                    _currentBuildTargetTiles.Add(worldCoord);
                }
            }
        }

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
            List<HexTileCoordinate> rotatedShape = HexTileHelper.GetRotatedShape(_selectedBuildingTemplate.ShapeData, 
                _currentPreviewBuildingRotation);
            Vector3 avgPos = HexTileHelper.GetAveragePosition(rotatedShape, _config.TileSize, _config.TileHeight);

            _previewBuilding.transform.position = snappedPos + avgPos;
            _previewBuildingMeshRenderer.enabled = true;
            MarkNewTilesAsBuildTargets(targetCoord, rotatedShape);
        }

        private static GameObject SpawnLocal(GameObject prefab, Transform parent = null)
        {
            GameObject obj = Instantiate(prefab, parent);
            if (obj.TryGetComponent(out NetworkObject netObj))
                netObj.enabled = false;
            return obj;
        }
    }
}