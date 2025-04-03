using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.BuildingSystem.HexTile;
using FortressForge.Serializables;
using UnityEngine.UI;

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
        private readonly List<(HexGridData data, HexGridView view)> _allGrids = new ();

        [Header("Player HexGridConfiguration")] [SerializeField]
        private HexGridConfiguration _hexGridConfiguration;
        
        [Header("Player GameStartConfiguration")] [SerializeField]
        private GameStartConfiguration _gameStartConfiguration;

        public List<BaseBuildingTemplate> _otherTilePrefab; 
        public Dropdown superTollesDropdown; 

        private void Start()
        {
            HexTileCoordinate.Init(_hexGridConfiguration.Radius, _hexGridConfiguration.TileHeight, Vector3.zero);
            InitializeHexGridForPlayers(_gameStartConfiguration, _hexGridConfiguration);
        }
        
        public void InitializeHexGridForPlayers(GameStartConfiguration gameStartConfiguration, HexGridConfiguration hexGridConfiguration)
        {
            // Create a hex grid for each starting position
            for (int i = 0; i < gameStartConfiguration.PlayerIdsHexGridIdTuplesList.Count; i++)
            {
                var (data, view) = HexGridFactory.CreateHexGrid(
                    id: i,
                    origin: gameStartConfiguration.HexGridOrigins[i],
                    radius: hexGridConfiguration.Radius,
                    maxBuildHight: hexGridConfiguration.MaxBuildHeight,
                    tileSize: hexGridConfiguration.TileSize,
                    tileHeight: hexGridConfiguration.TileHeight,
                    tilePrefab: hexGridConfiguration.TilePrefab
                );
                
                _allGrids.Add((data, view));
            }
            
            // Assign each player to their respective hex grid(s)
            for (int i = 0; i < gameStartConfiguration.PlayerIdsHexGridIdTuplesList.Count; i++)
            {
                var playerId = gameStartConfiguration.PlayerIdsHexGridIdTuplesList[i].PlayerId;
                var hexGridId = gameStartConfiguration.PlayerIdsHexGridIdTuplesList[i].HexGridId;
                
                _allGrids[hexGridId].data.AddPlayer(playerId);
            }
            
            // Add Buttonmanager TODO replace this with a more robust approach
            var _gameManager = new GameObject("GameManager");

            var buttonManager = _gameManager.AddComponent<ButtonManager>();
            buttonManager.dropdown = superTollesDropdown;
            var playerController = _gameManager.AddComponent<BuildViewController>();
            var grid1 = _allGrids[0];
            playerController.hexGridData = grid1.data;
            playerController.hexGridView = grid1.view;

            buttonManager.availableBuildings = _otherTilePrefab;
            buttonManager.Init();

            buttonManager.buildViewController = playerController;
        }
    }
}