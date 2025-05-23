using System;
using System.Collections.Generic;
using System.Linq;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Singleton that holds all the data for the current game session.
    /// Pure C#—can be new’d up in tests, but by default you access GameSessionData.Instance.
    /// </summary>
    [Serializable]
    public class GameSessionData
    {
        // Lazy<T> ensures thread-safe lazy initialization
        private static readonly Lazy<GameSessionData> _lazyInstance =
            new(() => new GameSessionData());

        /// <summary>
        /// The one and only instance.
        /// </summary>
        public static GameSessionData Instance => _lazyInstance.Value;

        // Private constructor prevents external instantiation
        private GameSessionData() { }

        /// <summary>
        /// The current player's ID (-1 if none).
        /// </summary>
        public int PlayerId { get; set; } = -1;

        [Serializable]
        private class GridPlayerIdPair
        {
            public int PlayerId;
            public int HexGridId;
        }

        // Backing list
        private readonly List<GridPlayerIdPair> _gridPlayerIdPairs = new List<GridPlayerIdPair>();

        /// <summary>
        /// Read-only snapshot of (PlayerId, HexGridId) tuples.
        /// </summary>
        public IReadOnlyList<(int PlayerId, int HexGridId)> GridPlayerIdTuples =>
            _gridPlayerIdPairs.Select(p => (p.PlayerId, p.HexGridId)).ToList();

        /// <summary>
        /// Map a player to a hex-grid. Adds if new, updates if exists.
        /// </summary>
        public void SetPlayerGrid(int playerId, int hexGridId)
        {
            var existing = _gridPlayerIdPairs.FirstOrDefault(p => p.PlayerId == playerId);
            if (existing != null)
                existing.HexGridId = hexGridId;
            else
                _gridPlayerIdPairs.Add(new GridPlayerIdPair
                {
                    PlayerId = playerId,
                    HexGridId = hexGridId
                });
        }

        /// <summary>
        /// Clears all player-grid mappings.
        /// </summary>
        public void ClearMappings() => _gridPlayerIdPairs.Clear();
    }
}
