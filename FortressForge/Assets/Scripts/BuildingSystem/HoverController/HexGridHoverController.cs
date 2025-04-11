using FortressForge.BuildingSystem.HexGrid;
using UnityEngine;

namespace FortressForge.BuildingSystem.HoverController
{
    public class HexGridHoverController : MonoBehaviour
    {
        [SerializeField] private float raycastDistance = 1000f;

        public HexTileView CurrentlyHoveredTile { get; set; }

        private void Update()
        {
            if (Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance)) // TODO combine with other raycast method in HexGrid
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
                    CurrentlyHoveredTile.UpdateVisuals(false);
                
                hitTileView.UpdateVisuals(true);
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
                CurrentlyHoveredTile.UpdateVisuals(false);
                CurrentlyHoveredTile = null;
            }
        }
    }
}
