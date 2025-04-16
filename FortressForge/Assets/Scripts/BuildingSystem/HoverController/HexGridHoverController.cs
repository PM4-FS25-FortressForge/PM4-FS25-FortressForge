using FortressForge.HexGrid.View;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace FortressForge.BuildingSystem
{
    public class HexGridHoverController : MonoBehaviour
    {
        [CanBeNull] public HexTileView CurrentlyHoveredTile { get; set; }
        
        [SerializeField] private float _raycastDistance = 3000;

        private void Update()
        {
            if (Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (!Physics.Raycast(ray, out RaycastHit hit, _raycastDistance))
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
