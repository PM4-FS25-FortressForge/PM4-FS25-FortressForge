using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.HoverController;
using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Responsible for initializing the game by setting up hex grids for each player.
    /// </summary>
    public class GameInitializer
    {
        /// <summary>
        /// Initializes the game for the given teams.
        /// </summary>
        /// <param name="teams">List of teams to initialize the game for.</param>
        public void InitializeGame(List<Team> teams)
        {
            foreach (var team in teams)
            {
                foreach (var player in team.Players)
                {
                    InitializeHexGridsForPlayer(player);
                }
            }
        }

        /// <summary>
        /// Initializes hex grids for the given player.
        /// </summary>
        /// <param name="player">The player to initialize hex grids for.</param>
        private void InitializeHexGridsForPlayer(Player player)
        {
            player.HexGrids = new List<HexGridData> { new HexGridData(0, new Vector3(), 0, 0, 0, 0) }; // Pass required parameters

            foreach (var hexGrid in player.HexGrids)
            {
                var hexGridHoverController = new GameObject("HexGridHoverController").AddComponent<HexGridHoverController>();
                hexGridHoverController.CurrentlyHoveredTile = null;
            }
        }
    }
}