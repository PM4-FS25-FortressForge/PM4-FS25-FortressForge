using FortressForge.GameInitialization;
using FortressForge.HexGrid.View;
using FortressForge.UI;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildManager {
    /// <summary>
    /// Represents the visual representation of a building.
    /// Unity can't handle generic MonoBehaviours, so we need to create a concrete class.
    /// </summary>
    public class BuildingView : GameObjectView<BuildingData>
    {
    }
}