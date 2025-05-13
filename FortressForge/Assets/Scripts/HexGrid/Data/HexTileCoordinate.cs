using System;
using UnityEngine;

namespace FortressForge.HexGrid
{
    /// <summary>
    /// Struct to store hex tile coordinates.
    /// Value-type, comparable, usable in dictionaries or sets.
    /// </summary>
    [System.Serializable]
    public struct HexTileCoordinate : IEquatable<HexTileCoordinate>
    {
        // Axial + height coordinate
        public int Q;
        public int R;
        public int S => -Q - R;
        public int H;
        
        public (int q, int r, int h) AxialCoord => (Q, R, H);
        public (int q, int r, int s, int h) HexCoord => (Q, S, R, H); // Cube coordinates

        public HexTileCoordinate(int q, int r, int height)
        {
            Q = q;
            R = r;
            H = height;
        }

        public HexTileCoordinate(float tileRadius, float tileHeight, Vector3 origin=default) 
        { 
            // Convert world position to hex grid axial coordinates
            float x = origin.x / (tileRadius * 3f / 2f); 
            float z = origin.z / (tileRadius * Mathf.Sqrt(3));

            Q = Mathf.RoundToInt(x);
            R = Mathf.RoundToInt(z - (Q / 2f)); // Adjust for hex grid layout
            H = Mathf.CeilToInt(origin.y / tileHeight); // Assuming h (height) is 0 for ground-level placement
        }
        
        /// <summary>
        /// Calculates the world position of a tile based on its axial coordinates.
        /// </summary>
        public Vector3 GetWorldPosition(Vector3 origin, float tileRadius, float tileHeight)
        {
            float x = tileRadius * 3f / 2f * Q;
            float z = tileRadius * Mathf.Sqrt(3) * (R + Q / 2f);
            return new Vector3(x, H * tileHeight, z) + origin;
        }
        
        public Vector3 GetWorldPosition(float tileRadius, float tileHeight)
        {
            return GetWorldPosition(Vector3.zero, tileRadius, tileHeight);
        }
        
        /// <summary>
        /// Determines whether the current <see cref="HexTileCoordinate"/> is equal to another <see cref="HexTileCoordinate"/>.
        /// </summary>
        public bool Equals(HexTileCoordinate other)
        {
            return Q == other.Q && R == other.R && H == other.H;
        }

        /// <summary>
        /// Determines whether this <see cref="HexTileCoordinate"/> is equal to another object.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is HexTileCoordinate other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this <see cref="HexTileCoordinate"/> based on its Q, R, and H values.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Q, R, H);
        }

        /// <summary>
        /// Determines whether two <see cref="HexTileCoordinate"/> instances are equal.
        /// </summary>
        public static bool operator ==(HexTileCoordinate a, HexTileCoordinate b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether two <see cref="HexTileCoordinate"/> instances are not equal.
        /// </summary>
        public static bool operator !=(HexTileCoordinate a, HexTileCoordinate b)
        {
            return !a.Equals(b);
        }
        
        /// <summary>
        /// Adds two <see cref="HexTileCoordinate"/> instances, combining their Q, R, and H values.
        /// </summary>
        public static HexTileCoordinate operator +(HexTileCoordinate a, HexTileCoordinate b)
        {
            return new HexTileCoordinate(a.Q + b.Q, a.R + b.R, a.H + b.H);
        }

        /// <summary>
        /// Subtracts one <see cref="HexTileCoordinate"/> from another, subtracting their Q, R, and H values.
        /// </summary>
        public static HexTileCoordinate operator -(HexTileCoordinate a, HexTileCoordinate b)
        {
            return new HexTileCoordinate(a.Q - b.Q, a.R - b.R, a.H - b.H);
        }
    }
}