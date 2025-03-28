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
        public static float TileRadius;
        public static float TileHeight;
        
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
        
        public HexTileCoordinates(Vector3 worldPosition) // TODO make sure to use custom axial coordinates
        { 
            // Convert world position to hex grid axial coordinates
            float x = worldPosition.x / (TileRadius * 3f / 2f); // TODO throws exception regularly
            float z = worldPosition.z / (TileRadius * Mathf.Sqrt(3));

            Q = Mathf.RoundToInt(x);
            R = Mathf.RoundToInt(z - (Q / 2f)); // Adjust for hex grid layout
            H = 0; // Assuming h (height) is 0 for ground-level placement
        }
        
        /// <summary>
        /// Calculates the world position of a tile based on its axial coordinates.
        /// </summary>
        public Vector3 GetWorldPosition(Vector3 origin)
        {
            float x = TileRadius * 3f / 2f * Q;
            float z = TileRadius * Mathf.Sqrt(3) * (R + Q / 2f);
            return new Vector3(x, H * TileHeight, z) + origin;
        }
        
        public Vector3 GetWorldPosition()
        {
            return GetWorldPosition(Vector3.zero);
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
        
        public static HexTileCoordinates operator +(HexTileCoordinates a, Vector3 b)
        {
            var c = new HexTileCoordinates(b);
            return a + c;
        }

        public static HexTileCoordinates operator -(HexTileCoordinates a, Vector3 b)
        {
            var c = new HexTileCoordinates(b);
            return a - c;
        }
    }
}