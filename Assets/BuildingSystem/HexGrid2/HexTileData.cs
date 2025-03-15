using UnityEngine;

/// <summary>
/// Reine Daten pro Feld. Kann beliebig erweitert werden 
/// (z.B. Terrain-Typ, Einheit, Gebäude, etc.)
/// </summary>
public class HexTileData
{
    public (int, int, int) HexCoord; // q, r, h
    public bool IsOccupied;

    public HexTileData((int, int, int) hexCoord)
    {
        HexCoord = hexCoord;
        IsOccupied = false;
    }
}