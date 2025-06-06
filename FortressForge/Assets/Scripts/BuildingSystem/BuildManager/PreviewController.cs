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
    /// Handles the visualisation and logic of a single previewed building during build mode.
    /// </summary>
    public class PreviewController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the hex grid manager.
        /// </summary>
        private HexGridManager _hexGridManager;

        /// <summary>
        /// Game start configuration reference.
        /// </summary>
        private GameStartConfiguration _config;

        /// <summary>
        /// Reference to the tile hover controller.
        /// </summary>
        private HexTileHoverController _hexTileHoverController;
        
        /// <summary>
        /// The currently selected building template for preview.
        /// </summary>
        private BaseBuildingTemplate _selectedBuildingTemplate;

        /// <summary>
        /// The current preview building GameObject instance.
        /// </summary>
        private GameObject _previewBuilding;

        /// <summary>
        /// MeshRenderer of the preview building for visual feedback.
        /// </summary>
        private MeshRenderer _previewBuildingMeshRenderer;

        /// <summary>
        /// Current rotation of the preview building in degrees.
        /// </summary>
        private float _currentPreviewBuildingRotation;

        /// <summary>
        /// List of tile coordinates currently marked as build targets.
        /// </summary>
        private readonly List<HexTileCoordinate> _currentBuildTargetTiles = new();
        
        /// <summary>
        /// The currently hovered hex tile under the cursor.
        /// </summary>
        private HexTileData _hoveredHexTile;
        
        /// <summary>
        /// Returns true if a preview building is currently active.
        /// </summary>
        public bool IsPreviewMode => _previewBuilding != null;

        /// <summary>
        /// Gets the currently hovered hex tile.
        /// </summary>
        public HexTileData HoveredHexTile => _hoveredHexTile;

        /// <summary>
        /// Gets the current rotation of the preview building.
        /// </summary>
        public float CurrentPreviewBuildingRotation => _currentPreviewBuildingRotation;
        
        /// <summary>
        /// Initializes the preview controller with required dependencies.
        /// </summary>
        /// <param name="config">Game start configuration.</param>
        /// <param name="hexGridManager">Hex grid manager instance.</param>
        /// <param name="hoverController">Tile hover controller.</param>
        public void Init(GameStartConfiguration config,
            HexGridManager hexGridManager, HexTileHoverController hoverController)
        {
            _config = config;
            _hexGridManager = hexGridManager;
            _hexTileHoverController = hoverController;

            _hexTileHoverController.OnHoverTileChanged += OnHexTileChanged;
        }

        /// <summary>
        /// Unsubscribes from events on destruction.
        /// </summary>
        private void OnDestroy()
        {
            _hexTileHoverController.OnHoverTileChanged -= OnHexTileChanged;
        }

        /// <summary>
        /// Called when the hovered hex tile changes.
        /// Updates the preview building position and build targets.
        /// </summary>
        /// <param name="hexTileData">The new hovered hex tile data.</param>
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
        /// Handles selection of the building item. Shows the preview building and marks the tiles
        /// on the targeted tile as build targets.
        /// </summary>
        /// <param name="buildingTemplate">The building template to preview.</param>
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
        /// Handles the rotation of the preview building and updates the build targets.
        /// </summary>
        /// <param name="angle">The angle in degrees to rotate the preview building.</param>
        public void RotatePreviewBuilding(float angle)
        {
            if (!IsPreviewMode || _previewBuilding == null) return;

            _currentPreviewBuildingRotation = (_currentPreviewBuildingRotation + angle) % 360f;
            _previewBuilding.transform.rotation = Quaternion.Euler(0f, _currentPreviewBuildingRotation, 0f) * 
                                                  _selectedBuildingTemplate.BuildingPrefab.transform.rotation;
            if (_hoveredHexTile == null) return;

            MovePreviewObject(_hoveredHexTile.HexTileCoordinate);
        }
        
        /// <summary>
        /// Removes the preview building at the end of the frame and clears build targets.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
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

        /// <summary>
        /// Clears all previously marked build target tiles and disables the preview mesh renderer.
        /// </summary>
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

        /// <summary>
        /// Marks new tiles as build targets based on the given origin and shape.
        /// </summary>
        /// <param name="origin">The origin coordinate for the building shape.</param>
        /// <param name="shape">The list of shape offsets.</param>
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
        /// removes previous build targets, marks new ones, and updates the prefab position.
        /// </summary>
        /// <param name="targetCoord">The coordinate to move the preview building to.</param>
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

        /// <summary>
        /// Instantiates a local preview building prefab and disables its NetworkObject if present.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <returns>The instantiated GameObject.</returns>
        private static GameObject SpawnLocal(GameObject prefab, Transform parent = null)
        {
            GameObject obj = Instantiate(prefab, parent);
            if (obj.TryGetComponent(out NetworkObject netObj))
                netObj.enabled = false;
            return obj;
        }
    }
}