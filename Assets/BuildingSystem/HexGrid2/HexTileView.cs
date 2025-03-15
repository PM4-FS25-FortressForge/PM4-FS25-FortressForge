using UnityEngine;

/// <summary>
/// Stellt ein einzelnes Hex-Feld dar.
/// </summary>
public class HexTileView : MonoBehaviour
{
    private HexTileData tileData;

    public void Init(HexTileData data)
    {
        tileData = data;
        UpdateVisuals();
    }

    /// <summary>
    /// Hier z.B. Farbe, Text oder Mesh je nach TileData anpassen
    /// </summary>
    private void UpdateVisuals()
    {
        // Beispiel: Andere Farbe, wenn IsBlocked = true
        // Achtung: MeshRenderer muss am selben Objekt oder Kindobjekt hängen
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            if (tileData.IsOccupied)
                renderer.material.color = Color.red;
            else
                renderer.material.color = Color.green;
        }
    }

    // Hier kann man Klick-Events oder MouseOver-Logik einbauen (OnMouseDown etc.),
    // um Interaktionen zu ermöglichen.
}