using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.HexGrid.BuildingData;
using FortressForge.HexGrid;
using FortressForge.HexGrid.HexTile;

namespace Tests.Hexgrid
{
    public class HexGridDataTests
    {
        [Test]
        [TestCase(0, 0, 1)]
        [TestCase(0, 0, 0)]
        [TestCase(99999, 0, 0)]
        [TestCase(5, 5, 0)]
        public void TestValidateBuildingPlacement_Returns_False(int x, int y, int z)
        {
            HexGridData gridData = new HexGridData(
                id: 1,
                origin: Vector3.zero,
                radius: 5,
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

            // Act
            bool canPlace = gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate); 

            // Assert
            Assert.IsFalse(canPlace,
                $"ValidateBuildingPlacement sollte false zurückgeben, wenn Tile ({x},{y},{z}) bereits belegt ist.");
        }
        
        [Test]
        [TestCase(2, 2, 0)]
        [TestCase(3, -3, 0)]
        public void TestValidateBuildingPlacement_Returns_True(int x, int y, int z)
        {
            HexGridData gridData = new HexGridData(
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

            // Act
            bool canPlace = gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate);

            // Assert
            Assert.IsTrue(canPlace,
                $"ValidateBuildingPlacement sollte true zurückgeben, wenn Tile ({x},{y},{z}) bereits belegt ist.");
        }
    }
}