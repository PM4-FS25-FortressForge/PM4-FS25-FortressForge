using System.Collections.Generic;
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
        public string PlayerId;
        
        [Header("PlayerIds und ihre HexGrid Zugehörigkeit")]
        public List<(int PlayerId, int HexGridId)> PlayerIdsHexGridIdTuplesList;
        
        [Header("HexGrid origin Koordinaten")]
        public List<Vector3> HexGridOrigins;
        
        [Header("Available Buildings")]
        public List<BaseBuildingTemplate> availableBuildings = new List<BaseBuildingTemplate>();
        
        [Header("HexGrid Konfiguration")]
        public int Radius;
        public float TileSize;
        public float TileHeight;
        
        [Header("Referenzen für das GridView")] [SerializeField]
        public GameObject TilePrefab;
    }
}