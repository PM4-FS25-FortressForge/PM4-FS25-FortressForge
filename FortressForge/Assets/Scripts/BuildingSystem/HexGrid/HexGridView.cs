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
        private readonly Dictionary<(int, int, int), HexTileView> _tileViews = new ();
        private HexTileView _currentlyHoveredTile;

        /// <summary>
        /// Creates a new HexTileView for each tile in the HexGrid and initializes the visuals.
        /// </summary>
        public void BuildGridView(GameObject tilePrefab, HexGridData hexGrid)
        {
            _tilePrefab = tilePrefab;
            _hexGrid = hexGrid;

            if (_hexGrid == null || _tilePrefab == null)
            {
                throw new InvalidOperationException("HexGridView: Invalid reference to HexGrid or TilePrefab.");
            }

            InitializeHexGridView();
            UpdateHexGridView();
        }

        /// <summary>
        /// Updates the visuals of all HexTiles in the HexGrid.
        /// </summary>
        public void UpdateHexGridView()
        {
            foreach (var kvp in _hexGrid.AllTiles)
            {
                (int q, int r, int h) = kvp.Key;
                HexTileView hexTileView = _tileViews[(q, r, h)];
                hexTileView.UpdateVisuals();

                bool canRender = ShouldRenderTile((q, r, h));
                SetTileVisibility(hexTileView, canRender);
            }
        }

        /// <summary>
        /// Initializes the HexTileViews for all tiles in the HexGrid.  
        /// </summary>
        private void InitializeHexGridView()
        {
            foreach (var kvp in _hexGrid.AllTiles)
            {
                (int q, int r, int h) = kvp.Key;
                HexTileData tileData = kvp.Value;

                HexTileView hexTileView = InitializeTile(tileData, (q, r, h));
                _tileViews[(q, r, h)] = hexTileView;
            }
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
        private HexTileView InitializeTile(HexTileData tileData, (int, int, int) coord)
        {
            Vector3 worldPos = CalculateWorldPosition(coord, _hexGrid.Origin);

            if (!_tileViews.ContainsKey(coord))
            {
                GameObject tileObj = Instantiate(
                    _tilePrefab,
                    worldPos,
                    _tilePrefab.transform.rotation,
                    this.transform
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
        private bool ShouldRenderTile((int q, int r, int h) coord)
        {
            if (coord.h == 0)
                return true;
            
            var belowCoord = (coord.q, coord.r, coord.h - 1);
            if (_hexGrid.AllTiles.TryGetValue(belowCoord, out HexTileData belowTileData))
            {
                return belowTileData.IsOccupied;
            }
            
            return false;
        }

        
        private void Update()
        {
            if (Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                ClearHoveredTile();
                return;
            }
            
            HexTileView hitTileView = hit.collider.GetComponentInParent<HexTileView>();

            if (hitTileView == null)
            {
                ClearHoveredTile();
                return;
            }
            
            if (hitTileView != _currentlyHoveredTile)
            {
                if (_currentlyHoveredTile != null)
                    _currentlyHoveredTile.UpdateVisuals(false);
                
                hitTileView.UpdateVisuals(true);
                _currentlyHoveredTile = hitTileView;
            }
        }

        /// <summary>
        /// Clears the hover effect from the currently hovered tile.
        /// </summary>
        private void ClearHoveredTile()
        {
            if (_currentlyHoveredTile != null)
            {
                _currentlyHoveredTile.UpdateVisuals(false);
                _currentlyHoveredTile = null;
            }
        }

        /// <summary>
        /// Calculates the world position of a tile based on its axial coordinates.
        /// </summary>
        public Vector3 CalculateWorldPosition((int, int, int) coord, Vector3 origin)
        {
            float x = _hexGrid.TileRadius * 3f / 2f * coord.Item1;
            float z = _hexGrid.TileRadius * Mathf.Sqrt(3) * (coord.Item2 + coord.Item1 / 2f);
            return new Vector3(x, coord.Item3 * _hexGrid.TileHeight, z) + origin;
        }

        /// <summary>
        /// 
        /// </summary>
        public (int, int, int) WorldPositionToHexCoord(Vector3 worldPosition)
        {
            // Convert world position to hex grid axial coordinates
            float x = worldPosition.x / (_hexGrid.TileRadius * 3f / 2f);
            float z = worldPosition.z / (_hexGrid.TileRadius * Mathf.Sqrt(3));

            int q = Mathf.RoundToInt(x);
            int r = Mathf.RoundToInt(z - (q / 2f)); // Adjust for hex grid layout

            return (q, r, 0); // Assuming h (height) is 0 for ground-level placement
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