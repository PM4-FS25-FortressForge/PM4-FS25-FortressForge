using System;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem;
using FortressForge.HexGrid.Data;

namespace FortressForge.HexGrid.View
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
        /// Handles setup when a new tile is created.
        /// </summary>
        private void HandleNewTileCreated(HexTileData hexData, HexTileCoordinate newCoords)
        {
            InitializeTile(hexData, newCoords);
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

                InitializeTile(data, coords);
            }
        }

        /// <summary>
        /// Initializes the HexTileView script (if present) and stores it in the dictionary.
        /// </summary>
        private void InitializeTile(HexTileData tileData, HexTileCoordinate coord)
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
            }
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