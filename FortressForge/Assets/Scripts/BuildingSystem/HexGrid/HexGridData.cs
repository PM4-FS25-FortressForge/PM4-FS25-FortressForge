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

        // Players that own this grid
        public readonly List<string> PlayerIds = new();

        /// <summary>
        /// The size of the hex tiles are defined here, so that in the future different
        /// tile sizes for different players are possible (e.g. for AI).
        /// </summary>
        public readonly float TileRadius; // TODO TileRadius is available here and in HexGridConfiguration, consider always using one or making both readonly

        public readonly float TileHeight;
        
        public readonly Dictionary<HexTileCoordinate, HexTileData> TileMap = new();

        public HexGridData(int id, Vector3 origin, int radius, int height, float tileSize, float tileHeight)
        {
            Id = id;
            Origin = origin;
            TileRadius = tileSize;
            TileHeight = tileHeight;

            for (int h = 0; h < height; h++) // TODO instead of precreating for each height consider creating on demand
            {
                for (int q = -radius; q <= radius; q++)
                {
                    int r1 = Math.Max(-radius, -q - radius);
                    int r2 = Math.Min(radius, -q + radius);
                    for (int r = r1; r <= r2; r++)
                    {
                        TileMap[new HexTileCoordinate(q, r, h) + new HexTileCoordinate(Origin, TileRadius, TileHeight)] = new HexTileData();
                    }
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
                HexTileData tileCoordinates = TileMap[hexCoord + coord];
                if (tileCoordinates == null || tileCoordinates.IsOccupied)
                    return false;
                tileCoordinates.IsOccupied = true; // Todo move this to a better place
            }
            
            return true;
        }
    }
}