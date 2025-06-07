using System;
using FortressForge.GenericElements.Data;
using FortressForge.HexGrid.View;
using UnityEngine;

namespace FortressForge.HexGrid.Data
{
    /// <summary>
    /// Data for a hex tile.
    /// </summary>
    public class HexTileData : ISelectableGameObjectData<HexTileData>
    {
        /// <summary>Fired when tile data changes.</summary>
        public event Action<HexTileData> OnChanged;
        /// <summary>Fired when hover state changes.</summary>
        public event Action<HexTileData> OnHoverChanged;
        /// <summary>Fired on left mouse click.</summary>
        public event Action OnMouseLeftClick;

        private bool _isMouseTarget;
        private bool _isOccupied;
        private bool _isBuildTarget;
        private bool _isHighlighted;
        private bool _isOwned;
        private bool _isInvisible;

        /// <summary>True if tile is occupied.</summary>
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

        /// <summary>True if tile is a build target.</summary>
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

        /// <summary>True if mouse is over tile.</summary>
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

        /// <summary>True if tile is owned.</summary>
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

        /// <summary>True if tile is invisible.</summary>
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

        /// <summary>True if tile is highlighted.</summary>
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

        /// <summary>Tile coordinate.</summary>
        public HexTileCoordinate HexTileCoordinate { get; }

        /// <summary>
        /// Create new tile data.
        /// </summary>
        public HexTileData(HexTileCoordinate hexTileCoordinate)
        {
            HexTileCoordinate = hexTileCoordinate;
            _isOccupied = false;
            _isOwned = false;
        }

        /// <summary>
        /// Call on left mouse click.
        /// </summary>
        public void TriggerMouseLeftClick() {
            OnMouseLeftClick?.Invoke();
        }
    }
}