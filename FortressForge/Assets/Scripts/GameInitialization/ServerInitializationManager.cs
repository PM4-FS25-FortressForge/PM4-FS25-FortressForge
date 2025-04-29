using System;
using FishNet;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.Economy;
using FortressForge.HexGrid;
using UnityEngine;

namespace FortressForge.GameInitialization
{
    public class ServerInitializationManager : NetworkBehaviour
    {
        public static ServerInitializationManager Instance { get; private set; }

        /// <summary>
        /// Gets called once when the server starts.
        /// Server initialization is done here.
        /// </summary>
        public override void OnStartServer() // TODO Consider removing this if no use case arises
        {
            Instance = this;
        }
    }
}