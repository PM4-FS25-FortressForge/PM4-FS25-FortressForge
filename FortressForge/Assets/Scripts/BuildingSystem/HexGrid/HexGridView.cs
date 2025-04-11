using System;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.HexTile;
using FortressForge.BuildingSystem.HoverController;

namespace FortressForge.BuildingSystem.HexGrid
{
    /// <summary>
    /// A MonoBehaviour that takes data from a HexGrid and
    /// creates the visual representation in the scene.
    /// </summary>
    public class HexGridView : MonoBehaviour
    {
        private HexGridData _hexGrid;
        private GameObject _tilePrefab;
        private readonly Dictionary<HexTileCoordinate, HexTileView> _tileViews = new ();
        private HexTileView _currentlyHoveredTile;

        /// <summary>
        /// Creates a new HexTileView for each tile in the HexGrid and initializes the visuals.
        /// </summary>
        public void Initialize(GameObject tilePrefab, HexGridData hexGrid)
        {
            _tilePrefab = tilePrefab;
            _hexGrid = hexGrid;
            _hexGrid.OnNewTileCreated += HandleNewTileCreated;

            if (_hexGrid == null || _tilePrefab == null)
                throw new InvalidOperationException("HexGridView: Invalid reference to HexGrid or TilePrefab.");
            
            InitializeHexGridView();
        }

        /// <summary>
        /// Updates the visuals of all HexTiles in the HexGrid.
        /// </summary>
        public void UpdateHexGridView()
        {
            foreach (var kvp in _hexGrid.TileMap)
            {
                var tileCoords = kvp.Key;
                HexTileView hexTileView = _tileViews[tileCoords];
                hexTileView.UpdateVisuals();

                bool canRender = ShouldRenderTile(tileCoords);
                SetTileVisibility(hexTileView, canRender);
            }
        }
        
        /// <summary>
        /// Handles setup when a new tile is created.
        /// </summary>
        private void HandleNewTileCreated(HexTileData hexData, HexTileCoordinate newCoords)
        {
            InitializeTile(hexData, newCoords);
            UpdateHexGridView();
        }

        /// <summary>
        /// Initializes the HexTileViews for all tiles in the HexGrid.  
        /// </summary>
        private void InitializeHexGridView()
        {
            foreach (var kvp in _hexGrid.TileMap)
            {
                var coords = kvp.Key;
                var data = kvp.Value;

                HexTileView hexTileView = InitializeTile(data, coords);
                _tileViews[coords] = hexTileView;
            }
            UpdateHexGridView();
        }

        /// <summary>
        /// Activates/Deactivates the Renderer and Collider to prevent
        /// an invisible tile from being drawn or blocking raycasts.
        /// </summary>
        private void SetTileVisibility(HexTileView tileObj, bool canRender)
        {
            Renderer rend = tileObj.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.enabled = canRender;
            }
            
            Collider col = tileObj.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = canRender;
            }
        }

        /// <summary>
        /// Initializes the HexTileView script (if present) and stores it in the dictionary.
        /// </summary>
        private HexTileView InitializeTile(HexTileData tileData, HexTileCoordinate coord)
        {
            Vector3 worldPos = coord.GetWorldPosition(_hexGrid.TileRadius, _hexGrid.TileHeight);

            if (!_tileViews.ContainsKey(coord))
            {
                GameObject tileObj = Instantiate(
                    _tilePrefab,
                    worldPos,
                    _tilePrefab.transform.rotation,
                    transform
                );

                HexTileView tileView = tileObj.GetComponent<HexTileView>();
                tileView.Init(tileData);
                _tileViews[coord] = tileView;

                return tileView;
            }
            return _tileViews[coord];
        }

        /// <summary>
        /// Checks if a tile should be rendered at the given coordinates.
        /// It should be rendered if the tile is at ground level (h = 0) or
        /// if the tile below it is occupied.
        /// </summary>
        private bool ShouldRenderTile(HexTileCoordinate coord)
        {
            if (coord.H == 0)
                return true;
            
            var belowCoord = new HexTileCoordinate(coord.Q, coord.R, coord.H - 1);
            if (_hexGrid.TileMap.TryGetValue(belowCoord, out HexTileData belowTileData))
            {
                return belowTileData.IsOccupied;
            }
            
            return false;
        }

        /// <summary>
        /// Returns the coordinate of the currently hovered hex tile.
        /// </summary>
        public HexTileCoordinate GetCurrentlyHoveredHexTileCoordinate()
        {
            if (GetComponent<HexGridHoverController>().CurrentlyHoveredTile != null) // TODO just get a reference to the hover controller
            {
                return GetComponent<HexGridHoverController>().CurrentlyHoveredTile.HexTileCoordinate;
            }
            
            return default; // TODO: Returning default is bad, because its still a valid coordinate, consider returning null, after action changes
        }
    }
}