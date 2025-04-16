using System;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.GameInitialization;
using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;
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
        public List<HexGridData> AllGrids { get; } = new();
        
        private TerrainHeightProvider _terrainHeightProvider;

        private void Awake()
        {
            _terrainHeightProvider = new TerrainHeightProvider();
        }

        public void InitializeHexGridForPlayers(GameStartConfiguration gameStartConfiguration)
        {
            for (int i = 0; i < gameStartConfiguration.PlayerIdsHexGridIdTuplesList.Count; i++)
            {
                Vector3 origin = gameStartConfiguration.HexGridOrigins[i];
                int radius = gameStartConfiguration.Radius;
                float tileSize = gameStartConfiguration.TileSize;
                float tileHeight = gameStartConfiguration.TileHeight;
                
                HexGridData gridData = new HexGridData(
                    id: i,
                    origin: origin,
                    radius: radius,
                    tileSize: tileSize,
                    tileHeight: tileHeight,
                    terrainHeightProvider: _terrainHeightProvider
                );
                
                AllGrids.Add(gridData);
            }
        }
    }
}