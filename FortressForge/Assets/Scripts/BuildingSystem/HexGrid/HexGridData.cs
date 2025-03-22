using System;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;

namespace FortressForge.BuildingSystem.HexGrid
{
    /// <summary>
    /// Repräsentiert die reine Datenstruktur eines Hexfeld-Grids.
    /// Keine direkte Rendering-Logik!
    /// </summary>
    public class HexGridData {
        public int gridId { get; private set; }
        public Vector3 origin { get; private set; }
        public string ownerId { get; set; }

        /// <summary>
        /// Grösse der HexTiles werden hier definiert, damit in Zukunft auch unterschiedliche
        /// Tile-Grössen für unterschiedliche Spieler möglich ist (bspw. für KI).
        /// </summary>
        public float tileRadius { get; private set; }

        public float tileHeight { get; private set; }

        // Einfache Beispiel-Sammlung für Tile-Daten (key: hex-Koordinate)
        private readonly Dictionary<(int, int, int), HexTileData> _tiles = new ();

        /// <summary>
        /// Zugriff auf alle Tiles (z.B. für die View)
        /// </summary>
        public Dictionary<(int, int, int), HexTileData> AllTiles => _tiles;

        public HexGridData(int gridId, Vector3 origin, int radius, int height, float tileSize, float tileHeight) {
            this.gridId = gridId;
            this.origin = origin;
            tileRadius = tileSize;
            this.tileHeight = tileHeight;

            // Beispiel: Einfaches rechteckiges Gitter in axialen oder "2D" Koordinaten
            // In echten Hex-Systemen musst du evtl. "odd-r" oder "even-r" Offsets beachten.
            for (int h = 0; h < height; h++) {
                for (int q = -radius; q <= radius; q++) {
                    int r1 = Math.Max(-radius, -q - radius);
                    int r2 = Math.Min(radius, -q + radius);
                    for (int r = r1; r <= r2; r++) {
                        (int, int, int) coord = (q, r, h);
                        _tiles[coord] = new HexTileData(coord);
                    }
                }
            }
        }

        public bool ValidateBuidlingPlacement((int, int, int) hexCoord, BaseBuilding building) {
            foreach (var kvp in building.shapeData) {
                (int, int, int) coord = (kvp.r, kvp.q, kvp.h);
                HexTileData tileData = GetTileData((coord.Item1 + hexCoord.Item1, coord.Item2 + hexCoord.Item2,
                    coord.Item3 + hexCoord.Item3));
                if (tileData == null || tileData.isOccupied) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gibt die Daten eines bestimmten Hex-Feldes zurück, falls vorhanden.
        /// </summary>
        public HexTileData GetTileData((int, int, int) hexCoord) {
            if (_tiles.TryGetValue(hexCoord, out HexTileData data)) {
                return data;
            }

            return null;
        }

        /// <summary>
        /// Setzt (oder aktualisiert) die Daten eines bestimmten Hex-Feldes.
        /// </summary>
        public void SetTileData((int, int, int) hexCoord, HexTileData newData) {
            if (_tiles.ContainsKey(hexCoord)) {
                _tiles[hexCoord] = newData;
            }
        }

        // Hier kann man umfangreichere Logik ergänzen (z.B. Bewegung, Pfadfindung, Events ...)
    }
}