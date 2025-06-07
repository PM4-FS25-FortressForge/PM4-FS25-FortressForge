using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.HexGrid
{
    /// <summary>
    /// Provides helper methods for hex tile shape extraction, rotation, and position calculations.
    /// </summary>
    public class HexTileHelper
    {
        /// <summary>
        /// Extracts coordinates and stackable flags from a list of hex tile entries.
        /// </summary>
        /// <param name="shapeDataEntries">List of hex tile entries.</param>
        /// <returns>
        /// A tuple containing a list of coordinates and a list of stackable flags.
        /// </returns>
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

        /// <summary>
        /// Rotates a list of hex tile coordinates by a given angle (in degrees, multiple of 60).
        /// </summary>
        /// <param name="originalShape">The original list of hex tile coordinates.</param>
        /// <param name="angle">The rotation angle in degrees (should be a multiple of 60).</param>
        /// <returns>A new list of rotated hex tile coordinates.</returns>
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

        /// <summary>
        /// Calculates the average world position of a list of hex tile coordinates.
        /// </summary>
        /// <param name="hexTileCoordinates">List of hex tile coordinates.</param>
        /// <param name="tileRadius">The radius of a single hex tile.</param>
        /// <param name="tileHeight">The height of a single hex tile.</param>
        /// <returns>The average world position as a Vector3.</returns>
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