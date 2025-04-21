using FortressForge.HexGrid;
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
        }
    }
}