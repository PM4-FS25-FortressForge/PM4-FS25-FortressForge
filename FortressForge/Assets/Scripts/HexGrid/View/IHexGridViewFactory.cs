using FortressForge.HexGrid.Data;
using UnityEngine;

namespace FortressForge.HexGrid.View
{
    public interface IHexGridViewFactory
    {
        /// <summary>
        /// Erzeugt ein passendes GameObject und das zugehörige HexGridView,
        /// verknüpft es mit dem übergebenen HexGridData und gibt das View zurück.
        /// </summary>
        HexGridView CreateView(HexGridData gridData, GameObject tilePrefab);
    }
}