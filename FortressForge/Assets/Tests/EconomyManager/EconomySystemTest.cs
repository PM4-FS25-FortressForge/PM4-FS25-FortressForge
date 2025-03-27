using System.Collections.Generic;
using FortressForge.EconomyManager;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;
using System.Collections.Generic;

namespace Tests.EconomyManager
{
    /// <summary>
    /// Business logic tests for the EconomySystem.
    /// Tests if resources are calculated correctly, and building are disabled if they would produce negative resource totals.
    /// </summary>
    public class EconomySystemTest
    {
        private EconomySystem _economySystem;

        [SetUp]
        public void SetUp()
        {
            _economySystem = new EconomySystem();
        }

        [Test]
        public void EconomySystem_DisablesActor_WhenResourceGoesNegative()
        {
            // Arrange
            var mockActor = new MockEconomyActor();
            mockActor.ResourceChange = new Dictionary<ResourceType, float>
            {
                { ResourceType.Iron, -100f } // Force negative resource
            };

            _economySystem.RegisterActor(mockActor);

            // Act
            _economySystem.UpdateEconomy();

            // Assert
            Assert.IsTrue(mockActor.IsDisabled, "The actor should be disabled due to negative Iron.");
        }

        [Test]
        public void EconomySystem_DisablesActor_InOrder()
        {
            // Arrange
            var mocks = new List<MockEconomyActor>()
            {
                new()
                {
                    ResourceChange = new Dictionary<ResourceType, float>
                    {
                        { ResourceType.Power, 200f } // Force negative resource
                    }
                }, 
                new()
                {
                    ResourceChange = new Dictionary<ResourceType, float>
                    {
                        { ResourceType.Power, -100f } // Force negative resource
                    }
                }, 
                new()
                {
                    ResourceChange = new Dictionary<ResourceType, float>
                    {
                        { ResourceType.Power, -200f } // Force negative resource
                    }
                }, 
                new()
                {
                    ResourceChange = new Dictionary<ResourceType, float>
                    {
                        { ResourceType.Power, -100f } // Force negative resource
                    }
                }
            };

            mocks.ForEach(mock => _economySystem.RegisterActor(mock));

            // Act
            _economySystem.UpdateEconomy();

            // Assert
            Assert.IsFalse(mocks[1].IsDisabled, "The actor should not be disabled.");
            Assert.IsTrue(mocks[2].IsDisabled, "The actor should be disabled due to negative Iron.");
            Assert.IsFalse(mocks[3].IsDisabled, "The actor should not be disabled.");
        }

        [Test]
        public void EconomySystem_CalculatesEconomy()
        {
            // Arrange
            var mocks = new List<MockEconomyActor>()
            {
                new()
                {
                    ResourceChange = new Dictionary<ResourceType, float>
                    {
                        { ResourceType.Power, 200f } // Force negative resource
                    }
                }, 
                new()
                {
                    ResourceChange = new Dictionary<ResourceType, float>
                    {
                        { ResourceType.Power, -100f }, // Force negative resource
                        { ResourceType.Iron, 200f } // Force negative resource
                    }
                }, 
                new()
                {
                    ResourceChange = new Dictionary<ResourceType, float>
                    {
                        { ResourceType.Power, -200f }, // Force negative resource
                        { ResourceType.Iron, 300f } // Force negative resource
                    }
                }, 
                new()
                {
                    ResourceChange = new Dictionary<ResourceType, float>
                    {
                        { ResourceType.Power, -100f }, // Force negative resource
                        { ResourceType.Iron, 150f } // Force negative resource
                    }
                }
            };

            mocks.ForEach(mock => _economySystem.RegisterActor(mock));

            // Act
            _economySystem.UpdateEconomy();

            // Assert
            Assert.AreEqual(_economySystem.CurrentResources[ResourceType.Power].CurrentAmount, 0f, "Power should be 0.");
            Assert.AreEqual(_economySystem.CurrentResources[ResourceType.Iron].CurrentAmount, 350f, "Iron should be 350.");
        }
    }
}

public class MockEconomyActor : IEconomyActor
{
    public Dictionary<ResourceType, float> ResourceChange = new();
    public bool IsDisabled { get; private set; }

    public Dictionary<ResourceType, float> GetResourceAmount() => ResourceChange;

    public void Disable() => IsDisabled = true;
}
