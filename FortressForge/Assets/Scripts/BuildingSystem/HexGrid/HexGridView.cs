using UnityEngine;
using System.Collections.Generic;

namespace FortressForge.BuildingSystem.HexGrid {
    /// <summary>
    /// Ein MonoBehaviour, das einem HexGrid Daten entnimmt und
    /// dafür die Darstellung in der Szene erzeugt.
    /// </summary>
    public class HexGridView : MonoBehaviour {
        private HexGridData _hexGrid { get; set; }
        private GameObject _tilePrefab { get; set; }

        private readonly Dictionary<(int, int, int), HexTileView> _tileViews = new ();

        // Merkt sich das aktuell "gehoverte" Tile, damit wir es beim Verlassen zurücksetzen können.
        private HexTileView currentlyHoveredTile;

        /// <summary>
        /// Erzeugt die visuellen Hex-Tiles basierend auf den Grid-Daten.
        /// </summary>
        public void BuildGridView(GameObject tilePrefab, HexGridData hexGrid) {
            _tilePrefab = tilePrefab;
            _hexGrid = hexGrid;

            if (_hexGrid == null || _tilePrefab == null) {
                Debug.LogWarning("HexGridView: Keine gültigen Referenzen für Grid oder Prefab!");
                return;
            }

            InitializeHexGridView();
            UpdateHexGridView();
        }

        /// <summary>
        /// Erzeugt bzw. aktualisiert alle Tiles im Hexgrid.
        /// </summary>
        public void UpdateHexGridView() {
            // Jetzt einmal durch alle Einträge des HexGrid loopen:
            foreach (var kvp in _hexGrid.AllTiles) {
                (int q, int r, int h) = kvp.Key;
                HexTileView hexTileView = _tileViews[(q, r, h)];
                hexTileView.UpdateVisuals();

                bool canRender = ShouldRenderTile((q, r, h));
                SetupTileVisibility(hexTileView, canRender);
            }
        }

        /// <summary>
        /// Initialisiert die Hex-Tiles im Hexgrid.
        /// </summary>
        private void InitializeHexGridView() {
            foreach (var kvp in _hexGrid.AllTiles) {
                (int q, int r, int h) = kvp.Key;
                HexTileData tileData = kvp.Value;

                HexTileView hexTileView = InitializeTile(tileData, (q, r, h));
                _tileViews[(q, r, h)] = hexTileView;
            }
        }

        /// <summary>
        /// Aktiviert/Deaktiviert Renderer und Collider, damit
        /// ein ausgeblendetes Tile weder gezeichnet wird noch Raycasts blockiert.
        /// </summary>
        private void SetupTileVisibility(HexTileView tileObj, bool canRender) {
            // Renderer an-/abschalten
            Renderer rend = tileObj.GetComponent<Renderer>();
            if (rend != null) {
                rend.enabled = canRender;
            }

            // Collider an-/abschalten
            Collider col = tileObj.GetComponent<Collider>();
            if (col != null) {
                col.enabled = canRender;
            }
        }

        /// <summary>
        /// Initialisiert das HexTileView-Skript (falls vorhanden)
        /// und merkt es sich im Dictionary.
        /// </summary>
        private HexTileView InitializeTile(HexTileData tileData, (int, int, int) coord) {
            Vector3 worldPos = CalculateWorldPosition(coord, _hexGrid.origin);

            // Instanziere das Prefab
            // Du könntest vorher prüfen: "Haben wir das Tile (q,r,h) schon in tileViews?"
            GameObject tileObj = Instantiate(
                _tilePrefab,
                worldPos,
                _tilePrefab.transform.rotation,
                this.transform
            );

            HexTileView tileView = tileObj.GetComponent<HexTileView>();
            tileView.Init(tileData);
            _tileViews[coord] = tileView;

            return tileView;
        }

        /// <summary>
        /// Prüft, ob ein Tile an den gegebenen Koordinaten gerendert werden soll.
        /// In diesem Beispiel: Immer wenn h = 0 (Boden) oder das darunterliegende
        /// Tile 'IsOccupied = true' ist.
        /// </summary>
        private bool ShouldRenderTile((int q, int r, int h) coord) {
            // Boden-Level (kein Tile darunter erforderlich)
            if (coord.h == 0)
                return true;

            // Tile darunter bestimmen
            var belowCoord = (coord.q, coord.r, coord.h - 1);
            if (_hexGrid.AllTiles.TryGetValue(belowCoord, out HexTileData belowTileData)) {
                // Nur rendern, wenn das darunterliegende Tile belegt ist
                return belowTileData.isOccupied;
            }

            // Gibt es gar kein darunterliegendes Tile, wird auch nicht gerendert
            return false;
        }

        private void Update() {
            // Per Raycast herausfinden, ob wir ein Tile unter dem Mauszeiger haben
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f)) {
                // Prüfen, ob ein HexTileView getroffen wurde
                var hitTileView = hit.collider.GetComponentInParent<HexTileView>();

                if (hitTileView != null) {
                    // Wenn es ein neues Tile ist, highlighten wir es
                    if (hitTileView != currentlyHoveredTile) {
                        // Altes Tile (falls vorhanden) zurücksetzen
                        if (currentlyHoveredTile != null)
                            currentlyHoveredTile.UpdateVisuals(false);

                        // Neues Tile highlighten
                        hitTileView.UpdateVisuals(true);
                        currentlyHoveredTile = hitTileView;
                    }
                }
                else {
                    // Wir haben etwas getroffen, aber kein HexTileView
                    ClearHoveredTile();
                }
            }
            else {
                // Nichts getroffen oder Maus außerhalb – ggf. das alte Hover-Tile zurücksetzen
                ClearHoveredTile();
            }
        }

        private void ClearHoveredTile() {
            if (currentlyHoveredTile != null) {
                currentlyHoveredTile.UpdateVisuals(false);
                currentlyHoveredTile = null;
            }
        }

        /// <summary>
        /// Beispielhafte Umrechnung von 3D-Koords in Weltposition für "flache" Hex.
        /// </summary>
        private Vector3 CalculateWorldPosition((int, int, int) coord, Vector3 origin) {
            float x = _hexGrid.tileRadius * 3f / 2f * coord.Item1;
            float z = _hexGrid.tileRadius * Mathf.Sqrt(3) * (coord.Item2 + coord.Item1 / 2f);
            // -> Die dritte Koordinate (coord.Item3) benutzen wir als "Stockwerk" (Höhe)
            return new Vector3(x, coord.Item3 * _hexGrid.tileHeight, z) + origin;
        }

        /// <summary>
        /// Blendet alle Tiles eines bestimmten "Stockwerks" ein oder aus,
        /// damit ein Raycast ggf. "durch" die höheren Stockwerke gehen kann.
        /// </summary>
        public void SetFloorVisibility(int floorLevel, bool visible) {
            foreach (var kvp in _tileViews) {
                var c = kvp.Key; // -> (q, r, h)
                var tileView = kvp.Value; // -> Das passende Tile
                if (c.Item3 == floorLevel) {
                    tileView.gameObject.SetActive(visible);
                }
            }

            // Wenn wir gerade ein Tile ausblenden, das aktuell gehighlightet war,
            // sollten wir das Hover zurücksetzen:
            if (currentlyHoveredTile != null &&
                currentlyHoveredTile.FloorLevel == floorLevel &&
                !visible) {
                ClearHoveredTile();
            }
        }
    }
}
