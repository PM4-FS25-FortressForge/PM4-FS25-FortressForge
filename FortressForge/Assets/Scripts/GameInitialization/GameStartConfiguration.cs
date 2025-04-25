using System.Collections.Generic;
using System.Linq;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;
using UnityEngine.UI;

namespace FortressForge.GameInitialization
{
    [CreateAssetMenu(fileName = "GameStartConfiguration", menuName = "Configurations/GameStartConfiguration")]
    public class GameStartConfiguration : ScriptableObject
    {
        [Header("Map")]
        public Terrain Terrain;
        
        [Header("This PlayerID")]
        public int PlayerId;
        
        [Header("PlayerIds und ihre HexGrid Zugehörigkeit")]
        [SerializeField]
        private List<SerializablePair> _serializedList;
        public List<(int PlayerId, int HexGridId)> PlayerIdsHexGridIdTuplesList =>
            _serializedList.Select(p => (p.PlayerId, p.HexGridId)).ToList();
        
        [Header("HexGrid origin Koordinaten")]
        public List<Vector3> HexGridOrigins;
        
        [Header("Available Buildings")]
        public List<BaseBuildingTemplate> availableBuildings = new();
        
        [Header("HexGrid Konfiguration")]
        public int Radius;
        public float TileSize;
        public float TileHeight;
        
        [Header("Referenzen für das GridView")] [SerializeField]
        public GameObject TilePrefab;
        
        [System.Serializable]
        public class SerializablePair
        {
            public int PlayerId, HexGridId;
        }
    }
}