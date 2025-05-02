using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;

namespace FortressForge.GameInitialization
{
    public class ServerInitializationManager : MonoBehaviour
    {
        private static ServerInitializationManager Instance { get; set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartGame();
        }

        private void StartGame()
        {
            if (InstanceFinder.IsServerStarted)
            {
                Debug.Log("Server startet das Spiel.");
                foreach (KeyValuePair<int, NetworkConnection> client in InstanceFinder.ServerManager.Clients)
                {
                    Debug.Log($"Spawne PlayerManager für Client: {client.Value.ClientId}");
                    GameObject playerManager = Instantiate(Resources.Load<GameObject>("Prefabs/PlayerManager"));
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