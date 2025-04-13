using FortressForge.HexGrid.HoverController;
using UnityEngine;

namespace FortressForge.HexGrid
{
    public static class HexGridFactory
    {
        public static (HexGridData, HexGridView) CreateHexGrid(
            int id,
            Vector3 origin,
            int radius,
            float tileSize,
            float tileHeight,
            GameObject tilePrefab)
        {
            HexGridData hexGridData = new HexGridData(id, origin, radius, tileSize, tileHeight);
            
            GameObject go = new GameObject("HexGridView");
            go.AddComponent<HexGridHoverController>();
            
            HexGridView hexGridView = go.AddComponent<HexGridView>();
            hexGridView.Initialize(tilePrefab, hexGridData);

            return (hexGridData, hexGridView);
        }
    }

}
