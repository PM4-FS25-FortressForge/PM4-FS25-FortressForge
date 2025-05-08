using FortressForge.GameInitialization;
using FortressForge.HexGrid.Data;
using FortressForge.UI;
using UnityEngine;

namespace FortressForge.HexGrid.View {
    /// <summary>
    /// Represents the visual representation of a HexTile.
    /// Unity can't handle generic MonoBehaviours, so we need to create a concrete class.
    /// </summary>
    public class HexTileView : GameObjectView<HexTileData>
    {
    }
}