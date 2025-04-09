using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.GameInitialization;
using UnityEngine.UI;

namespace FortressForge.BuildingSystem.HexGrid
{
    /// <summary>
    /// Manages the creation, storage, and retrieval of HexGridData instances.
    /// Interacts with the GameInitializer to initialize hex grids for players
    /// based on the game start configuration.
    /// </summary>
    [RequireComponent(typeof(GameInitializer))]
    public class HexGridManager : MonoBehaviour
    {
        private readonly List<(HexGridData data, HexGridView view)> _allGrids = new();

        [Header("References")] [SerializeField]
        private GameInitializer _gameInitializer;

        public List<BaseBuildingTemplate> _otherTilePrefab;
        public Dropdown gridSelectionDropdown;

        /// <summary>
        /// Retrieves the GameInitializer reference and initializes hex grids for players
        /// based on the game start configuration.
        /// </summary>
        private void Awake()
        {
            _gameInitializer = GetComponent<GameInitializer>();
            _gameInitializer.InitializeHexGridForPlayers();
        }
    }
}