using System;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.HexTile;
using UnityEngine.Tilemaps;

namespace FortressForge.BuildingSystem.HexGrid
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
        public Vector3 Origin { get; private set; }
        public int Id { get; set; }

        public readonly int MaxBuildHeight;

        // Players that own this grid
        public readonly List<string> PlayerIds = new();

        /// <summary>
        /// The size of the hex tiles are defined here, so that in the future different
        /// tile sizes for different players are possible (e.g. for AI).
        /// </summary>
        public readonly float TileRadius;

        public readonly float TileHeight;
        
        public readonly Dictionary<HexTileCoordinate, HexTileData> TileMap = new();
        
        public event Action<HexTileData, HexTileCoordinate> OnNewTileCreated;

        public HexGridData(int id, Vector3 origin, int radius, int maxBuildHight, float tileSize, float tileHeight)
        {
            Id = id;
            Origin = origin;
            MaxBuildHeight = maxBuildHight;
            TileRadius = tileSize;
            TileHeight = tileHeight;
            
            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Math.Max(-radius, -q - radius);
                int r2 = Math.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    HexTileCoordinate newHexCoords = new HexTileCoordinate(q, r, 0) +
                        new HexTileCoordinate(TileRadius, TileHeight, origin);
                                                     
                    TileMap[newHexCoords] = new HexTileData(newHexCoords);
                }
            }
        }
        
        public void AddPlayer(string playerId)
        {
            PlayerIds.Add(playerId);
        }

        public bool ValidateBuildingPlacement(HexTileCoordinate hexCoord, BaseBuildingTemplate buildingTemplate)
        {
            foreach (var coord in buildingTemplate.ShapeData)
            {
                TileMap.TryGetValue((hexCoord + coord), out var tileData);
                if (tileData == null || tileData.IsOccupied)
                {
                    Debug.Log("Placement failed");
                    return false;
                }
                   
                UpdateHexTileData(hexCoord);
            }
            
            Debug.Log("Placement succeeded");
            return true;
        }

        private void UpdateHexTileData(HexTileCoordinate hexCoord)
        {
            TileMap[hexCoord].IsOccupied = true;
            
            // unlock tile above
            TileMap.TryGetValue(hexCoord + new HexTileCoordinate(0, 0, 1), out var tileData);

            if (tileData == null && hexCoord.H + 1 <= MaxBuildHeight)
            {
                HexTileCoordinate newHexCoords = hexCoord + new HexTileCoordinate(0, 0, 1);
                HexTileData hexTileData = new HexTileData(newHexCoords);
                TileMap[newHexCoords] = hexTileData;
                OnNewTileCreated?.Invoke(hexTileData, newHexCoords); 
            }
        }
    }
}