using UnityEngine;
using System.Collections.Generic;

namespace FortressForge.BuildingSystem.HexGrid
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
        
        private readonly Dictionary<int, HexGridData> _allGrids = new Dictionary<int, HexGridData>();

        private int _nextGridId = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
            }
            else {
                Instance = this;
            }
        }

        /// <summary>
        /// Creates a new HexGrid with the specified size and origin.
        /// OwnerId can be a player name, a network ID, etc.
        /// </summary>
        public HexGridData CreateHexGrid(Vector3 origin, int radius, int height, string ownerId, float tileSize, float tileHeight) {
            HexGridData newGrid = new HexGridData(_nextGridId, origin, radius, height, tileSize, tileHeight)
                {OwnerId = ownerId};

            _allGrids.Add(_nextGridId, newGrid);
            _nextGridId++;

            return newGrid;
        }

        /// <summary>
        /// Returns the HexGrid with the specified ID.
        /// </summary>
        public HexGridData GetGridById(int gridId)
        {
            return _allGrids.GetValueOrDefault(gridId, null);
        }
        
        /// <summary>
        /// Returns the HexGrid with the specified owner ID.
        /// </summary>
        public HexGridData GetGridByOwnerId(string ownerId)
        {
            foreach (var grid in _allGrids.Values) {
                if (grid.OwnerId == ownerId) {
                    return grid;
                }
            }

            return null;
        }
    }
}