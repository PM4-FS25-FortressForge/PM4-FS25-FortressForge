using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.Economy;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;

namespace Tests.Hexgrid
{
    public class FakeTerrainHeightProvider : ITerrainHeightProvider
    {
        public float SampleHeight(Vector3 position) => 0f;
        public float SampleHexHeight(Vector3 position, float tileHeight, float tileRadius) => 0f;
    }

    [TestFixture]
    public class HexGridDataTests
    {
        private ITerrainHeightProvider _fakeTerrain;
        private HexGridData _gridData;

        [SetUp]
        public void Setup()
        {
            BuildingManager buildingManager = new BuildingManager();

            var maxValues = new Dictionary<ResourceType, float>
            {
                { ResourceType.Power, 0f },
                { ResourceType.Metal, 10000f },
            };

            var gloabalEconomy = new GlobalEconomy(0f);
            EconomySystem economySystem = new EconomySystem(buildingManager, gloabalEconomy, maxValues);

            _fakeTerrain = new FakeTerrainHeightProvider();
            _gridData = new HexGridData(
                id: 1,
                origin: Vector3.zero,
                radius: 7,
                tileSize: 1f,
                tileHeight: 2f,
                terrainHeightProvider: _fakeTerrain,
                economySystem: economySystem,
                buildingManager: buildingManager
            );
        }

        [Test]
        [TestCase(0, 0, 1)]
        [TestCase(0, 0, 0)]
        [TestCase(99999, 0, 0)]
        [TestCase(5, 5, 0)]
        public void TestValidateBuildingPlacement_Returns_False(int x, int y, int z)
        {
            var alreadyOccupiedCoord = new HexTileCoordinate(0, 0, 0);
            if (_gridData.TileMap.ContainsKey(alreadyOccupiedCoord))
            {
                _gridData.TileMap[alreadyOccupiedCoord].IsOccupied = true;
            }

            var buildingTemplate = ScriptableObject.CreateInstance<WeaponBuildingTemplate>();
            buildingTemplate.ShapeDataEntries = new List<HexTileEntry>
            {
                new HexTileEntry(new HexTileCoordinate(0, 0, 0), true),
                new HexTileEntry(new HexTileCoordinate(1, 0, 0), true),
                new HexTileEntry(new HexTileCoordinate(0, 1, 0), true),
                new HexTileEntry(new HexTileCoordinate(1, 1, 0), true)
            };

            var placementCoord = new HexTileCoordinate(x, y, z);

            bool canPlace = _gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate.ShapeData);

            Assert.IsFalse(canPlace,
                $"ValidateBuildingPlacement sollte false zurückgeben, wenn Tile ({x},{y},{z}) bereits belegt oder ungültig ist.");
        }

        [Test]
        [TestCase(2, 2, 0)]
        [TestCase(3, -3, 0)]
        public void TestValidateBuildingPlacement_Returns_True(int x, int y, int z)
        {
            var alreadyOccupiedCoord = new HexTileCoordinate(0, 0, 0);
            if (_gridData.TileMap.ContainsKey(alreadyOccupiedCoord))
            {
                _gridData.TileMap[alreadyOccupiedCoord].IsOccupied = true;
            }

            var buildingTemplate = ScriptableObject.CreateInstance<WeaponBuildingTemplate>();
            buildingTemplate.ShapeDataEntries = new List<HexTileEntry>
            {
                new HexTileEntry(new HexTileCoordinate(0, 0, 0), true),
                new HexTileEntry(new HexTileCoordinate(1, 0, 0), true),
                new HexTileEntry(new HexTileCoordinate(0, 1, 0), true),
                new HexTileEntry(new HexTileCoordinate(1, 1, 0), true)
            };

            var placementCoord = new HexTileCoordinate(x, y, z);

            bool canPlace = _gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate.ShapeData);

            Assert.IsTrue(canPlace,
                $"ValidateBuildingPlacement sollte true zurückgeben, wenn Tile ({x},{y},{z}) frei ist.");
        }
    }
}
