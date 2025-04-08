using FortressForge.BuildingSystem.HoverController;
using UnityEngine;

namespace FortressForge.BuildingSystem.HexGrid
{
    public static class HexGridFactory
    {
        public static (HexGridData, HexGridView) CreateHexGrid(
            int id,
            Vector3 origin,
            int radius,
            int maxBuildHeight,
            float tileSize,
            float tileHeight,
            GameObject tilePrefab)
        {
            HexGridData hexGridData = new HexGridData(id, origin, radius, maxBuildHeight, tileSize, tileHeight);
            
            GameObject go = new GameObject("HexGridView");
            go.AddComponent<HexGridHoverController>();
            HexGridView hexGridView = go.AddComponent<HexGridView>();
            hexGridView.Initialize(tilePrefab, hexGridData);

            return (hexGridData, hexGridView);
        }
    }

}
