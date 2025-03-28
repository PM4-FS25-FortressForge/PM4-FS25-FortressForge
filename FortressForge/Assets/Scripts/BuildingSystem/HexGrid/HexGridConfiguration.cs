using UnityEngine;

namespace FortressForge
{
    [CreateAssetMenu(fileName = "HexGridConfiguration", menuName = "Configurations/HexGridConfiguration")]
    public class HexGridConfiguration : ScriptableObject
    {
        [Header("HexGrid Konfiguration")]
        public int Radius; // TODO Currently this doesnt influence the tile size
        public int Height;
        public float TileSize;
        public float TileHeight;
        
        [Header("Referenzen für das GridView")] [SerializeField]
        public GameObject TilePrefab;
    }
}