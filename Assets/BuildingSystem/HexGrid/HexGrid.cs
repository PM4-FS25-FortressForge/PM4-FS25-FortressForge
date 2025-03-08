using System;
using System.Collections.Generic;
using UnityEngine;

// Hexgrid coordinates are relative to each player's core building
public class HexGrid
{
    private Vector3 origin;
    private Dictionary<(int, int, int), HexCell> hexMap; // Q, R, H -> HexCell
    private int radius; // Radius of the hex grid
    private int height; // Height of the hex grid
    
    private float innerRadius = 1f;
    
    public HexGrid(Vector3 origin, int initialRadius, int initialHeight)
    {
        this.origin = origin;
        this.radius = initialRadius;
        this.height = initialHeight;
        InitializeGrid(initialRadius, initialHeight);
    }
    
    public (int, int, int) WorldCoordinatesToHexCoordinates(Vector3 worldCoordinates)
    {
        Vector3 localCoordinates = worldCoordinates - origin;
        float q = (2f / 3f * localCoordinates.x) / innerRadius;
        float r = (-1f / 3f * localCoordinates.x + Mathf.Sqrt(3) / 3f * localCoordinates.z) / innerRadius;
        return (Mathf.RoundToInt(q), Mathf.RoundToInt(r), Mathf.RoundToInt(worldCoordinates.y));
    }
    
    public Vector3 HexCoordinatesToWorldCoordinates(int q, int r, int h)
    {
        float x = innerRadius * 3f / 2f * q;
        float z = innerRadius * Mathf.Sqrt(3) * (r + q / 2f);
        return new Vector3(x, h, z) + origin;
    }
    
    // Get neighbor in a specific direction (0-7)
    public HexCell GetNeighbor(HexCell hex, int direction)
    {
        return hexMap[(hex.Q + Directions[direction].Q, hex.R + Directions[direction].R, hex.H + Directions[direction].H)];
    }
    
    // Find all 8 neighbors of a given hex
    public HexCell[] GetAllNeighbors(HexCell hex)
    {
        HexCell[] neighbors = new HexCell[8];
        for (int i = 0; i < 8; i++)
        {
            neighbors[i] = GetNeighbor(hex, i);
        }
        return neighbors;
    }
    
    public Dictionary<(int, int, int), HexCell> GetHexMap()
    {
        return hexMap;
    }
    
    private void InitializeGrid(int initialRadius, int initialHeight)
    {
        hexMap = new Dictionary<(int, int, int), HexCell>();
        for (int h = 0; h < initialHeight; h++)
        {
            for (int q = -initialRadius; q <= initialRadius; q++)
            {
                int r1 = Math.Max(-initialRadius, -q - initialRadius);
                int r2 = Math.Min(initialRadius, -q + initialRadius);
                for (int r = r1; r <= r2; r++)
                {
                    HexCell hex = new HexCell(q, r, h);
                    hex.Position = HexCoordinatesToWorldCoordinates(q, r, h);
                    hexMap.Add((q, r, h), hex);
                }
            }
        }
    }
    
    // Six possible movement directions in a cube coordinate system
    private static readonly HexCell[] Directions = {
        new HexCell(+1, -1, 0), new HexCell(+1, 0, 0), new HexCell(0, +1, 0),
        new HexCell(-1, +1, 0), new HexCell(-1, 0, 0), new HexCell(0, -1, 0),
        new HexCell(0, 0, +1), new HexCell(0, 0, -1)
    };
}
