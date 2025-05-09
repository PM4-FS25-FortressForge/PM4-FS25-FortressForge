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
        
        // The following properties are used to determine the visual state of the tile.
        private bool _isOccupied;        
        private bool _isBuildTarget;
        private bool _isMouseTarget;
        private bool _isOwned;

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

        public bool IsOwned
        {
            get => _isOwned;
            set
            {
                if (_isOwned != value)
                {
                    _isOwned = value;
                    OnChanged?.Invoke(this);
                }
            }
        }
        
        public HexTileCoordinate HexTileCoordinate { get; }

        public HexTileData(HexTileCoordinate hexTileCoordinate)
        {
            HexTileCoordinate = hexTileCoordinate;
            _isOccupied = false;
            _isOwned = false;
        }
    }
}