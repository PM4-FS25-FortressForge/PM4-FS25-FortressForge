using FortressForge.BuildingSystem.HexGrid;
using System.Collections.Generic;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Represents a player in the game.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Gets or sets the list of hex grids the player has access to.
        /// </summary>
        public List<HexGridData> HexGrids { get; set; }
    }
}