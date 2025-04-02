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
        private static float _tileRadius;
        private static float _tileHeight;
        public static Vector3 Origin { get; set; }
        
        // Axial + height coordinate
        public int Q { get; private set; }
        public int R { get; private set; }
        public int S => -Q - R;
        public int H { get; private set; }
        
        public (int q, int r, int h) AxialCoord => (Q, R, H);
        public (int q, int r, int s, int h) HexCoord => (Q, S, R, H); // Cube coordinates

        public static void Init(float tileRadius, float tileHeight, Vector3 origin)
        {
            Origin = origin;
            _tileRadius = tileRadius;
            _tileHeight = tileHeight;
        }

        public static HexTileCoordinate GetHexTileCoordinateFromWorldPosition(Vector3 position)
        {
            // Adjust position by removing the origin offset
            Vector3 pos = position - Origin;

            // Convert back from world space to axial coordinates (Q, R) in float
            float q = (2f / 3f * pos.x) / _tileRadius;
            float r = (-1f / 3f * pos.x + Mathf.Sqrt(3) / 3f * pos.z) / _tileRadius;
            float h = pos.y / _tileHeight;

            // Round to nearest hex tile using cube coordinates
            float s = -q - r;
            int rq = Mathf.RoundToInt(q);
            int rr = Mathf.RoundToInt(r);
            int rs = Mathf.RoundToInt(s);

            float dq = Mathf.Abs(rq - q);
            float dr = Mathf.Abs(rr - r);
            float ds = Mathf.Abs(rs - s);

            if (dq > dr && dq > ds)
            {
                rq = -rr - rs;
            }
            else if (dr > ds)
            {
                rr = -rq - rs;
            }

            return new HexTileCoordinate(rq, rr, Mathf.RoundToInt(h));
        }

        public HexTileCoordinate(int q, int r, int height)
        {
            Q = q;
            R = r;
            H = height;
        }

        public HexTileCoordinate(float tileRadius, float tileHeight) // TODO make sure to use custom axial coordinates
        { 
            // Convert world position to hex grid axial coordinates
            float x = Origin.x / (tileRadius * 3f / 2f); // TODO throws exception regularly
            float z = Origin.z / (tileRadius * Mathf.Sqrt(3));

            Q = Mathf.RoundToInt(x);
            R = Mathf.RoundToInt(z - (Q / 2f)); // Adjust for hex grid layout
            H = (int) (Origin.y / tileHeight); // Assuming h (height) is 0 for ground-level placement TODO add object height transformation
        }

        public HexTileCoordinate LocalToGlobal(HexTileCoordinate origin, HexTileCoordinate pos)
        {
            return origin - pos;
        }
        
        public HexTileCoordinate GlobalToLocal(HexTileCoordinate origin, HexTileCoordinate pos)
        {
            return origin + pos;
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
        
        public Vector3 GetWorldPosition()
        {
            return GetWorldPosition(Vector3.zero, _tileRadius, _tileHeight);
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