using UnityEngine;

/// <summary>
/// Ein MonoBehaviour, das einem HexGrid Daten entnimmt und 
/// dafür die Darstellung in der Szene erzeugt.
/// </summary>
public class HexGridView : MonoBehaviour
{
    private HexGrid HexGrid { get; set; }
    
    private GameObject TilePrefab { get; set; }

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

        foreach (var kvp in HexGrid.AllTiles)
        {
            (int, int, int) coord = kvp.Key;
            HexTileData tileData = kvp.Value;

            // Berechne eine (sehr einfache) Position in der Welt.
            // In einem echten Hex-Layout solltest du Offsets und "odd/even rows" o. ä. beachten.
            Vector3 worldPos = CalculateWorldPosition(coord, HexGrid.Origin);

            // Erzeuge ein Tile-Objekt in der Szene
            GameObject tileObj = Instantiate(TilePrefab, worldPos, TilePrefab.transform.rotation, this.transform);

            // Falls das Prefab ein HexTileView-Skript hat, übergeben wir die Daten
            HexTileView tileView = tileObj.GetComponent<HexTileView>();
            if (tileView != null)
            {
                tileView.Init(tileData);
            }
        }
    }

    /// <summary>
    /// Beispielhafte Umrechnung von 3D-Koords in Weltposition für "flache" Hex.
    /// </summary>
    private Vector3 CalculateWorldPosition((int, int, int) coord, Vector3 origin)
    {
        float x = HexGrid.TileRadius * 3f / 2f * coord.Item1;
        float z = HexGrid.TileRadius * Mathf.Sqrt(3) * (coord.Item2 + coord.Item1 / 2f);
        return new Vector3(x, coord.Item3 * HexGrid.TileHeight, z) + origin;
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
}
