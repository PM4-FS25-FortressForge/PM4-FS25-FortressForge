using UnityEngine;

namespace FortressForge.HexGrid.Data
{
    public class HexGridDataFactory : IHexGridDataFactory
    {
        private readonly ITerrainHeightProvider _terrainHeightProvider;

        public HexGridDataFactory(ITerrainHeightProvider terrainHeightProvider)
        {
            _terrainHeightProvider = terrainHeightProvider;
        }

        public HexGridData CreateData(int id, Vector3 origin, int radius, float tileSize, float tileHeight)
        {
            return new HexGridData(id, origin, radius, tileSize, tileHeight, _terrainHeightProvider);
        }
    }
}