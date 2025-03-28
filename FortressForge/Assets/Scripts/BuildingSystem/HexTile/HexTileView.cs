using UnityEngine;

namespace FortressForge.BuildingSystem.HexGrid
{
    /// <summary>
    /// Represents the visual representation of a HexTile.
    /// </summary>
    public class HexTileView : MonoBehaviour
    {
        private HexTileData _tileCoordinates;

        public Material FreeMaterial;
        public Material OccupiedMaterial;
        public Material HighlightMaterial;

        private MeshRenderer _renderer;

        /// <summary>
        /// Initializes the HexTileView with the given HexTileData.
        /// </summary>
        /// <param name="data"></param>
        public void Init(HexTileData data)
        {
            _tileCoordinates = data;
            _renderer = GetComponentInChildren<MeshRenderer>(); // TODO: Check if we can remove this GetComponent call and not use MonoBehaviour
            UpdateVisuals();
        }

        /// <summary>
        /// Changes the material of the HexTileView based on the IsOccupied property of the HexTileData.
        /// </summary>
        public void UpdateVisuals()
        {
            _renderer.material = _tileCoordinates.IsOccupied ? OccupiedMaterial : FreeMaterial;
        }

        /// <summary>
        /// Changes the material of the HexTileView based on the IsOccupied property of the HexTileData.
        /// </summary>
        /// <param name="highlight">Whether the tile should be highlighted for a hover effect.</param>
        public void UpdateVisuals(bool highlight)
        {
            if (highlight)
                _renderer.material = HighlightMaterial;
            else
            {
                _renderer.material = _tileCoordinates.IsOccupied ? OccupiedMaterial : FreeMaterial; // TODO: Throws NullReferenceException in some cases check out why
            }
        }
    }
}