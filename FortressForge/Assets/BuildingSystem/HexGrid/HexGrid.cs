using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Repräsentiert die reine Datenstruktur eines Hexfeld-Grids.
/// Keine direkte Rendering-Logik!
/// </summary>
public class HexGrid
{
    public int GridId { get; private set; }
    public Vector3 Origin { get; private set; }
    public string OwnerId { get; set; }
    
    /// <summary>
    /// Grösse der HexTiles werden hier definiert, damit in Zukunft auch unterschiedliche
    /// Tile-Grössen für unterschiedliche Spieler möglich ist (bspw. für KI).
    /// </summary>
    public float TileRadius { get; private set; }
    public float TileHeight { get; private set; }

    // Einfache Beispiel-Sammlung für Tile-Daten (key: hex-Koordinate)
    private Dictionary<(int, int, int), HexTileData> tiles 
        = new Dictionary<(int, int, int), HexTileData>();

    /// <summary>
    /// Zugriff auf alle Tiles (z.B. für die View)
    /// </summary>
    public Dictionary<(int, int, int), HexTileData> AllTiles => tiles;

    public HexGrid(int gridId, Vector3 origin, int radius, int height, float tileSize, float tileHeight)
    {
        GridId = gridId;
        Origin = origin;
        TileRadius = tileSize;
        TileHeight = tileHeight;

        // Beispiel: Einfaches rechteckiges Gitter in axialen oder "2D" Koordinaten
        // In echten Hex-Systemen musst du evtl. "odd-r" oder "even-r" Offsets beachten.
        for (int h = 0; h < height; h++)
        {
            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Math.Max(-radius, -q - radius);
                int r2 = Math.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    (int, int, int) coord = (q, r, h);
                    tiles[coord] = new HexTileData(coord);
                }
            }
        }
    }
    
    public bool ValidateBuidlingPlacement((int, int, int) hexCoord, BaseBuilding building) {
        foreach (var kvp in building.shapeData) {
            (int, int, int) coord = (kvp.r, kvp.q, kvp.h);
            HexTileData tileData = GetTileData((coord.Item1 + hexCoord.Item1, coord.Item2 + hexCoord.Item2, coord.Item3 + hexCoord.Item3));
            if (tileData == null || tileData.IsOccupied) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gibt die Daten eines bestimmten Hex-Feldes zurück, falls vorhanden.
    /// </summary>
    public HexTileData GetTileData((int, int, int) hexCoord)
    {
        if (tiles.TryGetValue(hexCoord, out HexTileData data))
        {
            return data;
        }
        return null;
    }

    /// <summary>
    /// Setzt (oder aktualisiert) die Daten eines bestimmten Hex-Feldes.
    /// </summary>
    public void SetTileData((int, int, int) hexCoord, HexTileData newData)
    {
        if (tiles.ContainsKey(hexCoord))
        {
            tiles[hexCoord] = newData;
        }
    }

    // Hier kann man umfangreichere Logik ergänzen (z.B. Bewegung, Pfadfindung, Events ...)
}