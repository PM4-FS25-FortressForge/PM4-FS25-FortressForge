using System.Collections.Generic;
using System.Linq;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FortressForge.GameInitialization
{
    [CreateAssetMenu(fileName = "GameStartConfiguration", menuName = "Configurations/GameStartConfiguration")]
    public class GameStartConfiguration : ScriptableObject
    {
        [Header("Map")]
        public Terrain Terrain;
        
        [Header("Available Buildings")]
        public List<BaseBuildingTemplate> availableBuildings = new();
        
        [Header("HexGrid Konfiguration")]
        public int Radius;
        public float TileSize;
        public float TileHeight;
        
        [Header("Referenzen für das GridView")] [SerializeField]
        public GameObject TilePrefab;
    }
}