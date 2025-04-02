using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.HexTile;

namespace Tests.Hexgrid
{
    public class HexGridDataTests
    {
        [Test]
        public void ValidateBuildingPlacement_ReturnsTrue_IfAllTilesAreFree()
        {
            
            // Arrange
            HexGridData gridData = new HexGridData(
                id: 1,
                origin: Vector3.zero,
                radius: 1,
                height: 1,
                tileSize: 1f,
                tileHeight: 2f
            );

            // Beispiel-BuildingTemplate, das nur EIN Tile belegt
            BaseBuildingTemplate buildingTemplate = ScriptableObject.CreateInstance<BaseBuildingTemplate>();
            buildingTemplate.ShapeData = new List<HexTileCoordinate>{
                new HexTileCoordinate(0, 0, 0) // belegt nur den "Mittelpunkt"
            };

            HexTileCoordinate placementCoord = new HexTileCoordinate(0, 0, 0);

            // Act
            bool canPlace = gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate);

            // Assert
            Assert.IsTrue(canPlace, "Der Gebäude-Bau sollte erlaubt sein, wenn alle Tiles frei sind.");

            // Zusätzlich können wir überprüfen, ob das Tile jetzt als belegt (IsOccupied=true) markiert wurde
            var placedTile = gridData.TileMap[placementCoord];
            Assert.IsTrue(placedTile.IsOccupied,
                "Das Tile sollte nach erfolgreichem Platzieren als belegt markiert sein.");
        }

        [Test]
        public void TestValidateBuildingPlacement_Returns_False()
        {
            HexGridData gridData = new HexGridData(
                id: 1,
                origin: Vector3.zero,
                radius: 5,
                height: 2,
                tileSize: 1f,
                tileHeight: 2f
            );

            HexTileCoordinate alreadyOccupiedCoord = new HexTileCoordinate(0, 1, 0);
            gridData.TileMap[alreadyOccupiedCoord].IsOccupied = true;

            BaseBuildingTemplate buildingTemplate = ScriptableObject.CreateInstance<WeaponBuildingTemplate>();
            buildingTemplate.ShapeData = new List<HexTileCoordinate>{
                new HexTileCoordinate(0, 0, 0),
                new HexTileCoordinate(1, -1, 0),
                new HexTileCoordinate(0, 0, 1)
            };

            HexTileCoordinate placementCoord = new HexTileCoordinate(0, 0, 0);
            
            bool canPlace = gridData.ValidateBuildingPlacement(placementCoord, buildingTemplate);
            
            Assert.IsFalse(canPlace,
                "ValidateBuildingPlacement sollte false zurückgeben, wenn eines der Tiles bereits belegt ist.");
        }
    }
}
