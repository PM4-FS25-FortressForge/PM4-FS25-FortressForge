using FortressForge.BuildingSystem.HexGrid;
using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Responsible for initializing the hex grid system for players at the start of the game.
    /// This includes creating hex grids and assigning players to their respective grids
    /// based on the game start configuration.
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("Player HexGridConfiguration")] [SerializeField]
        private HexGridConfiguration _hexGridConfiguration;

        [Header("Player GameStartConfiguration")] [SerializeField]
        private GameStartConfiguration _gameStartConfiguration;

        private readonly List<(HexGridData data, HexGridView view)> _allGrids = new();
        
        /// <summary>
        /// Initializes the hex grid system for all players by creating the grids
        /// and assigning players to their respective grids.
        /// </summary>
        public void InitializeHexGridForPlayers()
        {
            CreateHexGrids();
            AssignPlayersToHexGrids();
        }

        /// <summary>
        /// Creates hex grids for all players based on the game start configuration
        /// and adds them to the internal list of grids.
        /// </summary>
        private void CreateHexGrids()
        {
            for (int i = 0; i < _gameStartConfiguration.PlayerIdsHexGridIdTuplesList.Count; i++)
            {
                var (data, view) = HexGridFactory.CreateHexGrid(
                    id: i,
                    origin: _gameStartConfiguration.HexGridOrigins[i],
                    radius: _hexGridConfiguration.Radius,
                    maxBuildHight: _hexGridConfiguration.MaxBuildHeight,
                    tileSize: _hexGridConfiguration.TileSize,
                    tileHeight: _hexGridConfiguration.TileHeight,
                    tilePrefab: _hexGridConfiguration.TilePrefab
                );

                _allGrids.Add((data, view));
            }
        }

        /// <summary>
        /// Assigns players to their respective hex grids based on the game start configuration.
        /// </summary>
        private void AssignPlayersToHexGrids()
        {
            for (int i = 0; i < _gameStartConfiguration.PlayerIdsHexGridIdTuplesList.Count; i++)
            {
                var playerId = _gameStartConfiguration.PlayerIdsHexGridIdTuplesList[i].PlayerId;
                var hexGridId = _gameStartConfiguration.PlayerIdsHexGridIdTuplesList[i].HexGridId;

                _allGrids[hexGridId].data.AddPlayer(playerId);
            }
        }
    }
}