using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildManager;
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
        private readonly List<(HexGridData, HexGridView)> _allGrids = new ();

        [Header("Player HexGridConfiguration")] [SerializeField]
        private HexGridConfiguration _hexGridConfiguration;
        
        [Header("Player GameStartConfiguration")] [SerializeField]
        private GameStartConfiguration _gameStartConfiguration;

        public GameObject _otherTilePrefab; // TODO remove after refactor

        private void Start()
        {
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
                    height: hexGridConfiguration.Height,
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
                
                var (data, _) = GetGridById(hexGridId);
                data.AddPlayer(playerId);
            }
            
            
            // Add Buttonmanager
            var _gameManager = new GameObject("GameManager");

            var buttonManager = _gameManager.AddComponent<ButtonManager>();
            buttonManager.buildingButtons.Add(GameObject.Find("Button").GetComponent<Button>());
            var playerController = _gameManager.AddComponent<PlayerController>();
            var grid1 = _allGrids[0];
            playerController.hexGridData = grid1.Item1;
            playerController.hexGridView = grid1.Item2;

            buttonManager.availableBuildings.Add(_otherTilePrefab);

            buttonManager.playerController = playerController;
        }

        /// <summary>
        /// Returns the HexGrid with the specified ID.
        /// </summary>
        private (HexGridData, HexGridView) GetGridById(int id)
        {
            return _allGrids[id];
        }
    }
}