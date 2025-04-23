using System;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.Economy;
using UnityEngine.Tilemaps;

namespace FortressForge.HexGrid.Data
{
    /// <summary>
    /// Represents the pure data of a hex grid. This class is responsible for
    /// organizing the data of the hex tiles and providing methods to access them.
    /// In this implementation, AXIAL coordinates are used to identify the hex tiles
    /// with a additional height coordinate.
    /// The coordinates are stored as (q, r, h) tuples. (Queue, Row, Height)
    /// Reference: https://www.redblobgames.com/grids/hexagons/
    /// </summary>
    public class HexGridData
    {
        public event Action<HexTileData> OnHoverTileChanged;
        public Vector3 Origin { get; private set; }
        public int Id { get; set; }

        // Players that own this grid
        public readonly List<string> PlayerIds = new();
        
        public readonly float TileRadius;

        public readonly float TileHeight;
        
        public Dictionary<HexTileCoordinate, HexTileData> TileMap = new();
        public EconomySystem EconomySystem { get; private set; }
        public BuildingManager BuildingManager { get; private set; }
        
        public event Action<HexTileData, HexTileCoordinate> OnNewTileCreated;
        
        private readonly ITerrainHeightProvider _terrainHeightProvider;

        public HexGridData(int id,
            Vector3 origin,
            int radius,
            float tileSize,
            float tileHeight,
            ITerrainHeightProvider terrainHeightProvider,
            EconomySystem economySystem,
            BuildingManager buildingManager)
        {
            Id = id;
            Origin = origin;
            TileRadius = tileSize;
            TileHeight = tileHeight;
            _terrainHeightProvider = terrainHeightProvider;
            EconomySystem = economySystem;
            BuildingManager = buildingManager;

            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Math.Max(-radius, -q - radius);
                int r2 = Math.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    HexTileCoordinate newHexCoords = new HexTileCoordinate(q, r, 0) +
                                                     new HexTileCoordinate(TileRadius, TileHeight, origin);
                    newHexCoords = GetTerrainHeightFromHexTileCoordinate(newHexCoords);
                                                     
                    CreateNewHexTile(newHexCoords);
                }
            }
        }

        private void CreateNewHexTile(HexTileCoordinate newHexCoords)
        {
            TileMap[newHexCoords] = new HexTileData(newHexCoords);
            TileMap[newHexCoords].OnHoverChanged += OnHoverTileChangedEvent;
        }

        public void AddPlayer(string playerId)
        {
            PlayerIds.Add(playerId);
        }

        /// <summary>
        /// Places buildings on the hex grid. This doesn't check if the placement is valid. Use Validate Building Placement first.
        /// </summary>
        /// <param name="hexCoord"></param>
        /// <param name="buildingTemplate"></param>
        public void PlaceBuilding(HexTileCoordinate hexCoord, BaseBuildingTemplate buildingTemplate)
        { 
            foreach (var coord in buildingTemplate.ShapeData)
            {
                OccupyHexTileAndUnlockNewTile(hexCoord + coord);
            }
        }

        /// <summary>
        /// Validates if a building can be placed on the hex grid.
        /// </summary>
        /// <param name="hexCoord"></param>
        /// <param name="buildingTemplate"></param>
        /// <returns>Returns true if placement is unoccupied</returns>
        public bool ValidateBuildingPlacement(HexTileCoordinate hexCoord, BaseBuildingTemplate buildingTemplate)
        { // TODO: THis method does more then just validate the placement, consider renaming, or better splitting it up
            foreach (var coord in buildingTemplate.ShapeData)
            {
                TileMap.TryGetValue((hexCoord + coord), out var tileData);
                if (tileData == null || tileData.IsOccupied)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Updates the specified hex tile by marking it as occupied and unlocking the tile directly above it.
        /// If the tile above does not exist and is within the maximum build height,
        /// a new tile is created and added to the tile map, and the OnNewTileCreated event is triggered.
        /// </summary>
        private void OccupyHexTileAndUnlockNewTile(HexTileCoordinate hexCoord)
        {
            TileMap[hexCoord].IsOccupied = true;
            
            // unlock tile above
            TileMap.TryGetValue(hexCoord + new HexTileCoordinate(0, 0, 1), out var tileData);

            if (tileData == null)
            {
                HexTileCoordinate newHexCoords = hexCoord + new HexTileCoordinate(0, 0, 1);
                HexTileData hexTileData = new HexTileData(newHexCoords);
                TileMap[newHexCoords] = hexTileData;
                OnNewTileCreated?.Invoke(hexTileData, newHexCoords); 
            }
        }

        private HexTileCoordinate GetTerrainHeightFromHexTileCoordinate(HexTileCoordinate position)
        {
            Vector3 worldPos = position.GetWorldPosition(TileRadius, TileHeight);
        
            // Hier wird jetzt das Interface benutzt:
            float terrainHeight = _terrainHeightProvider.SampleHeight(worldPos);

            return new HexTileCoordinate(
                position.Q, 
                position.R, 
                Mathf.CeilToInt(terrainHeight / TileHeight)
            );
        }
        
        private void OnHoverTileChangedEvent(HexTileData hexTileData)
        {
            OnHoverTileChanged?.Invoke(hexTileData);
        }
    }
}