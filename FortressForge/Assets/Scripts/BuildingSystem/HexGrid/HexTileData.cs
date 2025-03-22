using UnityEngine;

namespace FortressForge.BuildingSystem.HexGrid {
    /// <summary>
    /// Reine Daten pro Feld. Kann beliebig erweitert werden 
    /// (z.B. Terrain-Typ, Einheit, Gebäude, etc.)
    /// </summary>
    public class HexTileData {
        public (int, int, int) hexCoord; // q, r, h
        public bool isOccupied;

        public HexTileData((int, int, int) hexCoord) {
            this.hexCoord = hexCoord;
            isOccupied = false;
        }
    }
}