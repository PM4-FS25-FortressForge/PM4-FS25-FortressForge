﻿using FishNet;
using FishNet.Managing.Scened;
using UnityEngine;

namespace FortressForge.Network
{
    /// <summary>
    /// Manages the loading and unloading of scenes for all clients
    /// </summary>
    public class BootstrapSceneManager : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// Loads a scene for all clients
        /// </summary>
        /// <param name="sceneName">The name of the scene to load</param>
        public void LoadScene(string sceneName)
        {
            if (!InstanceFinder.IsServerStarted)
            {
                Debug.Log("Server not started. Cannot load scene.");
                return;
            }

            SceneLoadData sceneLoadData = new(sceneName);
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
        }

        /// <summary>
        /// Unloads a scene for all clients
        /// </summary>
        /// <param name="sceneName">The name of the scene to unload</param>
        public void UnloadScene(string sceneName)
        {
            if (!InstanceFinder.IsServerStarted) 
            {
                Debug.Log("Server not started. Cannot unload scene.");
                return;
            }
            
            SceneUnloadData sceneUnloadData = new(sceneName);
            InstanceFinder.SceneManager.UnloadGlobalScenes(sceneUnloadData);
        }
    }
}