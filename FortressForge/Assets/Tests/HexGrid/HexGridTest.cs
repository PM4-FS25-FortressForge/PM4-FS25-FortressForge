using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.HexGrid;
using FortressForge.HexGrid.BuildingData;
using FortressForge.HexGrid.Data;

namespace Tests.Hexgrid
{
    public class FakeTerrainHeightProvider : ITerrainHeightProvider
    {
        public float SampleHeight(Vector3 position)
        {
            return 0f; // Wir tun so, als wäre das Terrain immer bei y=0
        }
    }

    [TestFixture]
    public class HexGridDataTests
    {
        private IHexGridDataFactory _hexGridDataFactory;
        
        // Wird vor jedem Test ausgeführt.
        // Hier erzeugen wir die Factory mit unserem Fake-Terrain.
        [SetUp]
        public void Setup()
        {
            var fakeTerrain = new FakeTerrainHeightProvider();
            _hexGridDataFactory = new HexGridDataFactory(fakeTerrain);
        }
        
        [Test]
        [TestCase(0, 0, 1)]
        [TestCase(0, 0, 0)]
        [TestCase(99999, 0, 0)]
        [TestCase(5, 5, 0)]
        public void TestValidateBuildingPlacement_Returns_False(int x, int y, int z)
        {
            // Arrange: Wir erstellen ein HexGridData über unsere Factory,
            //         damit das Terrain "gemockt" ist.
            HexGridData gridData = _hexGridDataFactory.CreateData(
                id: 1,
                origin: Vector3.zero,
                radius: 5,
                tileSize: 1f,
                tileHeight: 2f
            );
            
            // Wir belegen schon mal die Koordinate (0,0,0).
            HexTileCoordinate alreadyOccupiedCoord = new HexTileCoordinate(0, 0, 0);
            gridData.TileMap[alreadyOccupiedCoord].IsOccupied = true;
            
            // Hier ein Beispiel-BuildingTemplate
            BaseBuildingTemplate buildingTemplate = ScriptableObject.CreateInstance<WeaponBuildingTemplate>();
            buildingTemplate.ShapeData = new List<HexTileCoordinate> {
                new HexTileCoordinate(0, 0, 0),
                new HexTileCoordinate(1, 0, 0),
                new HexTileCoordinate(0, 1, 0),
                new HexTileCoordinate(1, 1, 0)
            };
            
            HexTileCoordinate placementCoord = new HexTileCoordinate(x, y, z);

            // Act
            bool canPlace = gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate);

            // Assert
            Assert.IsFalse(canPlace,
                $"ValidateBuildingPlacement sollte false zurückgeben, wenn Tile ({x},{y},{z}) bereits belegt ist oder ungültig ist.");
        }

        [Test]
        [TestCase(2, 2, 0)]
        [TestCase(3, -3, 0)]
        public void TestValidateBuildingPlacement_Returns_True(int x, int y, int z)
        {
            // Arrange: Neues GridData anlegen.
            HexGridData gridData = _hexGridDataFactory.CreateData(
                id: 1,
                origin: Vector3.zero,
                radius: 7,
                tileSize: 1f,
                tileHeight: 2f
            );
            
            HexTileCoordinate alreadyOccupiedCoord = new HexTileCoordinate(0, 0, 0);
            gridData.TileMap[alreadyOccupiedCoord].IsOccupied = true;
            
            BaseBuildingTemplate buildingTemplate = ScriptableObject.CreateInstance<WeaponBuildingTemplate>();
            buildingTemplate.ShapeData = new List<HexTileCoordinate>{
                new HexTileCoordinate(0, 0, 0),
                new HexTileCoordinate(1, 0, 0),
                new HexTileCoordinate(0, 1, 0),
                new HexTileCoordinate(1, 1, 0),
            };

            HexTileCoordinate placementCoord = new HexTileCoordinate(x, y, z);
            
            bool canPlace = gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate);
            
            Assert.IsTrue(canPlace,
                $"ValidateBuildingPlacement sollte true zurückgeben, wenn Tile ({x},{y},{z}) noch frei ist.");
        }
    }
}