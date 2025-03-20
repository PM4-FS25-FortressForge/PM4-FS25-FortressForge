using UnityEngine;

/// <summary>
/// Stellt ein einzelnes Hex-Feld dar.
/// </summary>
public class HexTileView : MonoBehaviour
{
    private HexTileData tileData;
    private Color defaultColor = Color.green; // Wird beim Init korrekt gesetzt.

    public void Init(HexTileData data)
    {
        tileData = data;
        var renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            // Speichere die beim Instantiieren vorhandene Farbe,
            // damit wir später wieder auf "Normal" zurück können.
            // defaultColor = renderer.material.color;
        }

        UpdateVisuals();
    }

    /// <summary>
    /// Hier z.B. Farbe, Text oder Mesh je nach TileData anpassen.
    /// </summary>
    private void UpdateVisuals()
    {
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = tileData.IsOccupied ? Color.red : defaultColor;
        }
    }

    /// <summary>
    /// Setzt einen "Hover"-Effekt auf Orange, oder setzt die ursprüngliche Farbe wieder zurück.
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        var renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer == null) return;
        renderer.material.color = tileData.IsOccupied ? Color.red : Color.yellow;
    }

    /// <summary>
    /// Optional: Gibt das "Stockwerk" zurück – falls in tileData die Koord.Tupel (q,r,h) stecken.
    /// </summary>
    public int FloorLevel
    {
        get
        {
            // Beispiel: Wir nehmen an, tileData.Coord = (q, r, h)
            return tileData.HexCoord.Item3;
        }
    }
}