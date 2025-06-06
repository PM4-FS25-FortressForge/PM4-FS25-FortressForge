namespace FortressForge.HexGrid
{
    /// <summary>
    /// Entry for a hex tile with coordinate and stackable flag.
    /// </summary>
    [System.Serializable]
    public class HexTileEntry
    {
        public HexTileCoordinate Coordinate;
        public bool IsStackable = true;

        /// <summary>
        /// Create a new tile entry.
        /// </summary>
        public HexTileEntry(HexTileCoordinate coordinate, bool isStackable)
        {
            Coordinate = coordinate;
            IsStackable = isStackable;
        }
    }
}