using UnityEngine;

namespace FortressForge.HexGrid.HexTile
{
    /// <summary>
    /// Represents the visual representation of a HexTile.
    /// </summary>
    public class HexTileView : MonoBehaviour
    {
        public HexTileCoordinate HexTileCoordinate { get; set; }
        
        private HexTileData _tileData;

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
            HexTileCoordinate = data.HexTileCoordinate;
            _tileData = data;
            _tileData.OnChanged += UpdateVisuals;
            _renderer = GetComponentInChildren<MeshRenderer>(); 
            UpdateVisuals();
        }

        /// <summary>
        /// Changes the material of the HexTileView based on the IsOccupied property of the HexTileData.
        /// </summary>
        public void UpdateVisuals()
        { // TODO maybe add custom colors for overlapping events such as build target and occupied
            if (_tileData.IsBuildTarget) 
                _renderer.material = HighlightMaterial;
            else if (_tileData.IsMouseTarget)
                _renderer.material = HighlightMaterial;
            else if (_tileData.IsOccupied)
                _renderer.material = OccupiedMaterial;
            else
                _renderer.material = FreeMaterial;
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
                UpdateVisuals(); // TODO: Throws NullReferenceException in some cases check out why
            }
        }
    }
}