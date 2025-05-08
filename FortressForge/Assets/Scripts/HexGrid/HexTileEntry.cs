namespace FortressForge.HexGrid
{
    [System.Serializable]
    public class HexTileEntry
    {
        public HexTileCoordinate Coordinate;
        public bool IsStackable = true;

        public HexTileEntry(HexTileCoordinate coordinate, bool isStackable)
        {
            Coordinate = coordinate;
            IsStackable = isStackable;
        }
    }
}