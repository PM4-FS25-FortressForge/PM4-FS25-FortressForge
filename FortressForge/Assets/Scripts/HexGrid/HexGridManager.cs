using System;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.Economy;
using UnityEngine;
using FortressForge.GameInitialization;
using FortressForge.HexGrid.Data;

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
        private readonly Dictionary<int, HexGridData> _gridsById = new();
        private readonly Dictionary<int, HexGridData> _gridsByOwnerId = new();

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

        public void InitializeHexGridForPlayers(GameStartConfiguration gameStartConfiguration)
        {
            foreach (var player in gameStartConfiguration.PlayerIdsHexGridIdTuplesList)
            {
                Vector3 origin = gameStartConfiguration.HexGridOrigins[player.HexGridId];
                int radius = gameStartConfiguration.Radius;
                float tileSize = gameStartConfiguration.TileSize;
                float tileHeight = gameStartConfiguration.TileHeight;
                
                BuildingManager buildingManager = new BuildingManager();
                
                // Example for max value application // TODO move or remove when actual max values are set
                var maxValues = new Dictionary<ResourceType, float>
                {
                    { ResourceType.Power, 0f },
                    { ResourceType.Metal, 10000f },
                };
                
                EconomySystem economyController = new EconomySystem(buildingManager, maxValues);
                
                HexGridData gridData = new HexGridData(
                    id: player.HexGridId,
                    origin: origin,
                    radius: radius,
                    tileSize: tileSize,
                    tileHeight: tileHeight,
                    terrainHeightProvider: _terrainHeightProvider,
                    economySystem: economyController,
                    buildingManager: buildingManager
                );

                AllGrids.Add(gridData);
                _gridsById[player.HexGridId] = gridData;
                _gridsByOwnerId[player.PlayerId] = gridData;
            }
        }

        public HexGridData GetGridById(int id)
        {
            return _gridsById.GetValueOrDefault(id);
        }

        public HexGridData GetGridByOwner(int ownerId)
        {
            return _gridsByOwnerId.GetValueOrDefault(ownerId);
        }
    }
}
