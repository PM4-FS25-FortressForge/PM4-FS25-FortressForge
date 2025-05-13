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
        
        private bool _isMouseTarget;
        // The following properties are used to determine the visual state of the tile.
        private bool _isOccupied;        
        private bool _isBuildTarget;
        private bool _isHighlighted;
        private bool _isOwned;
        private bool _isInvisible;

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
        
        public bool IsInvisible
        {
            get => _isInvisible;
            set
            {
                if (_isInvisible != value)
                {
                    _isInvisible = value;
                    OnChanged?.Invoke(this);
                }
            }
        }
        
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
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