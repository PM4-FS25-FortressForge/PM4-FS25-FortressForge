using FortressForge.HexGrid;
using FortressForge.HexGrid.HexTile;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FortressForge.HexGrid.HoverController
{
    public class HexGridHoverController : MonoBehaviour
    {
        [SerializeField] private float raycastDistance = 3000;

        public HexTileView CurrentlyHoveredTile { get; set; }

        private void Update()
        {
            if (Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
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
            
            if (hitTileView != CurrentlyHoveredTile)
            {
                if (CurrentlyHoveredTile != null)
                    CurrentlyHoveredTile.TileData.IsMouseTarget = false;
                
                hitTileView.TileData.IsMouseTarget = true;
                CurrentlyHoveredTile = hitTileView;
            }
        }

        /// <summary>
        /// Clears the hover effect from the currently hovered tile.
        /// </summary>
        private void ClearHoveredTile()
        {
            if (CurrentlyHoveredTile != null)
            {
                CurrentlyHoveredTile.TileData.IsMouseTarget = false;
            }
        }
    }
}
