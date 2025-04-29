using System.Collections.Generic;
using System.Linq;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Contains information needed to start a play session.
    /// Such as Player Grid Assignments, Game Mode, and other configurations.
    /// </summary>
    [CreateAssetMenu(fileName = "PlaySessionStartConfiguration", menuName = "Configurations/PlaySessionStartConfiguration")]
    public class GameSessionStartConfiguration : ScriptableObject
    {
        [Header("This PlayerID")]
        public int PlayerId;
        
        [Header("PlayerIds und ihre HexGrid Zugehörigkeit")]
        [SerializeField]
        private List<GridPlayerIdPair> _gridPlayerIdPairs;
        
        public List<(int PlayerId, int HexGridId)> GridPlayerIdTuples =>
            _gridPlayerIdPairs.Select(p => (p.PlayerId, p.HexGridId)).ToList();
        
        [Header("HexGrid origin Koordinaten")]
        public List<Vector3> HexGridOrigins;
        
        /// <summary>
        /// Represents a pair of PlayerId and HexGridId.
        /// Made for unity serialization.
        /// </summary>
        [System.Serializable]
        public class GridPlayerIdPair
        {
            public int PlayerId, HexGridId;
        }
    }
}