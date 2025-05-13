using System;
using System.Collections.Generic;
using System.Linq;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.Economy;
using UnityEngine;
using FortressForge.GameInitialization;
using FortressForge.HexGrid.Data;
using JetBrains.Annotations;

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
        public event Action<HexTileData> OnHoverTileChanged;
        public static HexGridManager Instance { get; private set; }

        public List<HexGridData> AllGrids { get; } = new();
        public HexGridData IndependentGrid => AllGrids.First(grid => grid.Id == -1);

        private TerrainHeightProvider _terrainHeightProvider;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _terrainHeightProvider = new TerrainHeightProvider();
        }

        public void InitializeHexGrids(GameSessionStartConfiguration gameSessionStartConfiguration,
            GameStartConfiguration gameStartConfiguration)
        {
            var magmaAmount = gameStartConfiguration.GlobalMagmaAmount;
            GlobalEconomy globalEconomy = new GlobalEconomy(magmaAmount);

            int radius = gameStartConfiguration.GridRadius;
            float tileSize = gameStartConfiguration.TileSize;
            float tileHeight = gameStartConfiguration.TileHeight;
            
            for (var index = -1; index < gameSessionStartConfiguration.HexGridOrigins.Count; index++)
            {
                BuildingManager buildingManager = new BuildingManager();

                // Example for max value application // TODO move or remove when actual max values are set
                var maxValues = new Dictionary<ResourceType, float>
                {
                    { ResourceType.Power, 10000f },
                    { ResourceType.Metal, 10000f },
                    { ResourceType.Magma, magmaAmount },
                    { ResourceType.Amunition, 10000f },
                    { ResourceType.Concrete, 10000f },
                };
                EconomySystem economySystem = new EconomySystem(buildingManager, globalEconomy, maxValues);

                HexGridData gridData = new HexGridData(
                    id: index,
                    tileSize: tileSize,
                    tileHeight: tileHeight,
                    terrainHeightProvider: _terrainHeightProvider,
                    economySystem: economySystem,
                    buildingManager: buildingManager,
                    hexGridManager: this,
                    isInvisible: index == -1
                );
                
                if (index != -1)
                {
                    var hexGridOrigin = gameSessionStartConfiguration.HexGridOrigins[index];
                    gridData.CreateStarterGrid(hexGridOrigin, radius);
                }
                
                AllGrids.Add(gridData);
                gridData.OnHoverTileChanged += tileData => OnHoverTileChanged?.Invoke(tileData);
            }
        }

        /// <summary>
        /// Retrieves a HexGridData instance by its coordinate.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns>Returns the tile data or null if not found.</returns>
        [CanBeNull]
        public HexTileData GetHexTileDataOrCreate(HexTileCoordinate coordinate)
        {
            // Find tile if it exist
            var tile = AllGrids.FirstOrDefault(grid => grid.TileMap.ContainsKey(coordinate))
                ?.TileMap[coordinate];
            
            // If not found, create a new tile on the independent grid
            if (tile != null) 
                return tile;
            
            return IndependentGrid.CreateTile(coordinate);
        }

        public void AddGrid(HexGridData gridData) {
            AllGrids.Add(gridData);
            gridData.OnHoverTileChanged += tileData => OnHoverTileChanged?.Invoke(tileData);
        }
    }
}
