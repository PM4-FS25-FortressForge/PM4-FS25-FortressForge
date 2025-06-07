using UnityEngine;

namespace FortressForge.HexGrid
{
    /// <summary>
    /// Provides methods for sampling terrain height and converting world positions to hex tile coordinates.
    /// </summary>
    public interface ITerrainHeightProvider
    {
        float SampleHeight(Vector3 position);
        float SampleHexHeight(Vector3 position, float tileHeight, float tileSize);
        HexTileCoordinate GetHexTileCoordinate(Vector3 position, float tileHeight, float tileRadius);
    }
}