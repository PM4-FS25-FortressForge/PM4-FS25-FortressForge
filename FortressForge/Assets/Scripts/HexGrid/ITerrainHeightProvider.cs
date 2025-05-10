using UnityEngine;

namespace FortressForge.HexGrid
{
    public interface ITerrainHeightProvider
    {
        float SampleHeight(Vector3 position);
        float SampleHexHeight(Vector3 position, float tileHeight, float tileSize);
        HexTileCoordinate GetHexTileCoordinate(Vector3 position, float tileHeight, float tileSize);
    }
}