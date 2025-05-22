using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.HexGrid;

namespace Tests.HexGrid
{
    public class HexTileHelperTests
    {
        [Test]
        public void ExtractShapeInformation_EmptyList_ReturnsEmptyLists()
        {
            var input = new List<HexTileEntry>();
            var (coords, flags) = HexTileHelper.ExtractShapeInformation(input);

            Assert.IsNotNull(coords);
            Assert.IsNotNull(flags);
            Assert.IsEmpty(coords);
            Assert.IsEmpty(flags);
        }

        [Test]
        public void ExtractShapeInformation_PopulatedList_ReturnsMatchingTuples()
        {
            var entry1 = new HexTileEntry (new HexTileCoordinate(1, 2, 0), true);
            var entry2 = new HexTileEntry (new HexTileCoordinate(-1, 0, 0), false);
            var list    = new List<HexTileEntry> { entry1, entry2 };

            var (coords, flags) = HexTileHelper.ExtractShapeInformation(list);

            Assert.AreEqual(2, coords.Count);
            Assert.AreEqual(2, flags.Count);

            Assert.AreEqual(entry1.Coordinate, coords[0]);
            Assert.AreEqual(entry2.Coordinate, coords[1]);
            Assert.IsTrue(flags[0]);
            Assert.IsFalse(flags[1]);
        }

        [Test]
        public void GetRotatedShape_EmptyList_ReturnsEmptyList()
        {
            var result = HexTileHelper.GetRotatedShape(new List<HexTileCoordinate>(), 123f);
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetRotatedShape_ZeroAngle_ReturnsOriginalCoordinates()
        {
            var original = new List<HexTileCoordinate>
            {
                new HexTileCoordinate(2, -1, 0),
                new HexTileCoordinate(0,  3,  0)
            };

            var rotated = HexTileHelper.GetRotatedShape(original, 0f);

            Assert.AreEqual(original.Count, rotated.Count);
            for (int i = 0; i < original.Count; i++)
                Assert.AreEqual(original[i], rotated[i]);
        }

        [Test]
        public void GetRotatedShape_180Degrees_RotatesAsExpected()
        {
            // For (1,0) rotating by 180 should yield (-1,0) under the implemented algorithm
            var original = new List<HexTileCoordinate> { new HexTileCoordinate(1, 0, 0) };
            var rotated  = HexTileHelper.GetRotatedShape(original, 180f);

            Assert.AreEqual(1, rotated.Count);
            Assert.AreEqual(new HexTileCoordinate(-1, 0, 0), rotated[0]);
        }

        [TestCase(360f)]
        [TestCase(720f)]
        public void GetRotatedShape_FullRotations_ReturnsOriginal(float angle)
        {
            var original = new List<HexTileCoordinate>
            {
                new HexTileCoordinate(1,  2, 0),
                new HexTileCoordinate(-3, 1, 0)
            };

            var rotated = HexTileHelper.GetRotatedShape(original, angle);
            Assert.AreEqual(original.Count, rotated.Count);

            for (int i = 0; i < original.Count; i++)
                Assert.AreEqual(original[i], rotated[i]);
        }

        [Test]
        public void GetAveragePosition_SingleCoordinate_EqualsWorldPosition()
        {
            float radius = 2.5f, height = 1.0f;
            var coord    = new HexTileCoordinate(3, -2, 0);
            var expected = coord.GetWorldPosition(radius, height);

            var avg = HexTileHelper.GetAveragePosition(
                new List<HexTileCoordinate> { coord }, radius, height
            );

            Assert.AreEqual(expected, avg);
        }

        [Test]
        public void GetAveragePosition_MultipleCoordinates_CalculatesMean()
        {
            float radius = 1.5f, height = 0.5f;
            var c1 = new HexTileCoordinate(0, 0, 0);
            var c2 = new HexTileCoordinate(1, 0, 0);
            var p1 = c1.GetWorldPosition(radius, height);
            var p2 = c2.GetWorldPosition(radius, height);
            var expected = (p1 + p2) / 2f;

            var avg = HexTileHelper.GetAveragePosition(
                new List<HexTileCoordinate> { c1, c2 }, radius, height
            );

            Assert.AreEqual(expected, avg);
        }
    }
}
