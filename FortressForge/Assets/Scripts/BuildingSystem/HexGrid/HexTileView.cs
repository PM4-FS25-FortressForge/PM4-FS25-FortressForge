using UnityEngine;

namespace FortressForge.BuildingSystem.HexGrid {
    /// <summary>
    /// Stellt ein einzelnes Hex-Feld dar.
    /// </summary>
    public class HexTileView : MonoBehaviour {
        private HexTileData tileData;

        public Material freeMaterial;
        public Material occupiedMaterial;
        public Material highlightMaterial;

        private MeshRenderer _renderer;

        public void Init(HexTileData data) {
            tileData = data;
            _renderer = GetComponentInChildren<MeshRenderer>();
            UpdateVisuals();
        }

        public void UpdateVisuals() {
            _renderer.material = tileData.isOccupied ? occupiedMaterial : freeMaterial;
        }

        /// <summary>
        /// Setzt einen "Hover"-Effekt auf Orange, oder setzt die ursprüngliche Farbe wieder zurück.
        /// </summary>
        public void UpdateVisuals(bool highlight) {
            if (highlight)
                _renderer.material = highlightMaterial;
            else {
                _renderer.material = tileData.isOccupied ? occupiedMaterial : freeMaterial;
            }
        }

        /// <summary>
        /// Optional: Gibt das "Stockwerk" zurück – falls in tileData die Koord.Tupel (q,r,h) stecken.
        /// </summary>
        public int FloorLevel {
            get {
                // Beispiel: Wir nehmen an, tileData.Coord = (q, r, h)
                return tileData.hexCoord.Item3;
            }
        }
    }
}