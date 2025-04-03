using FortressForge.BuildingSystem.HexTile;

namespace FortressForge.BuildingSystem.HexGrid
{
    public class HexTileData
    {
        public bool IsOccupied { get; set; }

        public HexTileCoordinate HexTileCoordinate;
        
        public HexTileData(HexTileCoordinate hexTileCoordinate)
        {
            HexTileCoordinate = hexTileCoordinate;
            IsOccupied = false;
        }
    }
}