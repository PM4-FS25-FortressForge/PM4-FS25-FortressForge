using UnityEngine;

[System.Serializable]
public struct HexCell
{
    public int Q { get; } // Spalte
    public int R { get; } // Reihe
    public int H { get; } // Höhe
    public int S { get { return -Q - R; } } // Diagonale
    public Vector3 Position { get; set; }

    public HexCell(int q, int r, int h)
    {
        this.Q = q;
        this.R = r;
        this.H = h;
        this.Position = Vector3.zero;
    }
}
