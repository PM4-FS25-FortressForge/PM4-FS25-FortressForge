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
        public List<HexGridAssembly> AllGrids { get; } = new();
        
        private IHexGridDataFactory _dataFactory;
        private IHexGridViewFactory _viewFactory;
        private HexGridAssembler _assembler;

        private void Start()
        {
            ITerrainHeightProvider terrainProvider = new UnityTerrainHeightProvider();
            
            _dataFactory = new HexGridDataFactory(terrainProvider);
            _viewFactory = new HexGridViewFactory();
            
            _assembler = new HexGridAssembler(_dataFactory, _viewFactory);
        }

        public void InitializeHexGridForPlayers(GameStartConfiguration gameStartConfiguration)
        {
            for (int i = 0; i < gameStartConfiguration.PlayerIdsHexGridIdTuplesList.Count; i++)
            {
                Vector3 origin = gameStartConfiguration.HexGridOrigins[i];
                int radius = gameStartConfiguration.Radius;
                float tileSize = gameStartConfiguration.TileSize;
                float tileHeight = gameStartConfiguration.TileHeight;
                GameObject tilePrefab = gameStartConfiguration.TilePrefab;
                
                HexGridAssembly gridAssembly = _assembler.CreateHexGrid(
                    i, origin, radius, tileSize, tileHeight, tilePrefab);
                
                var playerId = gameStartConfiguration
                    .PlayerIdsHexGridIdTuplesList[i].PlayerId;
                gridAssembly.Data.AddPlayer(playerId);
                
                AllGrids.Add(gridAssembly);
            }
        }
    }
}