using FortressForge.HexGrid;
using FortressForge.HexGrid.View;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.GameInitialization
{
    public class GlobalObjectInitializationManager : MonoBehaviour
    {
        [Header("Game Start Configuration")]
        [SerializeField] private GameStartConfiguration _gameStartConfiguration;
        [SerializeField] private GameSessionStartConfiguration _gameSessionStartConfiguration;
        
        /// <summary>
        /// Initializing all global objects.
        /// </summary>
        public void Awake()
        {
            Instantiate(_gameStartConfiguration.Terrain);
            gameObject.AddComponent<HexGridManager>();
            var meshRenderer = gameObject.AddComponent<GlobalGridMeshRenderer>();
            meshRenderer.Init(new TerrainHeightProvider(), _gameStartConfiguration);
        }
        
        /// <summary>
        /// Start function to initialize common game systems.
        /// </summary>
        public void Start()
        {
            if (_gameStartConfiguration == null)
            {
                Debug.LogError("GameStartConfiguration is not set.");
                return;
            }
            
            HexGridManager.Instance.InitializeHexGrids(_gameSessionStartConfiguration, _gameStartConfiguration);
            
            InitializeHexGridViews(_gameStartConfiguration, HexGridManager.Instance);
        }

        private void InitializeHexGridViews(GameStartConfiguration config, HexGridManager hexGridManager)
        {
            foreach (var data in hexGridManager.AllGrids)
            {
                HexGridView hexGridView = new GameObject("HexGridView_" + data.Id)
                        .AddComponent<HexGridView>();
                hexGridView.transform.SetParent(transform);
                hexGridView.Initialize(config.TilePrefab, data, _gameStartConfiguration);
            }
        }
    }
}