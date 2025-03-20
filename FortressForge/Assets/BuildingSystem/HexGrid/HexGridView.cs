using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ein MonoBehaviour, das einem HexGrid Daten entnimmt und
/// dafür die Darstellung in der Szene erzeugt.
/// </summary>
public class HexGridView : MonoBehaviour
{
    private HexGrid HexGrid { get; set; }
    private GameObject TilePrefab { get; set; }

    // Speichert alle erzeugten TileViews mit ihren Koordinaten als Schlüssel.
    private Dictionary<(int, int, int), HexTileView> tileViews 
        = new Dictionary<(int, int, int), HexTileView>();

    // Merkt sich das aktuell "gehoverte" Tile, damit wir es beim Verlassen zurücksetzen können.
    private HexTileView currentlyHoveredTile;

    /// <summary>
    /// Erzeugt die visuellen Hex-Tiles basierend auf den Grid-Daten.
    /// </summary>
    public void BuildGridView(GameObject tilePrefab, HexGrid hexGrid)
    {
        TilePrefab = tilePrefab;
        HexGrid = hexGrid;

        if (HexGrid == null || TilePrefab == null)
        {
            Debug.LogWarning("HexGridView: Keine gültigen Referenzen für Grid oder Prefab!");
            return;
        }
        UpdateHexGridView();
    }

    private void UpdateHexGridView() {
        foreach (var kvp in HexGrid.AllTiles) {
            (int, int, int) coord = kvp.Key;
            HexTileData tileData = kvp.Value;

            // Berechne eine Position in der Welt.
            Vector3 worldPos = CalculateWorldPosition(coord, HexGrid.Origin);

            // Erzeuge ein Tile-Objekt in der Szene
            GameObject tileObj = Instantiate(TilePrefab, worldPos, TilePrefab.transform.rotation, this.transform);

            // Falls das Prefab ein HexTileView-Skript hat, übergeben wir die Daten
            HexTileView tileView = tileObj.GetComponent<HexTileView>();
            if (tileView != null) {
                tileView.Init(tileData);
                tileViews[coord] = tileView;
            }
        }
    }
    
    private (int, int, int) CalculateGridCoord(Vector3 worldPos, Vector3 origin)
    {
        // Verschieben in den lokalen Raum (ursprüngliche Nullpunktlage):
        Vector3 localPos = worldPos - origin;
 
        // q, r und h berechnen (Floating-Point)
        float qFloat = localPos.x / (HexGrid.TileRadius * 1.5f);
        float rFloat = (localPos.z / (HexGrid.TileRadius * Mathf.Sqrt(3))) - (qFloat / 2f);
        float hFloat = localPos.y / HexGrid.TileHeight;
 
        // Auf int runden:
        int q = Mathf.RoundToInt(qFloat);
        int r = Mathf.RoundToInt(rFloat);
        int h = Mathf.RoundToInt(hFloat);
 
        return (q, r, h);
    }

    private void Update()
    {
        // 1) Per Raycast herausfinden, ob wir ein Tile unter dem Mauszeiger haben
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            // Prüfen, ob ein HexTileView getroffen wurde
            var hitTileView = hit.collider.GetComponentInParent<HexTileView>();

            if (hitTileView != null)
            {
                // Wenn es ein neues Tile ist, highlighten wir es
                if (hitTileView != currentlyHoveredTile)
                {
                    // Altes Tile (falls vorhanden) zurücksetzen
                    if (currentlyHoveredTile != null)
                        currentlyHoveredTile.SetHighlight(false);

                    // Neues Tile highlighten
                    hitTileView.SetHighlight(true);
                    currentlyHoveredTile = hitTileView;
                }
            }
            else
            {
                // Wir haben etwas getroffen, aber kein HexTileView
                ClearHoveredTile();
            }
        }
        else
        {
            // Nichts getroffen oder Maus außerhalb – ggf. das alte Hover-Tile zurücksetzen
            ClearHoveredTile();
        }
    }

    private void ClearHoveredTile()
    {
        if (currentlyHoveredTile != null)
        {
            currentlyHoveredTile.SetHighlight(false);
            currentlyHoveredTile = null;
        }
    }

    /// <summary>
    /// Beispielhafte Umrechnung von 3D-Koords in Weltposition für "flache" Hex.
    /// </summary>
    private Vector3 CalculateWorldPosition((int, int, int) coord, Vector3 origin)
    {
        float x = HexGrid.TileRadius * 3f / 2f * coord.Item1;
        float z = HexGrid.TileRadius * Mathf.Sqrt(3) * (coord.Item2 + coord.Item1 / 2f);
        // -> Die dritte Koordinate (coord.Item3) benutzen wir als "Stockwerk" (Höhe)
        return new Vector3(x, coord.Item3 * HexGrid.TileHeight, z) + origin;
    }

    /// <summary>
    /// Blendet alle Tiles eines bestimmten "Stockwerks" ein oder aus,
    /// damit ein Raycast ggf. "durch" die höheren Stockwerke gehen kann.
    /// </summary>
    public void SetFloorVisibility(int floorLevel, bool visible)
    {
        foreach (var kvp in tileViews)
        {
            var c = kvp.Key;           // -> (q, r, h)
            var tileView = kvp.Value;  // -> Das passende Tile
            if (c.Item3 == floorLevel)
            {
                tileView.gameObject.SetActive(visible);
            }
        }

        // Wenn wir gerade ein Tile ausblenden, das aktuell gehighlightet war,
        // sollten wir das Hover zurücksetzen:
        if (currentlyHoveredTile != null && 
            currentlyHoveredTile.FloorLevel == floorLevel && !visible)
        {
            ClearHoveredTile();
        }
    }
}
