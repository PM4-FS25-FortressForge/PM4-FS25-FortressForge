using System;
using FortressForge.GameInitialization;
using FortressForge.HexGrid.Data;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace FortressForge.HexGrid
{
    /// <summary>
    /// Holds the information about the currently hovered tile and manages hover state updates.
    /// </summary>
    public class HexTileHoverController : MonoBehaviour
    {
        [CanBeNull] private HexTileData _currentlyHoveredTile;
        private HexGridManager _hexGridManager;
        private GameStartConfiguration _gameStartConfiguration;
        private Camera _mainCamera;
        private ITerrainHeightProvider _terrainHeightProvider;
        private bool _isInitialized;
        
        /// <summary>
        /// Event fired when the hovered tile changes.
        /// </summary>
        public event Action<HexTileData> OnHoverTileChanged;

        /// <summary>
        /// Initializes the hover controller with required dependencies.
        /// </summary>
        /// <param name="terrainHeightProvider">Provider for terrain height sampling.</param>
        /// <param name="hexGridManager">Reference to the hex grid manager.</param>
        /// <param name="gameStartConfiguration">Game start configuration data.</param>
        public void Init(ITerrainHeightProvider terrainHeightProvider, HexGridManager hexGridManager, GameStartConfiguration gameStartConfiguration)
        {
            _hexGridManager = hexGridManager;
            _gameStartConfiguration = gameStartConfiguration;
            _terrainHeightProvider = terrainHeightProvider;
            _mainCamera = Camera.main;
            _hexGridManager.OnHoverTileChanged += HoverTileChanged;
            _isInitialized = true;
        }

        /// <summary>
        /// Unity Update callback. Checks and updates the currently hovered tile.
        /// </summary>
        public void Update()
        {
            if (!_isInitialized) return;
            
            if (_currentlyHoveredTile == null)
            {
                // We get the currently hovered tile so it gets spawned. 
                // We dont attach it to the _currentlyHoveredTile yet, because we want the tile to trigger this via onHoverTileChanged
                GetCurrentlyHoveredTile();
            }
        }

        /// <summary>
        /// Gets the currently hovered tile, or determines it from the mouse position if not set.
        /// </summary>
        /// <returns>The currently hovered <see cref="HexTileData"/>, or null if none.</returns>
        [CanBeNull]
        public HexTileData GetCurrentlyHoveredTile()
        {
            if (_currentlyHoveredTile != null)
            {
                return _currentlyHoveredTile;
            }
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit)) 
                return null;
            
            var tileSize = _gameStartConfiguration.TileSize;
            var tileHeight = _gameStartConfiguration.TileHeight;
            
            Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, new HexTileCoordinate(tileSize, tileHeight, hit.point).GetWorldPosition(tileSize,tileHeight).y, 0));
            if (!horizontalPlane.Raycast(ray, out float enter)) 
                return null;
            
            HexTileCoordinate coord = _terrainHeightProvider.GetHexTileCoordinate(ray.GetPoint(enter), tileHeight, tileSize);
            
            var newHexTile = _hexGridManager.GetHexTileDataOrCreate(coord);
            
            return newHexTile;
        }

        /// <summary>
        /// Unity callback when the object is disabled. Unsubscribes from events.
        /// </summary>
        private void OnDisable()
        {
            if (_hexGridManager != null)
            {
                _hexGridManager.OnHoverTileChanged -= HoverTileChanged;
            }
        }

        /// <summary>
        /// Handles changes to the hovered tile and fires the <see cref="OnHoverTileChanged"/> event.
        /// </summary>
        /// <param name="hexTileData">The new hovered tile data.</param>
        private void HoverTileChanged(HexTileData hexTileData)
        {
            _currentlyHoveredTile = hexTileData.IsMouseTarget ? hexTileData : null;
            OnHoverTileChanged?.Invoke(_currentlyHoveredTile);
        }
    }
}