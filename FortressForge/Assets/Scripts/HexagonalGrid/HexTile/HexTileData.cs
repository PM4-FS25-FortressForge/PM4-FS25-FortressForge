using System;

namespace FortressForge.HexGrid.HexTile
{
    public class HexTileData
    {
        public event Action OnChanged;
        
        private bool _isHovered;
        public bool IsHovered
        {
            get => _isOccupied;
            set
            {
                if (_isOccupied != value)
                {
                    _isOccupied = value;
                    OnChanged?.Invoke();
                }
            }
        }

        private bool _isOccupied;
        public bool IsOccupied
        {
            get => _isOccupied;
            set
            {
                if (_isOccupied != value)
                {
                    _isOccupied = value;
                    OnChanged?.Invoke();
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
                    OnChanged?.Invoke();
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
                    OnChanged?.Invoke();
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