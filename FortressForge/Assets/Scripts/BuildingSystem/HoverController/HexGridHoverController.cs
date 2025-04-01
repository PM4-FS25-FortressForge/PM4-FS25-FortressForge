using FortressForge.BuildingSystem.HexGrid;
using UnityEngine;

namespace FortressForge.BuildingSystem.HoverController
{
    public class HexGridHoverController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float raycastDistance = 1000f;

        private HexTileView _currentlyHoveredTile;

        private void Update()
        {
            if (mainCamera == null) return;
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
    }
}
