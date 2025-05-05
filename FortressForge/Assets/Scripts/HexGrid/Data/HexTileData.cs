using System;
using FortressForge.HexGrid.View;
using UnityEngine;

namespace FortressForge.HexGrid.Data
{
    /// <summary>
    /// Represents the data for a hexagonal tile in the grid.
    /// </summary>
    public class HexTileData : ISelectableGameObjectData<HexTileData>
    {
        public event Action<HexTileData> OnChanged;
        public event Action<HexTileData> OnHoverChanged;
        
        private bool _isOccupied;
        public bool IsOccupied
        {
            get => _isOccupied;
            set
            {
                if (_isOccupied != value)
                {
                    _isOccupied = value;
                    OnChanged?.Invoke(this);
                }
            }
        }

        private bool _isBuildTarget;
        public bool IsBuildTarget
        {
            get => _isBuildTarget;
            set
            {
                if (_isBuildTarget != value)
                {
                    _isBuildTarget = value;
                    OnChanged?.Invoke(this);
                }
            }
        }

        private bool _isMouseTarget;
        public bool IsMouseTarget
        {
            get => _isMouseTarget;
            set
            {
                if (_isMouseTarget != value)
                {
                    _isMouseTarget = value;
                    OnChanged?.Invoke(this);
                    OnHoverChanged?.Invoke(this);
                }
            }
        }

        public HexTileCoordinate HexTileCoordinate { get; }

        public HexTileData(HexTileCoordinate hexTileCoordinate)
        {
            HexTileCoordinate = hexTileCoordinate;
            _isOccupied = false;
        }
    }
}