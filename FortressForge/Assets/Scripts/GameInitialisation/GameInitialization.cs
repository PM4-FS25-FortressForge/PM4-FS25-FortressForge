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
        /// <param name="availableBuildings">List of available buildings.</param>
        /// <param name="availableTechnologies">List of available technologies.</param>
        /// <param name="selectedMap">The selected game map.</param>
        public void InitializeGame(List<Team> teams, List<string> availableBuildings, List<string> availableTechnologies, string selectedMap)
        {
            // Set available buildings and technologies
            SetAvailableBuildings(availableBuildings);
            SetAvailableTechnologies(availableTechnologies);
            SelectGameMap(selectedMap);

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
            player.HexGrids = new List<HexGridData> { new HexGridData(0, new Vector3(), 0, 0, 0, 0) };

            foreach (var hexGrid in player.HexGrids)
            {
                var hexGridHoverController = new GameObject("HexGridHoverController").AddComponent<HexGridHoverController>();
                hexGridHoverController.CurrentlyHoveredTile = null;
            }
        }

        /// <summary>
        /// Sets the available buildings for the game.
        /// </summary>
        /// <param name="availableBuildings">List of available buildings.</param>
        private void SetAvailableBuildings(List<string> availableBuildings)
        {
            // Implementation to set available buildings
        }

        /// <summary>
        /// Sets the available technologies for the game.
        /// </summary>
        /// <param name="availableTechnologies">List of available technologies.</param>
        private void SetAvailableTechnologies(List<string> availableTechnologies)
        {
            // Implementation to set available technologies
        }

        /// <summary>
        /// Selects the game map.
        /// </summary>
        /// <param name="selectedMap">The selected game map.</param>
        private void SelectGameMap(string selectedMap)
        {
            // Implementation to select the game map
        }
    }
}