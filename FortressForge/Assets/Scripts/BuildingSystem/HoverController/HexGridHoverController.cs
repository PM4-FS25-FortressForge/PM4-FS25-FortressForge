using FortressForge.HexGrid.View;
using FortressForge.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace FortressForge.BuildingSystem.HoverController
{
    public class HexGridHoverController : MonoBehaviour
    {
        [CanBeNull] public HexTileView CurrentlyHoveredTile { get; set; }

        [SerializeField] private float _raycastDistance = 3000;

        private UIClickChecker _clickChecker;

        public void OnEnable()
        {
            _clickChecker = new UIClickChecker();
        }

        private void Update()
        {
            if (_clickChecker.IsClickOnOverlay())
            {
                ClearHoveredTile();
                return;
            }

            if (Camera.main is null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, _raycastDistance))
            {
                ClearHoveredTile();
                return;
            }

            HexTileView hitTileView = hit.collider.GetComponentInParent<HexTileView>();

            if (hitTileView is null)
            {
                ClearHoveredTile();
                return;
            }

            if (hitTileView != CurrentlyHoveredTile)
            {
                if (CurrentlyHoveredTile is not null)
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
            if (CurrentlyHoveredTile is not null)
            {
                CurrentlyHoveredTile.TileData.IsMouseTarget = false;
            }
        }
    }
}