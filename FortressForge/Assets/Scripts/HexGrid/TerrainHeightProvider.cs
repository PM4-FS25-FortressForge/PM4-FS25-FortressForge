using UnityEngine;

namespace FortressForge.HexGrid
{
    public class TerrainHeightProvider : ITerrainHeightProvider
    {
        public float SampleHeight(Vector3 position)
        {
            if (Terrain.activeTerrain == null) return 0f;
            return Terrain.activeTerrain.SampleHeight(position);
        }
        
        /// <summary>
        /// Samples the height of a hex tile at the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="tileHeight"></param>
        /// <param name="tileSize"></param>
        /// <returns>The interpreted y position.</returns>
        public float SampleHexHeight(Vector3 position, float tileHeight, float tileSize)
        {
            float height = SampleHeight(position);
            position.y = height;
            return new HexTileCoordinate(tileSize, tileHeight, position)
                .GetWorldPosition(tileSize, tileHeight).y;
        }
    }
}