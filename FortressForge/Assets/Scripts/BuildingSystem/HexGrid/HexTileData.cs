using UnityEngine;

namespace FortressForge.BuildingSystem.HexGrid
{
    /// <summary>
    /// Pure data per field. Can be extended arbitrarily
    /// (e.g. terrain type, unit, building, etc.)
    /// </summary>
    public class HexTileData
    {
        public (int, int, int) HexCoord; // q, r, h
        public bool IsOccupied;

        public HexTileData((int, int, int) hexCoord)
        {
            this.HexCoord = hexCoord;
            IsOccupied = false;
        }
    }
}