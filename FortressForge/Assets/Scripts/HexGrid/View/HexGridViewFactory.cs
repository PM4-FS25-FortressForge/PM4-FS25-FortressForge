using FortressForge.BuildingSystem;
using FortressForge.HexGrid.Data;
using UnityEngine;

namespace FortressForge.HexGrid.View
{
    public class HexGridViewFactory : IHexGridViewFactory
    {
        /// <summary>
        /// Erstellt das benötigte GameObject, HexGridView und optional andere Komponenten
        /// wie HoverController und verknüpft es mit dem übergebenen Datenmodell.
        /// </summary>
        public HexGridView CreateView(HexGridData gridData, GameObject tilePrefab)
        {
            GameObject go = new GameObject("HexGridView_" + gridData.Id);
            go.AddComponent<HexGridHoverController>(); // TODO : Nur, wenn für diese View nötig
            
            HexGridView hexGridView = go.AddComponent<HexGridView>();
            hexGridView.Initialize(tilePrefab, gridData);

            return hexGridView;
        }
    }
}