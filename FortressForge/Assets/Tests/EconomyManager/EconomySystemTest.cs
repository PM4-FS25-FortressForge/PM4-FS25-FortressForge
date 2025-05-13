using System.Collections.Generic;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.Economy;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using UnityEngine;

namespace Tests.EconomyManager
{
    /// <summary>
    /// Business logic tests for the EconomySystem.
    /// Tests if resources are calculated correctly, and building are disabled if they would produce negative resource totals.
    /// </summary>
    public class EconomySystemTest
    {
        private EconomySystem _economySystem;
        private BuildingManager _buildingManager;

        [SetUp]
        public void SetUp()
        {
            var maxValues = new Dictionary<ResourceType, float>
            {
                { ResourceType.Power, 0f },
                { ResourceType.Metal, 10000f },
            };
            
            _buildingManager = new BuildingManager();
            var gloabalEconomy = new GlobalEconomy(0f);
            _economySystem = new EconomySystem(_buildingManager, gloabalEconomy, maxValues);
        }

        [Test]
        public void EconomySystem_DisablesActor_WhenResourceGoesNegative()
        {
            // Arrange
            var mockActor = ScriptableObject.CreateInstance<MockEconomyActor>();
            mockActor.ResourceChange = new Dictionary<ResourceType, float>
            {
                { ResourceType.Metal, -100f } // Force negative resource
            };
            
            var fakePrefab = new GameObject("MockBuilding");
            var fakeTiles = new List<HexTileData> { new(new HexTileCoordinate()) };
            _buildingManager.AddBuilding(new BuildingData(fakePrefab, fakeTiles, mockActor));

            // Act
            _economySystem.UpdateEconomy();

            // Assert
            Assert.IsTrue(mockActor.IsDisabled, "The actor should be disabled due to negative Iron.");
        }

        [Test]
        public void EconomySystem_DisablesActor_InOrder()
        {
            // Arrange
            var resourcesPerBuilding = new List<Dictionary<ResourceType, float>>()
            {
                new()
                {
                    { ResourceType.Power, 200f } // Force negative resource
                }, 
                new()
                {
                    { ResourceType.Power, -100f } // Force negative resource
                }, 
                new()
                {
                    { ResourceType.Power, -200f } // Force negative resource
                }, 
                new()
                {
                    { ResourceType.Power, -100f } // Force negative resource
                }
            };

            var mockActors = new List<MockEconomyActor>();
            foreach (var resources in resourcesPerBuilding)
            {
                var mock = ScriptableObject.CreateInstance<MockEconomyActor>();
                mock.ResourceChange = resources;
                mockActors.Add(mock);
            }

            var fakePrefab = new GameObject("MockBuilding");
            var fakeTiles = new List<HexTileData> { new(new HexTileCoordinate()) };
            mockActors.ForEach(mock => _buildingManager.AddBuilding(new BuildingData(fakePrefab,fakeTiles, mock)));

            // Act
            _economySystem.UpdateEconomy();

            // Assert
            Assert.IsFalse(mockActors[1].IsDisabled, "The actor should not be disabled.");
            Assert.IsTrue(mockActors[2].IsDisabled, "The actor should be disabled due to negative Iron.");
            Assert.IsFalse(mockActors[3].IsDisabled, "The actor should not be disabled.");
        }

        [Test]
        public void EconomySystem_CalculatesEconomy()
        {
            // Arrange
            var resourcesPerBuilding = new List<Dictionary<ResourceType, float>>()
            {
                new()
                {
                    { ResourceType.Power, 200f } 
                },
                new()
                {
                    { ResourceType.Power, -100f },
                    { ResourceType.Metal, 200f } 
                },
                new()
                {
                    { ResourceType.Power, -200f }, 
                    { ResourceType.Metal, 300f } 
                },
                new()
                {
                    { ResourceType.Power, -100f },
                    { ResourceType.Metal, 150f } 
                }
            };

            var mockActors = new List<MockEconomyActor>();
            foreach (var resources in resourcesPerBuilding)
            {
                var mock = ScriptableObject.CreateInstance<MockEconomyActor>();
                mock.ResourceChange = resources;
                mockActors.Add(mock);
            }

            var fakePrefab = new GameObject("MockBuilding");
            var fakeTiles = new List<HexTileData> { new(new HexTileCoordinate()) };
            mockActors.ForEach(mock => _buildingManager.AddBuilding(new BuildingData(fakePrefab, fakeTiles, mock)));

            // Act
            _economySystem.UpdateEconomy();

            // Assert
            Assert.AreEqual(_economySystem.CurrentResources[ResourceType.Power].CurrentAmount, 0f, "Power should be 0.");
            Assert.AreEqual(_economySystem.CurrentResources[ResourceType.Metal].CurrentAmount, 350f, "Iron should be 350.");
        }
    }
}

public class MockEconomyActor : BaseBuildingTemplate
{
    public Dictionary<ResourceType, float> ResourceChange = new();
    public Dictionary<ResourceType, float> ResourceCost = new();
    public bool IsDisabled => !_enabled;

    public override Dictionary<ResourceType, float> GetNetResourceChange() => ResourceChange;
    public override Dictionary<ResourceType, float> GetBuildCost() => ResourceCost;
}
