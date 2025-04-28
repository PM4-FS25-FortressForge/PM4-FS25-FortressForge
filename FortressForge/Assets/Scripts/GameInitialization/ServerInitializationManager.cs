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
        
        [Header("Game Start Configuration")]
        [SerializeField] private GameStartConfiguration _config;

        private BuildingManager _buildingManager;
        private EconomyController _economyController;
        private HexGridManager _hexGridManager;

        /// <summary>
        /// Gets called once when the server starts.
        /// Server initialization is done here.
        /// </summary>
        public override void OnStartServer() 
        {
            Instance = this;
        }
    }
}