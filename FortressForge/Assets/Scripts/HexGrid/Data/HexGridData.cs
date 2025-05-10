using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.Economy;
using FortressForge.HexGrid.View;

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
        
        public readonly float TileRadius;

        public readonly float TileHeight;
        
        public bool IsOwned { get; set; }
        
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
                    newHexCoords = GetTerrainHeightFromHexTileCoordinate(newHexCoords, TileRadius, TileHeight);
                                                     
                    CreateNewHexTile(newHexCoords);
                }
            }
        }

        private HexTileData CreateNewHexTile(HexTileCoordinate newHexCoords)
        {
            TileMap[newHexCoords] = new HexTileData(newHexCoords)
            {
                IsOwned = IsOwned
            };
            TileMap[newHexCoords].OnHoverChanged += tileData => OnHoverTileChanged?.Invoke(tileData);
            return TileMap[newHexCoords];
        }

        /// <summary>
        /// Places buildings on the hex grid. This doesn't check if the placement is valid. Use Validate Building Placement first.
        /// </summary>
        /// <param name="hexCoord"></param>
        /// <param name="shapeData"></param>
        /// <param name="isStackable"></param>
        public void MarkBuildingTiles(HexTileCoordinate hexCoord, List<HexTileCoordinate> shapeData, List<bool> isStackable)
        {
            if (shapeData.Count != isStackable.Count)
            {
                throw new ArgumentException("Shape data and stackable lists must have the same length.");
            }
            
            for (int i = 0; i < shapeData.Count; i++)
            {
                var coord = shapeData[i];
                OccupyHexTile(hexCoord + coord);
                
                if (isStackable[i]) {
                    CreateNewTileAbove(hexCoord);
                }
            }
        }
         

        /// <summary>
        /// Validates if a building can be placed on the hex grid.
        /// </summary>
        /// <param name="hexCoord"></param>
        /// <param name="shapeData"></param>
        /// <returns>Returns true if placement is unoccupied</returns>
        public bool ValidateBuildingPlacement(HexTileCoordinate hexCoord, List<HexTileCoordinate> shapeData)
        {
            foreach (var coord in shapeData)
            {
                TileMap.TryGetValue((hexCoord + coord), out var tileData);
                if (tileData == null || tileData.IsOccupied)
                {
                    return false;
                }
            }
            return true;
        }

        public void MarkGridAsOwned()
        {
            TileMap.ToList().ForEach(tile => tile.Value.IsOwned = true);
            IsOwned = true;
        }

        /// <summary>
        /// Updates the specified hex tile by marking it as occupied.
        /// </summary>
        private void OccupyHexTile(HexTileCoordinate hexCoord)
        {
            TileMap[hexCoord].IsOccupied = true;
        }

        /// <summary>
        /// Creates a new tile above the specified hex tile coordinate if it doesn't already exist.
        /// </summary>
        /// <param name="hexCoord"></param>
        private void CreateNewTileAbove(HexTileCoordinate hexCoord) {
            // unlock tile above
            TileMap.TryGetValue(hexCoord + new HexTileCoordinate(0, 0, 1), out var tileData);

            if (tileData == null)
            {
                HexTileCoordinate newHexCoords = hexCoord + new HexTileCoordinate(0, 0, 1);
                HexTileData hexTileData = CreateNewHexTile(newHexCoords);
                OnNewTileCreated?.Invoke(hexTileData, newHexCoords); 
            }
        }

        private HexTileCoordinate GetTerrainHeightFromHexTileCoordinate(HexTileCoordinate position, float tileRadius, float tileHeight)
        {
            Vector3 worldPos = position.GetWorldPosition(tileRadius, tileHeight);
        
            float terrainHeight = _terrainHeightProvider.SampleHeight(worldPos);

            return new HexTileCoordinate(
                position.Q, 
                position.R, 
                Mathf.CeilToInt(terrainHeight / tileHeight)
            );
        }
    }
}