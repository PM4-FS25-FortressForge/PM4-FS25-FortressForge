using FishNet;
using FishNet.Transporting;
using FortressForge.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FortressForge.Network
{
    /// <summary>
    /// Handles remote connections for the game, including server and client disconnections,
    /// and ensures a clean transition to the new scene when necessary.
    /// </summary>
    public class RemoteConnectionHandler : MonoBehaviour
    {
        private const string NEW_SCENE = "LobbyScene";

        private void Awake()
        {
            if (!InstanceFinder.IsServerStarted)
                InstanceFinder.ClientManager.OnClientConnectionState += HandleServerDisconnect;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (InstanceFinder.IsServerStarted)
                {
                    InstanceFinder.ServerManager.StopConnection(true);
                    LoadCleanScene(NEW_SCENE);
                }
                else if (InstanceFinder.IsClientStarted)
                {
                    InstanceFinder.ClientManager.OnClientConnectionState -= HandleServerDisconnect;
                    InstanceFinder.ClientManager.StopConnection();
                    LoadCleanScene(NEW_SCENE);
                }
            }
        }

        /// <summary>
        /// Handles the server disconnect event. This is called when the client disconnects from the server.
        /// </summary>
        /// <param name="state">The connection state of the client.</param>
        private void HandleServerDisconnect(ClientConnectionStateArgs state)
        {
            InstanceFinder.ClientManager.OnClientConnectionState -= HandleServerDisconnect;
            InstanceFinder.ClientManager.StopConnection();
            LoadCleanScene(NEW_SCENE);
        }

        /// <summary>
        /// Loads a clean scene by name. This method ensures that all objects in the current scene
        /// are destroyed before transitioning to the new scene.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        private void LoadCleanScene(string sceneName)
        {
            DestroyAllObjects();
            LoadScene(sceneName);
        }

        /// <summary>
        /// Loads a scene by name.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        private void LoadScene(string sceneName)
        {
            UIClickChecker.ResetInstance();
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene(), UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Destroys all objects in the current scene except for the Debug Updater.
        /// This is used to clean up the scene's DontDestroyOnLoad objects before loading a new scene.
        /// </summary>
        private void DestroyAllObjects()
        {
            foreach (var obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj.name != "[Debug Updater]")
                    Destroy(obj);
            }
        }
    }
}