using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using System.Collections.Generic;
using UnityEngine;
    
namespace FortressForge.GameInitialization
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private GameStartConfiguration _gameStartConfiguration;
        [SerializeField] private GlobalObjectInitializationManager _globalInit;
        [SerializeField] private PlayerInitializationManager _playerInit;

        // Auf dem Server geloggte Positionen pro Spieler
        private Dictionary<int, Vector3> _corePositions = new Dictionary<int, Vector3>();

        // Anzahl erwarteter Spieler aus der Start‐Config
        private int _expectedPlayerCount;

        private void Start()
        {
            _expectedPlayerCount = _gameStartConfiguration.PlayerNames.Count;
            
            GameSessionStartConfiguration gameSessionStartConfig = ScriptableObject.CreateInstance<GameSessionStartConfiguration>();
            
        }
    }
}