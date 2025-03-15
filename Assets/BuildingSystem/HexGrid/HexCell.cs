using UnityEngine;
using UnityEngine.Serialization;


public class HexCell : ScriptableObject {
    public int q; // Spalte
    public int r; // Reihe
    public int h; // Höhe
    public int s { get { return -q - r; } } // Diagonale
    public Vector3 position { get; set; }
    public bool isOccupied { get; set; }

    public HexCell(int q, int r, int h)
    {
        this.q = q;
        this.r = r;
        this.h = h;
        this.position = Vector3.zero;
        this.isOccupied = false;
    }
}
