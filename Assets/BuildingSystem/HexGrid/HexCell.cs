using UnityEngine;

[System.Serializable]
public struct HexCell
{
    public int q; // Spalte
    public int r; // Reihe
    public int h; // Höhe

    public HexCell(int q, int r, int h)
    {
        this.q = q;
        this.r = r;
        this.h = h;
    }
}
