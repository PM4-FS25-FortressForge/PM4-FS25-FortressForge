using UnityEngine;

namespace FortressForge.HexGrid.Data
{
    public interface IHexGridDataFactory
    {
        HexGridData CreateData(int id, Vector3 origin, int radius, float tileSize, float tileHeight);
    }
}
