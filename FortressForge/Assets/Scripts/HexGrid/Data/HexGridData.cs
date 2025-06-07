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
        /// <summary>
        /// Event triggered when the hovered tile changes.
        /// </summary>
        public event Action<HexTileData> OnHoverTileChanged;

        /// <summary>
        /// Event triggered when any tile in the grid changes.
        /// </summary>
        public event Action<HexTileData> OnChanged;

        /// <summary>
        /// Unique identifier for this grid.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The radius of each hex tile.
        /// </summary>
        public readonly float TileRadius;

        /// <summary>
        /// The height of each hex tile.
        /// </summary>
        public readonly float TileHeight;
        
        /// <summary>
        /// Indicates if the grid is owned by a player.
        /// </summary>
        public bool IsOwned { get; set; }

        /// <summary>
        /// Indicates if the grid is invisible.
        /// </summary>
        public bool IsInvisible { get; set; }

        /// <summary>
        /// Returns true if this grid is the independent grid (Id == -1).
        /// </summary>
        public bool IsIndependent => Id == -1;
        
        /// <summary>
        /// Maps tile coordinates to their data objects.
        /// </summary>
        public Dictionary<HexTileCoordinate, HexTileData> TileMap = new();

        /// <summary>
        /// The economy system associated with this grid.
        /// </summary>
        public EconomySystem EconomySystem { get; private set; }

        /// <summary>
        /// The building manager associated with this grid.
        /// </summary>
        public BuildingManager BuildingManager { get; private set; }

        private readonly HexGridManager _hexGridManager;
        
        /// <summary>
        /// Event triggered when a new tile is created.
        /// </summary>
        public event Action<HexTileData, HexTileCoordinate> OnNewTileCreated;

        /// <summary>
        /// Event triggered when a tile is removed.
        /// </summary>
        public event Action<HexTileCoordinate> OnTileRemoved;
        
        private readonly ITerrainHeightProvider _terrainHeightProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HexGridData"/> class.
        /// </summary>
        /// <param name="id">Grid identifier.</param>
        /// <param name="tileSize">Radius of each tile.</param>
        /// <param name="tileHeight">Height of each tile.</param>
        /// <param name="terrainHeightProvider">Terrain height provider.</param>
        /// <param name="economySystem">Economy system for the grid.</param>
        /// <param name="buildingManager">Building manager for the grid.</param>
        /// <param name="hexGridManager">Reference to the grid manager.</param>
        /// <param name="isInvisible">Whether the grid is invisible.</param>
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
        /// Creates a starter hex grid at the given origin with the specified radius.
        /// </summary>
        /// <param name="origin">World position of the grid's origin.</param>
        /// <param name="radius">Radius (in tiles) of the grid.</param>
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
        /// <param name="newHexCoords">Coordinates of the tile to create or claim.</param>
        /// <returns>The created or claimed <see cref="HexTileData"/>, or null if not possible.</returns>
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
        
        /// <summary>
        /// Claims a tile from the independent grid and creates it in this grid.
        /// </summary>
        /// <param name="newHexCoords">Coordinates of the tile to claim.</param>
        /// <returns>The claimed <see cref="HexTileData"/>.</returns>
        private HexTileData ClaimTile(HexTileCoordinate newHexCoords)
        {
            RemoveTile(newHexCoords);
            return CreateTile(newHexCoords);
        }

        /// <summary>
        /// Always creates a new tile. Use this if you want to create a new tile and not check if it already exists.
        /// This can cause conflicts so use it with caution.
        /// </summary>
        /// <param name="newHexCoords">Coordinates of the tile to create.</param>
        /// <returns>The created <see cref="HexTileData"/>, or null if placement is invalid.</returns>
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
        
        /// <summary>
        /// Removes a tile from the grid and triggers the removal event.
        /// </summary>
        /// <param name="hexCoord">Coordinates of the tile to remove.</param>
        private void RemoveTile(HexTileCoordinate hexCoord)
        {
            TileMap.Remove(hexCoord);

            // Execute event for removal
            OnTileRemoved?.Invoke(hexCoord);
        }

        /// <summary>
        /// Marks the specified tiles as occupied for building placement.
        /// This does not validate placement; use <see cref="ValidateBuildingPlacement"/> first.
        /// </summary>
        /// <param name="hexCoord">Origin coordinate for the building.</param>
        /// <param name="shapeData">Relative coordinates for the building's shape.</param>
        /// <param name="isStackable">List indicating if each tile is stackable.</param>
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
        /// <param name="hexCoord">Origin coordinate for the building.</param>
        /// <param name="shapeData">Relative coordinates for the building's shape.</param>
        /// <returns>Returns true if all placement tiles are unoccupied.</returns>
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

        /// <summary>
        /// Validates if a tile can be placed at the given coordinate.
        /// </summary>
        /// <param name="hexCoord">Tile coordinate to validate.</param>
        /// <returns>True if placement is valid, otherwise false.</returns>
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

        /// <summary>
        /// Marks all tiles in the grid as owned and sets the grid as owned.
        /// </summary>
        public void MarkGridAsOwned()
        {
            TileMap.ToList().ForEach(tile => tile.Value.IsOwned = true);
            IsOwned = true;
        }

        /// <summary>
        /// Marks the specified tile as occupied.
        /// </summary>
        /// <param name="hexCoord">Tile coordinate to mark as occupied.</param>
        private void OccupyHexTile(HexTileCoordinate hexCoord)
        {
            TileMap[hexCoord].IsOccupied = true;
        }

        /// <summary>
        /// Creates a new tile above the specified coordinate if it doesn't already exist.
        /// </summary>
        /// <param name="hexCoord">Base coordinate to create above.</param>
        private void CreateNewTileAbove(HexTileCoordinate hexCoord) {
            // unlock tile above
            TileMap.TryGetValue(hexCoord + new HexTileCoordinate(0, 0, 1), out var tileData);

            if (tileData == null) {
                HexTileCoordinate newHexCoords = hexCoord + new HexTileCoordinate(0, 0, 1);
                CreateOrClaimHexTile(newHexCoords);
            }
        }

        /// <summary>
        /// Adjusts the height of a tile coordinate based on terrain height.
        /// </summary>
        /// <param name="position">Original tile coordinate.</param>
        /// <param name="tileRadius">Tile radius.</param>
        /// <param name="tileHeight">Tile height.</param>
        /// <returns>Tile coordinate with adjusted height.</returns>
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