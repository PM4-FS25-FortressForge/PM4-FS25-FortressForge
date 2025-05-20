using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using FortressForge.HexGrid.Data;
using FortressForge.Economy;
using FortressForge.HexGrid;
using FortressForge.BuildingSystem.BuildManager;


namespace Tests.WeaponTests
{
    [TestFixture]
    public class WeaponInputHandlerReloadTests
    {
        private GameObject _weaponObject;
        private WeaponInputHandler _handler;
        private HexGridData _gridData;
        private EconomySystem _economy;

        [SetUp]
        public void Setup()
        {
            _weaponObject = new GameObject("TestWeapon");
            _handler = _weaponObject.AddComponent<WeaponInputHandler>();

            BuildingManager buildingManager = new BuildingManager();

            var maxValues = new Dictionary<ResourceType, float>
            {
                { ResourceType.Metal, 1000f },
                { ResourceType.Power, 500f }
            };

            var globalEconomy = new GlobalEconomy(0f);
            _economy = new EconomySystem(buildingManager, globalEconomy, maxValues);

            HexGridManager gridManager = new HexGridManager();
            var terrain = new FakeTerrainHeightProvider();

            _gridData = new HexGridData(
                id: 1,
                tileSize: 1f,
                tileHeight: 2f,
                terrainHeightProvider: terrain,
                economySystem: _economy,
                buildingManager: buildingManager,
                hexGridManager: gridManager
            );

            gridManager.AddGrid(_gridData);
            _gridData.CreateStarterGrid(Vector3.zero, 5);

            _handler.Init(_gridData);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_weaponObject);
        }

        [Test]
        public void ReloadWeapon_WithSufficientResources_ShouldPayResources()
        {
            // Arrange
            var ResourceStock = new Dictionary<ResourceType, float>
            {
                { ResourceType.Metal, 100f },
                { ResourceType.Power, 100f }
            };

            float metalBefore = ResourceStock[ResourceType.Metal];

            // Act
            _handler.SendMessage("ReloadWeaponServerRpc");

            float metalAfter = ResourceStock[ResourceType.Metal];

            // Assert
            Assert.Less(metalAfter, metalBefore, "Expected Metal to be reduced after successful reload.");
        }

        [Test]
        public void ReloadWeapon_WithInsufficientResources_ShouldNotPayResources()
        {
            // Arrange
            var ResourceStock = new Dictionary<ResourceType, float>
            {
                { ResourceType.Metal, 0f },
                { ResourceType.Power, 0f }
            };

            float metalBefore = ResourceStock[ResourceType.Metal];

            // Act
            _handler.SendMessage("ReloadWeaponServerRpc");

            float metalAfter = ResourceStock[ResourceType.Metal];

            // Assert
            Assert.AreEqual(metalBefore, metalAfter, "Expected Metal to remain unchanged due to insufficient resources.");
        }
    }

    public class FakeTerrainHeightProvider : ITerrainHeightProvider
    {
        public float SampleHeight(Vector3 position) => 0f;
        public float SampleHexHeight(Vector3 position, float tileHeight, float tileRadius) => 0f;
        public HexTileCoordinate GetHexTileCoordinate(Vector3 position, float tileHeight, float tileRadius) =>
            new HexTileCoordinate(0, 0, 0);
    }
}
