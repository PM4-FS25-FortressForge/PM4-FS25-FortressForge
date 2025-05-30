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
    /// with an additional height coordinate.
    /// The coordinates are stored as (q, r, h) tuples. (Queue, Row, Height)
    /// Reference: https://www.redblobgames.com/grids/hexagons/
    /// </summary>
    public class HexGridData
    {
        public event Action<HexTileData> OnHoverTileChanged;
        public event Action<HexTileData> OnChanged;
        public int Id { get; set; }
        
        public readonly float TileRadius;

        public readonly float TileHeight;
        
        public bool IsOwned { get; set; }
        public bool IsInvisible { get; set; }
        public bool IsIndependent => Id == -1;
        
        public Dictionary<HexTileCoordinate, HexTileData> TileMap = new();
        public EconomySystem EconomySystem { get; private set; }
        public BuildingManager BuildingManager { get; private set; }
        private readonly HexGridManager _hexGridManager;
        
        public event Action<HexTileData, HexTileCoordinate> OnNewTileCreated;
        public event Action<HexTileCoordinate> OnTileRemoved;
        
        private readonly ITerrainHeightProvider _terrainHeightProvider;

        /// <summary>
        /// Create a starter hex grid with the specified parameters.
        /// </summary>
        public HexGridData(int id,
            float tileSize,
            float tileHeight,
            ITerrainHeightProvider terrainHeightProvider,
            EconomySystem economySystem,
            BuildingManager buildingManager,
            HexGridManager hexGridManager,
            bool isInvisible = false)
        {
            Id = id;
            TileRadius = tileSize;
            TileHeight = tileHeight;
            _terrainHeightProvider = terrainHeightProvider;
            EconomySystem = economySystem;
            BuildingManager = buildingManager;
            _hexGridManager = hexGridManager;
            IsInvisible = isInvisible;
        }

        /// <summary>
        /// Create a starter hex grid with the specified parameters.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public void CreateStarterGrid(Vector3 origin, int radius)
        {
            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Math.Max(-radius, -q - radius);
                int r2 = Math.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    HexTileCoordinate newHexCoords = new HexTileCoordinate(q, r, 0) +
                                                     new HexTileCoordinate(TileRadius, TileHeight, origin);
                    newHexCoords = GetTerrainHeightFromHexTileCoordinate(newHexCoords, TileRadius, TileHeight);
                                                     
                    CreateOrClaimHexTile(newHexCoords);
                }
            }
        }

        /// <summary>
        /// Searches for a tile in the grid. If it doesn't exist, it creates a new one.
        /// If the tile is already owned by another grid, it does nothing and returns null.
        /// </summary>
        /// <param name="newHexCoords"></param>
        /// <returns></returns>
        public HexTileData CreateOrClaimHexTile(HexTileCoordinate newHexCoords)
        {
            // Find if tile is in another grid already
            HexGridData previousOwner = _hexGridManager.AllGrids
                .FirstOrDefault(grid => grid.TileMap.ContainsKey(newHexCoords));

            if (previousOwner == this)
                return TileMap[newHexCoords];
            
            // If the tile is not owned by any grid, create a new tile.
            if (previousOwner == null) 
                return CreateTile(newHexCoords);
            
            // Check if the owner is the independent grid, in this case, we can just claim it.
            if (previousOwner == _hexGridManager.IndependentGrid)
                return ClaimTile(newHexCoords);
 
            // If the tile is owned by another grid, do nothing and return null.
            Debug.LogWarning("Tile already owned by another grid. Cannot create new tile.");
            return null;
        }
        
        private HexTileData ClaimTile(HexTileCoordinate newHexCoords)
        {
            RemoveTile(newHexCoords);
            return CreateTile(newHexCoords);
        }

        /// <summary>
        /// Always creates a new tile. Use this if you want to create a new tile and not check if it already exists.
        /// This can cause conflicts so use it with caution.
        /// </summary>
        /// <param name="newHexCoords"></param>
        /// <returns></returns>
        public HexTileData CreateTile(HexTileCoordinate newHexCoords)
        {
            if (!ValidateTilePlacement(newHexCoords))
                return null;
            
            var tile = new HexTileData(newHexCoords)
            {
                IsOwned = IsOwned,
                IsInvisible = IsInvisible,
            };
            
            // Create and store the delegate
            tile.OnHoverChanged += tileData => OnHoverTileChanged?.Invoke(tileData);
            tile.OnChanged += hexTileData => OnChanged?.Invoke(hexTileData);
            OnNewTileCreated?.Invoke(tile, newHexCoords);
            TileMap[newHexCoords] = tile;
            
            return tile;
        }
        
        private void RemoveTile(HexTileCoordinate hexCoord)
        {
            TileMap.Remove(hexCoord);

            // Execute event for removal
            OnTileRemoved?.Invoke(hexCoord);
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
                    CreateNewTileAbove(hexCoord + coord);
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
        
        public bool ValidateTilePlacement(HexTileCoordinate hexCoord)
        {
            // Check if lowest on the grid
            var lowestTile = _terrainHeightProvider.GetHexTileCoordinate(
                hexCoord.GetWorldPosition(TileRadius, TileHeight),
                TileHeight, TileRadius);
            if (lowestTile.H != hexCoord.H)
            {
                // Independent grid are not allowed to be placed on top of other grids
                if (IsIndependent) 
                    return false;
                
                // Check if tile below
                hexCoord.H -= 1;
                return TileMap.ContainsKey(hexCoord);
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

            if (tileData == null) {
                HexTileCoordinate newHexCoords = hexCoord + new HexTileCoordinate(0, 0, 1);
                CreateOrClaimHexTile(newHexCoords);
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