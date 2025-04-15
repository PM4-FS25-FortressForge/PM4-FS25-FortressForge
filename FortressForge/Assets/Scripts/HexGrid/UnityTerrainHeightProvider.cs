using UnityEngine;

namespace FortressForge.HexGrid
{
    public class UnityTerrainHeightProvider : ITerrainHeightProvider
    {
        public float SampleHeight(Vector3 position)
        {
            if (Terrain.activeTerrain == null) return 0f;
            return Terrain.activeTerrain.SampleHeight(position);
        }
    }
}