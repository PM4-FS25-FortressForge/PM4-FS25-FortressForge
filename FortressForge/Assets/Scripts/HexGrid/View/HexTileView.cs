using System;
using FortressForge.HexGrid.Data;
using UnityEngine;

namespace FortressForge.HexGrid.View
{
    /// <summary>
    /// Represents the visual representation of a HexTile.
    /// </summary>
    public class HexTileView : MonoBehaviour
    {
        public HexTileCoordinate HexTileCoordinate { get; set; }
        
        private HexTileData _tileData;
        public HexTileData TileData => _tileData;

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
            UpdateVisuals(data);
        }

        /// <summary>
        /// Changes the material of the HexTileView based on the IsOccupied property of the HexTileData.
        /// </summary>
        private void UpdateVisuals(HexTileData hexTileData)
        { // TODO maybe add custom colors for overlapping events such as build target and occupied
            if (hexTileData.IsBuildTarget) 
                _renderer.material = HighlightMaterial;
            else if (hexTileData.IsMouseTarget)
                _renderer.material = HighlightMaterial;
            else if (hexTileData.IsOccupied)
                _renderer.material = OccupiedMaterial;
            else
                _renderer.material = FreeMaterial;
        }

        private void OnMouseEnter()
        {
            TileData.IsMouseTarget = true;
        }
        
        private void OnMouseExit()
        {
            TileData.IsMouseTarget = false;
        }
    }
}