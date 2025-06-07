using FortressForge.BuildingSystem.BuildManager;
using FortressForge.HexGrid;
using FortressForge.HexGrid.View;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Manages the initialization of global game objects and systems at the start of a session.
    /// </summary>
    public class GlobalObjectInitializationManager : MonoBehaviour
    {
        [Header("Game Start Configuration")]
        [SerializeField] private GameStartConfiguration _gameStartConfiguration;
        [SerializeField] private GameSessionStartConfiguration _gameSessionStartConfiguration;
        
        /// <summary>
        /// Unity Awake callback. Instantiates the terrain and adds the HexGridManager component.
        /// </summary>
        public void Awake()
        {
            Instantiate(_gameStartConfiguration.Terrain);
            gameObject.AddComponent<HexGridManager>();
        }
        
        /// <summary>
        /// Unity Start callback. Initializes core game systems and grid views.
        /// </summary>
        public void Start()
        {
            if (_gameStartConfiguration == null)
            {
                Debug.LogError("GameStartConfiguration is not set.");
                return;
            }
            var terrainHeightProvider = new TerrainHeightProvider();
            var meshRenderer = gameObject.AddComponent<GlobalGridMeshRenderer>();
            meshRenderer.Init(terrainHeightProvider, _gameStartConfiguration);
            
            HexGridManager.Instance.InitializeHexGrids(_gameSessionStartConfiguration, _gameStartConfiguration);
            
            HexTileHoverController hexTileHoverController = gameObject.AddComponent<HexTileHoverController>();
            hexTileHoverController.Init(terrainHeightProvider, HexGridManager.Instance, _gameStartConfiguration);
            
            PreviewController previewController = gameObject.AddComponent<PreviewController>();
            previewController.Init(_gameStartConfiguration, HexGridManager.Instance, hexTileHoverController);
            
            InitializeHexGridViews(_gameStartConfiguration, HexGridManager.Instance);
        }

        /// <summary>
        /// Instantiates and initializes all HexGridView objects for each grid in the game.
        /// </summary>
        /// <param name="config">The game start configuration.</param>
        /// <param name="hexGridManager">The hex grid manager instance.</param>
        private void InitializeHexGridViews(GameStartConfiguration config, HexGridManager hexGridManager)
        {
            foreach (var data in hexGridManager.AllGrids)
            {
                HexGridView hexGridView = new GameObject("HexGridView_" + data.Id)
                        .AddComponent<HexGridView>();
                hexGridView.transform.SetParent(transform);
                hexGridView.Initialize(config.TilePrefab, data, _gameStartConfiguration);
                
                GameObject buildingContainer = new ("BuildingContainer_Grid_" + data.Id);
            }
        }
    }
}