using System;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
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

        public List<string> PlayerIds = new();

        /// <summary>
        /// The size of the hex tiles are defined here, so that in the future different
        /// tile sizes for different players are possible (e.g. for AI).
        /// </summary>
        public float TileRadius { get; private set; }

        public float TileHeight { get; private set; }

        private readonly Dictionary<(int, int, int), HexTileData> _tiles = new();

        /// <summary>
        /// Returns a list of all tiles in the grid.
        /// </summary>
        public Dictionary<(int, int, int), HexTileData> AllTiles => _tiles;

        public HexGridData(int id, Vector3 origin, int radius, int height, float tileSize, float tileHeight)
        {
            Id = id;
            this.Origin = origin;
            TileRadius = tileSize;
            this.TileHeight = tileHeight;

            for (int h = 0; h < height; h++)
            {
                for (int q = -radius; q <= radius; q++)
                {
                    int r1 = Math.Max(-radius, -q - radius);
                    int r2 = Math.Min(radius, -q + radius);
                    for (int r = r1; r <= r2; r++)
                    {
                        (int, int, int) coord = (q, r, h);
                        _tiles[coord] = new HexTileData(coord);
                    }
                }
            }
        }
        
        public void AddPlayer(string playerId)
        {
            PlayerIds.Add(playerId);
        }

        public bool ValidateBuildingPlacement((int, int, int) hexCoord, BaseBuilding building)
        {
            foreach (var kvp in building.shapeData)
            {
                (int, int, int) coord = (kvp.r, kvp.q, kvp.h);
                HexTileData tileData = GetTileData((coord.Item1 + hexCoord.Item1, coord.Item2 + hexCoord.Item2,
                    coord.Item3 + hexCoord.Item3));
                if (tileData == null || tileData.IsOccupied)
                    return false;
                tileData.IsOccupied = true;
            }
            
            return true;
        }

        /// <summary>
        /// Gets the data of a specific hex tile.
        /// </summary>
        public HexTileData GetTileData((int, int, int) hexCoord)
        {
            if (_tiles.TryGetValue(hexCoord, out HexTileData data))
            {
                return data;
            }

            return null;
        }

        /// <summary>
        /// Sets (or overwrites) the data of a specific hex tile.
        /// </summary>
        public void SetTileData((int, int, int) hexCoord, HexTileData newData)
        {
            if (_tiles.ContainsKey(hexCoord))
            {
                _tiles[hexCoord] = newData;
            }
        }
    }
}