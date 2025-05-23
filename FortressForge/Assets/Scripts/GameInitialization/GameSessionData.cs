using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Holds all the data for the current game session.
    /// Instance-based so you can inject it, serialize it, or attach it to a GameObject.
    /// </summary>
    public class GameSessionData : MonoBehaviour
    {
        // ------------------------------------------------------------
        // Singleton plumbing (optional—only if you want one global).
        // ------------------------------------------------------------
        public static GameSessionData Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // -------------------------
        // Session fields & methods
        // -------------------------
        
        [Tooltip("The current player's ID (-1 if none).")]
        public int PlayerId = -1;

        [SerializeField]
        [Tooltip("Pair each player to the hex-grid they're on.")]
        private List<GridPlayerIdPair> _gridPlayerIdPairs = new List<GridPlayerIdPair>();

        /// <summary>
        /// Read-only list of (player, grid) tuples.
        /// </summary>
        public IReadOnlyList<(int PlayerId, int HexGridId)> GridPlayerIdTuples =>
            _gridPlayerIdPairs
                .Select(p => (p.PlayerId, p.HexGridId))
                .ToList();
        
        /// <summary>
        /// Add or update the mapping of a player to a grid.
        /// </summary>
        public void SetPlayerGrid(int playerId, int hexGridId)
        {
            var existing = _gridPlayerIdPairs.FirstOrDefault(p => p.PlayerId == playerId);
            if (existing != null)
                existing.HexGridId = hexGridId;
            else
                _gridPlayerIdPairs.Add(new GridPlayerIdPair { PlayerId = playerId, HexGridId = hexGridId });
        }

        /// <summary>
        /// Clears all player-grid mappings.
        /// </summary>
        public void ClearMappings() => _gridPlayerIdPairs.Clear();

        [Serializable]
        private class GridPlayerIdPair
        {
            public int PlayerId;
            public int HexGridId;
        }
    }
}