using System;
using FortressForge.GameInitialization;
using FortressForge.HexGrid.Data;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace FortressForge.HexGrid
{
    /// <summary>
    /// Holds the information about the currently hovered tile.
    /// </summary>
    public class HexTileHoverController : MonoBehaviour
    {
        [CanBeNull] private HexTileData _currentlyHoveredTile;
        private HexGridManager _hexGridManager;
        private GameStartConfiguration _gameStartConfiguration;
        private Camera _mainCamera;
        private ITerrainHeightProvider _terrainHeightProvider;
        private bool _isInitialized;

        public void Init(ITerrainHeightProvider terrainHeightProvider, HexGridManager hexGridManager, GameStartConfiguration gameStartConfiguration)
        {
            _hexGridManager = hexGridManager;
            _gameStartConfiguration = gameStartConfiguration;
            _terrainHeightProvider = terrainHeightProvider;
            _mainCamera = Camera.main;
            _hexGridManager.OnHoverTileChanged += OnHoverTileChanged;
            _isInitialized = true;
        }

        public void Update()
        {
            if (!_isInitialized) return;
            
            if (_currentlyHoveredTile == null)
            {
                GetCurrentlyHoveredTile();
            }
        }

        [CanBeNull] // TODO This currently can highlight the wrong tile, make sure its validated first
        public HexTileData GetCurrentlyHoveredTile()
        {
            if (_currentlyHoveredTile != null)
            {
                return _currentlyHoveredTile;
            }
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var tileSize = _gameStartConfiguration.TileSize;
                var tileHeight = _gameStartConfiguration.TileHeight;
                Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, new HexTileCoordinate(tileSize, tileHeight, hit.point).GetWorldPosition(tileSize,tileHeight).y, 0));
                if (horizontalPlane.Raycast(ray, out float enter))
                {
                    HexTileCoordinate coord = _terrainHeightProvider.GetHexTileCoordinate(ray.GetPoint(enter), tileHeight, tileSize);
                    var newHexTile = _hexGridManager.GetHexTileDataOrCreate(coord);
                    return newHexTile;
                }
            }
            return null;
        }

        private void OnDisable()
        {
            if (_hexGridManager != null)
            {
                _hexGridManager.OnHoverTileChanged -= OnHoverTileChanged;
            }
        }

        private void OnHoverTileChanged(HexTileData hexTileData)
        {
            _currentlyHoveredTile = hexTileData.IsMouseTarget ? hexTileData : null;
        }
    }
}