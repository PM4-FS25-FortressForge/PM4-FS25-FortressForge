using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Manages the initialization of the server and the start of the game.
    /// </summary>
    public class ServerInitializationManager : MonoBehaviour
    {
        private static ServerInitializationManager Instance { get; set; }
        
        [SerializeField]
        private GameObject PlayerManagerPrefab;

        /// <summary>
        /// Ensures that only one instance of the ServerInitializationManager exists (Singleton pattern).
        /// If an instance already exists, destroys the duplicate.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scene loads
            }
            else
            {
                Destroy(gameObject); // Destroy duplicate instance
            }
        }

        /// <summary>
        /// Called on script start. Initiates the game start logic.
        /// </summary>
        private void Start()
        {
            StartGame();
        }

        /// <summary>
        /// Starts the game by executing logic for the server and client.
        /// - If the server is active, PlayerManager objects are spawned for all connected clients.
        /// - If the client is active, corresponding logic is executed (currently empty).
        /// </summary>
        private void StartGame()
        {
            if (InstanceFinder.IsServerStarted)
            {
                Debug.Log("Server startet das Spiel.");
                foreach (KeyValuePair<int, NetworkConnection> client in InstanceFinder.ServerManager.Clients)
                {
                    Debug.Log($"Spawn PlayerManager for Client: {client.Value.ClientId}");
                    GameObject playerManager = Instantiate(PlayerManagerPrefab);
                    InstanceFinder.ServerManager.Spawn(playerManager, client.Value);
                }
            }
            else if (InstanceFinder.IsClientStarted)
            {
                Debug.Log("Client ist bereit.");
                // Fügen Sie hier die Logik für den Client hinzu
            }
            else
            {
                Debug.Log("Server und Client sind nicht gestartet.");
            }
        }
    }
}