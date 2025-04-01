using UnityEngine;

namespace FortressForge.BuildingSystem.HexGrid
{
    public static class HexGridFactory
    {
        public static (HexGridData, HexGridView) CreateHexGrid(
            int id,
            Vector3 origin,
            int radius,
            int height,
            float tileSize,
            float tileHeight,
            GameObject tilePrefab)
        {
            HexGridData hexGridData = new HexGridData(id, origin, radius, height, tileSize, tileHeight);
            
            GameObject go = new GameObject("HexGridView");
            HexGridView hexGridView = go.AddComponent<HexGridView>();
            
            hexGridView.BuildGridView(tilePrefab, hexGridData);
            
            return (hexGridData, hexGridView);
        }
    }

}
