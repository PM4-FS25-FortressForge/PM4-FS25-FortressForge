using UnityEngine;

/// <summary>
/// Stellt ein einzelnes Hex-Feld dar.
/// </summary>
public class HexTileView : MonoBehaviour
{
    private HexTileData tileData;
    private Color _freeColor = Color.green; // Wird beim Init korrekt gesetzt.
    private Color _occupiedColor = Color.red;
    private Color _highlightColor = Color.yellow;
    
    public Material FREE_MATERIAL;
    public Material OCCUPIED_MATERIAL;
    public Material HIGHLIGHT_MATERIAL;
    
    private MeshRenderer _renderer;
    
    public void Init(HexTileData data)
    {
        tileData = data;
        _renderer = GetComponentInChildren<MeshRenderer>();
        UpdateVisuals();
    }
    
    public void UpdateVisuals()
    {
        _renderer.material = tileData.IsOccupied ? OCCUPIED_MATERIAL : FREE_MATERIAL;
    }

    /// <summary>
    /// Setzt einen "Hover"-Effekt auf Orange, oder setzt die ursprüngliche Farbe wieder zurück.
    /// </summary>
    public void UpdateVisuals(bool highlight) {
        if (highlight)
            _renderer.material = HIGHLIGHT_MATERIAL;
        else {
            _renderer.material = tileData.IsOccupied ? OCCUPIED_MATERIAL : FREE_MATERIAL;
        }
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