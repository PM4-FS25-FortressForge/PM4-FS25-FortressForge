using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;

namespace Tests.Hexgrid
{
    public class FakeTerrainHeightProvider : ITerrainHeightProvider
    {
        public float SampleHeight(Vector3 position)
        {
            return 0f; // Simuliert flaches Terrain auf y=0
        }
    }

    [TestFixture]
    public class HexGridDataTests
    {
        private ITerrainHeightProvider _fakeTerrain;
        private HexGridData _gridData;

        [SetUp]
        public void Setup()
        {
            _fakeTerrain = new FakeTerrainHeightProvider();
            _gridData = new HexGridData(
                id: 1,
                origin: Vector3.zero,
                radius: 7,
                tileSize: 1f,
                tileHeight: 2f,
                terrainHeightProvider: _fakeTerrain
            );
        }

        [Test]
        [TestCase(0, 0, 1)]
        [TestCase(0, 0, 0)]
        [TestCase(99999, 0, 0)]
        [TestCase(5, 5, 0)]
        public void TestValidateBuildingPlacement_Returns_False(int x, int y, int z)
        {
            // Arrange
            HexTileCoordinate alreadyOccupiedCoord = new HexTileCoordinate(0, 0, 0);
            if (_gridData.TileMap.ContainsKey(alreadyOccupiedCoord))
            {
                _gridData.TileMap[alreadyOccupiedCoord].IsOccupied = true;
            }

            var buildingTemplate = ScriptableObject.CreateInstance<WeaponBuildingTemplate>();
            buildingTemplate.ShapeData = new List<HexTileCoordinate>
            {
                new HexTileCoordinate(0, 0, 0),
                new HexTileCoordinate(1, 0, 0),
                new HexTileCoordinate(0, 1, 0),
                new HexTileCoordinate(1, 1, 0)
            };

            var placementCoord = new HexTileCoordinate(x, y, z);

            // Act
            bool canPlace = _gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate);

            // Assert
            Assert.IsFalse(canPlace,
                $"ValidateBuildingPlacement sollte false zurückgeben, wenn Tile ({x},{y},{z}) bereits belegt oder ungültig ist.");
        }

        [Test]
        [TestCase(2, 2, 0)]
        [TestCase(3, -3, 0)]
        public void TestValidateBuildingPlacement_Returns_True(int x, int y, int z)
        {
            // Arrange
            HexTileCoordinate alreadyOccupiedCoord = new HexTileCoordinate(0, 0, 0);
            if (_gridData.TileMap.ContainsKey(alreadyOccupiedCoord))
            {
                _gridData.TileMap[alreadyOccupiedCoord].IsOccupied = true;
            }

            var buildingTemplate = ScriptableObject.CreateInstance<WeaponBuildingTemplate>();
            buildingTemplate.ShapeData = new List<HexTileCoordinate>
            {
                new HexTileCoordinate(0, 0, 0),
                new HexTileCoordinate(1, 0, 0),
                new HexTileCoordinate(0, 1, 0),
                new HexTileCoordinate(1, 1, 0)
            };

            var placementCoord = new HexTileCoordinate(x, y, z);

            // Act
            bool canPlace = _gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate);

            // Assert
            Assert.IsTrue(canPlace,
                $"ValidateBuildingPlacement sollte true zurückgeben, wenn Tile ({x},{y},{z}) frei ist.");
        }
    }
}