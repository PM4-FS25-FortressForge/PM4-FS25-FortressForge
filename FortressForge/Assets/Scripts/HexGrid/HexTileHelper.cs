using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.HexGrid
{
    public class HexTileHelper
    {
        public static (List<HexTileCoordinate> coordinates, List<bool> isStackable) ExtractShapeInformation(
            List<HexTileEntry> shapeDataEntries)
        {
            List<HexTileCoordinate> coordinates = new List<HexTileCoordinate>();
            List<bool> isStackable = new List<bool>();

            foreach (var entry in shapeDataEntries)
            {
                coordinates.Add(entry.Coordinate);
                isStackable.Add(entry.IsStackable);
            }

            return (coordinates, isStackable);
        }

        public static List<HexTileCoordinate> GetRotatedShape(List<HexTileCoordinate> originalShape, float angle)
        {
            int steps = (((int)(-angle / 60)) % 6 + 6) % 6;
            var rotated = new List<HexTileCoordinate>(originalShape.Count);

            foreach (var hex in originalShape)
            {
                int q = hex.Q;
                int r = hex.R;

                for (int i = 0; i < steps; i++)
                {
                    int temp = q;
                    q = -r;
                    r = temp + r;
                }

                rotated.Add(new HexTileCoordinate(q, r, hex.H));
            }

            return rotated;
        }

        public static Vector3 GetAveragePosition(List<HexTileCoordinate> hexTileCoordinates, float tileRadius, float tileHeight)
        {
            Vector3 avg = Vector3.zero;
            foreach (var coord in hexTileCoordinates)
            {
                avg += coord.GetWorldPosition(tileRadius, tileHeight);
            }

            return avg / hexTileCoordinates.Count;
        }
    }
}