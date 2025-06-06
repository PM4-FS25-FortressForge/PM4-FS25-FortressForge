using UnityEngine;

namespace FortressForge.HexGrid
{
    public class TerrainHeightProvider : ITerrainHeightProvider
    {
        private readonly Terrain _terrain;
        
        public TerrainHeightProvider()
        {
            _terrain = Terrain.activeTerrain;
        }
        
        public float SampleHeight(Vector3 position)
        {
            if (!_terrain) return 0f;
            return _terrain.SampleHeight(position);
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
            return GetHexTileCoordinate(position, tileHeight, tileSize)
                .GetWorldPosition(tileSize, tileHeight).y;
        }
        
        /// <summary>
        /// Converts a world position to a hex tile coordinate, using the sampled terrain height.
        /// </summary>
        /// <param name="position">The world position to convert.</param>
        /// <param name="tileHeight">The height of a single hex tile.</param>
        /// <param name="tileRadius">The radius of a single hex tile.</param>
        /// <returns>The corresponding hex tile coordinate.</returns>
        public HexTileCoordinate GetHexTileCoordinate(Vector3 position, float tileHeight, float tileRadius) 
        {
            float height = SampleHeight(position);
            position.y = height;
            return new HexTileCoordinate(tileRadius, tileHeight, position);
        }
    }
}