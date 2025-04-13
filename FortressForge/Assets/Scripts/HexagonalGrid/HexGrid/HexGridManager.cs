using UnityEngine;
using System.Collections.Generic;
using FortressForge.GameInitialization;
using FortressForge.HexGrid.BuildingData;
using FortressForge.HexGrid.BuildManager;
using FortressForge.HexGrid.HexTile;
using FortressForge.Serializables;
using UnityEngine.UI;

namespace FortressForge.HexGrid
{
    /// <summary>
    /// Manages the creation, storage, and retrieval of HexGridData instances.
    /// Provides a singleton pattern to ensure only one manager is active,
    /// and offers methods to create new hex grids, as well as retrieve them
    /// by ID or owner.
    /// </summary>
    public class HexGridManager : MonoBehaviour
    {
        public List<(HexGridData data, HexGridView view)> AllGrids { get; } = new();
        
        public void InitializeHexGridForPlayers(GameStartConfiguration gameStartConfiguration)
        {
            // Create a hex grid for each starting position
            for (int i = 0; i < gameStartConfiguration.PlayerIdsHexGridIdTuplesList.Count; i++)
            {
                var (data, view) = HexGridFactory.CreateHexGrid(
                    id: i,
                    origin: gameStartConfiguration.HexGridOrigins[i],
                    radius: gameStartConfiguration.Radius,
                    tileSize: gameStartConfiguration.TileSize,
                    tileHeight: gameStartConfiguration.TileHeight,
                    tilePrefab: gameStartConfiguration.TilePrefab
                );
                
                AllGrids.Add((data, view));
            }
            
            // Assign each player to their respective hex grid(s)
            for (int i = 0; i < gameStartConfiguration.PlayerIdsHexGridIdTuplesList.Count; i++)
            {
                var playerId = gameStartConfiguration.PlayerIdsHexGridIdTuplesList[i].PlayerId;
                var hexGridId = gameStartConfiguration.PlayerIdsHexGridIdTuplesList[i].HexGridId;
                
                AllGrids[hexGridId].data.AddPlayer(playerId);
            }
        }
    }
}