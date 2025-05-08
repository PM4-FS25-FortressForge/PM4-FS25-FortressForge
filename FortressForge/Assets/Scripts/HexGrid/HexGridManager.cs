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
        public static HexGridManager Instance { get; private set; }

        public List<HexGridData> AllGrids { get; } = new();

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
            for (var index = 0; index < gameSessionStartConfiguration.HexGridOrigins.Count; index++)
            {
                var hexGridOrigin = gameSessionStartConfiguration.HexGridOrigins[index];
                int radius = gameStartConfiguration.GridRadius;
                float tileSize = gameStartConfiguration.TileSize;
                float tileHeight = gameStartConfiguration.TileHeight;

                BuildingManager buildingManager = new BuildingManager();

                // Example for max value application // TODO move or remove when actual max values are set
                var maxValues = new Dictionary<ResourceType, float>
                {
                    { ResourceType.Power, 0f },
                    { ResourceType.Metal, 10000f },
                };

                EconomySystem economySystem = new EconomySystem(buildingManager, maxValues);

                HexGridData gridData = new HexGridData(
                    id: index,
                    origin: hexGridOrigin,
                    radius: radius,
                    tileSize: tileSize,
                    tileHeight: tileHeight,
                    terrainHeightProvider: _terrainHeightProvider,
                    economySystem: economySystem,
                    buildingManager: buildingManager
                );

                AllGrids.Add(gridData);
            }
        }

        /// <summary>
        /// Retrieves a HexGridData instance by its coordinate.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns>Returns the tile data or null if not found.</returns>
        [CanBeNull]
        public HexTileData GetHexTileData(HexTileCoordinate coordinate)
        {
            return AllGrids.FirstOrDefault(grid => grid.TileMap.ContainsKey(coordinate))
                ?.TileMap[coordinate];
        } 
    }
}
