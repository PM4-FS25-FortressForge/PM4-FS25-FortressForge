using System;
using UnityEngine;
using System.Collections.Generic;

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
        private readonly Dictionary<HexTileCoordinates, HexTileView> _tileViews = new ();
        private HexTileView _currentlyHoveredTile;

        /// <summary>
        /// Creates a new HexTileView for each tile in the HexGrid and initializes the visuals.
        /// </summary>
        public void Initialize(GameObject tilePrefab, HexGridData hexGrid)
        {
            _tilePrefab = tilePrefab;
            _hexGrid = hexGrid;

            if (_hexGrid == null || _tilePrefab == null)
                throw new InvalidOperationException("HexGridView: Invalid reference to HexGrid or TilePrefab.");
            
            InitializeHexGridView2();
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
        /// Initializes the HexTileViews for all tiles in the HexGrid.  
        /// </summary>
        private void InitializeHexGridView2()
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
        private HexTileView InitializeTile(HexTileData tileData, HexTileCoordinates coord)
        {
            Vector3 worldPos = CalculateWorldPosition(coord, _hexGrid.Origin);

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
        private bool ShouldRenderTile(HexTileCoordinates coord)
        {
            if (coord.H == 0)
                return true;
            
            var belowCoord = new HexTileCoordinates(coord.Q, coord.R, coord.H - 1);
            if (_hexGrid.TileMap.TryGetValue(belowCoord, out HexTileData belowTileData))
            {
                return belowTileData.IsOccupied;
            }
            
            return false;
        }

        /// <summary>
        /// Calculates the world position of a tile based on its axial coordinates.
        /// </summary>
        public Vector3 CalculateWorldPosition(HexTileCoordinates coord, Vector3 origin)
        {
            float x = _hexGrid.TileRadius * 3f / 2f * coord.Q;
            float z = _hexGrid.TileRadius * Mathf.Sqrt(3) * (coord.R + coord.Q / 2f);
            return new Vector3(x, coord.H * _hexGrid.TileHeight, z) + origin;
        }

        /// <summary>
        /// 
        /// </summary>
        public HexTileCoordinates WorldPositionToHexCoord(Vector3 worldPosition) // TODO make sure to use custom axial coordinates
        { 
            // Convert world position to hex grid axial coordinates
            float x = worldPosition.x / (_hexGrid.TileRadius * 3f / 2f); // TODO throws exception regularly
            float z = worldPosition.z / (_hexGrid.TileRadius * Mathf.Sqrt(3));

            int q = Mathf.RoundToInt(x);
            int r = Mathf.RoundToInt(z - (q / 2f)); // Adjust for hex grid layout

            return new HexTileCoordinates(q, r, 0); // Assuming h (height) is 0 for ground-level placement
        }

        public Vector3 GetMouseWorldPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.point;
            }
            return Vector3.zero;
        }
    }
}