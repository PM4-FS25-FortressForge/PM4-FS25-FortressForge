using System;
using UnityEngine;

namespace FortressForge.BuildingSystem.HexGrid
{
    /// <summary>
    /// Struct to store hex tile coordinates.
    /// Value-type, comparable, usable in dictionaries or sets.
    /// </summary>
    public struct HexTileCoordinates : IEquatable<HexTileCoordinates>
    {
        // Axial + height coordinate
        public int Q { get; private set; }
        public int R { get; private set; }
        public int H { get; private set; }
        // Derived cube coordinate: S = -Q - R
        public int S => -Q - R;
        
        public (int q, int r, int h) HexCoord => (Q, R, H);
        public (int q, int r, int s, int h) CubeCoord => (Q, S, R, H); // Cube coordinates
        
        public HexTileCoordinates(int q, int r, int height)
        {
            Q = q;
            R = r;
            H = height;
        }
        
        public bool Equals(HexTileCoordinates other)
        {
            return Q == other.Q && R == other.R && H == other.H;
        }

        public override bool Equals(object obj)
        {
            return obj is HexTileCoordinates other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Q, R, H);
        }

        public static bool operator ==(HexTileCoordinates a, HexTileCoordinates b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(HexTileCoordinates a, HexTileCoordinates b)
        {
            return !a.Equals(b);
        }
        
        public static HexTileCoordinates operator +(HexTileCoordinates a, HexTileCoordinates b)
        {
            return new HexTileCoordinates(a.Q + b.Q, a.R + b.R, a.H + b.H);
        }

        public static HexTileCoordinates operator -(HexTileCoordinates a, HexTileCoordinates b)
        {
            return new HexTileCoordinates(a.Q - b.Q, a.R - b.R, a.H - b.H);
        }
    }
}