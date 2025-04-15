using UnityEngine;

namespace FortressForge.HexGrid
{
    public interface ITerrainHeightProvider
    {
        float SampleHeight(Vector3 position);
    }
}