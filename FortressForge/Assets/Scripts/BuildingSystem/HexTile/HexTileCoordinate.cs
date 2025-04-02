using System;
using UnityEngine;

namespace FortressForge.BuildingSystem.HexTile
{
    /// <summary>
    /// Struct to store hex tile coordinates.
    /// Value-type, comparable, usable in dictionaries or sets.
    /// </summary>
    public struct HexTileCoordinate : IEquatable<HexTileCoordinate>
    {
        // Axial + height coordinate
        public int Q { get; private set; }
        public int R { get; private set; }
        public int S => -Q - R;
        public int H { get; private set; }
        
        public (int q, int r, int h) AxialCoord => (Q, R, H);
        public (int q, int r, int s, int h) HexCoord => (Q, S, R, H); // Cube coordinates
        
        public HexTileCoordinate(int q, int r, int height)
        {
            Q = q;
            R = r;
            H = height;
        }
        
        public HexTileCoordinate(Vector3 worldPosition, float tileRadius, float tileHeight) // TODO make sure to use custom axial coordinates
        { 
            // Convert world position to hex grid axial coordinates
            float x = worldPosition.x / (tileRadius * 3f / 2f); // TODO throws exception regularly
            float z = worldPosition.z / (tileRadius * Mathf.Sqrt(3));

            Q = Mathf.RoundToInt(x);
            R = Mathf.RoundToInt(z - (Q / 2f)); // Adjust for hex grid layout
            H = (int) (worldPosition.y / tileHeight); // Assuming h (height) is 0 for ground-level placement TODO add object height transformation
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
        
        public bool Equals(HexTileCoordinate other)
        {
            return Q == other.Q && R == other.R && H == other.H;
        }

        public override bool Equals(object obj)
        {
            return obj is HexTileCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Q, R, H);
        }

        public static bool operator ==(HexTileCoordinate a, HexTileCoordinate b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(HexTileCoordinate a, HexTileCoordinate b)
        {
            return !a.Equals(b);
        }
        
        public static HexTileCoordinate operator +(HexTileCoordinate a, HexTileCoordinate b)
        {
            return new HexTileCoordinate(a.Q + b.Q, a.R + b.R, a.H + b.H);
        }

        public static HexTileCoordinate operator -(HexTileCoordinate a, HexTileCoordinate b)
        {
            return new HexTileCoordinate(a.Q - b.Q, a.R - b.R, a.H - b.H);
        }
    }
}