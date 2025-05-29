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
        
        [Header("Players")]
        public List<string> PlayerNames = new();
        
        [Header("Available Buildings")]
        public List<BaseBuildingTemplate> availableBuildings = new();
        
        [Header("Start Building")]
        public BaseBuildingTemplate StarterBuildingTemplate;
        
        [Header("HexGrid Konfiguration")]
        public int GridRadius;
        public float TileSize;
        public float TileHeight;
        
        [Header("Referenzen für das GridView")] [SerializeField]
        public GameObject TilePrefab;

        [Header("GameObject Materials")] 
        public Material FreeMaterial;
        public Material OccupiedMaterial;
        public Material HighlightMaterial;
        public Material NotAllowedMaterial;
        public Material PreviewMaterial;
        public Material MeshTilesMaterial;
        public Material UnownedHexMaterial;
        
        [Header("Game Rules")]
        public float GlobalMagmaAmount = 100000;
    }
}