using FortressForge.HexGrid;
using FortressForge.HexGrid.View;
using UnityEngine;

namespace FortressForge.GameInitialization
{
    public class GlobalObjectInitializationManager : MonoBehaviour
    {
        [Header("Game Start Configuration")]
        [SerializeField] private GameStartConfiguration _config;
        
        /// <summary>
        /// Initializing all global objects.
        /// </summary>
        public void Awake()
        {
            Instantiate(_config.Terrain);
            gameObject.AddComponent<HexGridManager>();
        }
        
        /// <summary>
        /// Start function to initialize common game systems.
        /// </summary>
        public void Start()
        {
            if (_config == null)
            {
                Debug.LogError("GameStartConfiguration is not set.");
                return;
            }
            
            HexGridManager.Instance.InitializeHexGridForPlayers(_config);
            
            InitializeHexGridViews(_config, HexGridManager.Instance);
        }

        private void InitializeHexGridViews(GameStartConfiguration config, HexGridManager hexGridManager)
        {
            foreach (var data in hexGridManager.AllGrids)
            {
                HexGridView hexGridView = new GameObject("HexGridView_" + data.Id).AddComponent<HexGridView>();
                hexGridView.transform.SetParent(transform);
                hexGridView.Initialize(config.TilePrefab, data);
            }
        }
    }
}