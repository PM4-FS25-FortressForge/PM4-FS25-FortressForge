using NUnit.Framework;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.Economy;
using FortressForge.BuildingSystem.BuildManager;

public class EconomySystem_PayResourceIfSufficientTests
{
    private EconomySystem _economySystem;

    [SetUp]
    public void SetUp()
    {
        var buildingManager = new BuildingManager(); // empty, not needed here
        var maxValues = new Dictionary<ResourceType, float>
        {
            { ResourceType.Metal, 100 },
            { ResourceType.Power, 100 },
            { ResourceType.Magma, 100 }
        };

        _economySystem = new EconomySystem(buildingManager, maxValues);

        // Set initial amounts manually for full control
        _economySystem.CurrentResources[ResourceType.Metal].CurrentAmount = 80;
        _economySystem.CurrentResources[ResourceType.Power].CurrentAmount = 50;
        _economySystem.CurrentResources[ResourceType.Magma].CurrentAmount = 0;
    }

    [Test]
    public void PayResourceIfSufficient_SufficientResources_ReturnsTrueAndDeducts()
    {
        var cost = new Dictionary<ResourceType, float>
        {
            { ResourceType.Metal, 30 },
            { ResourceType.Power, 20 }
        };

        var result = _economySystem.PayResourceIfSufficient(cost);

        Assert.IsTrue(result);
        Assert.AreEqual(50, _economySystem.CurrentResources[ResourceType.Metal].CurrentAmount);
        Assert.AreEqual(30, _economySystem.CurrentResources[ResourceType.Power].CurrentAmount);
    }

    [Test]
    public void PayResourceIfSufficient_ExactAmount_ReturnsTrueAndZeroesOut()
    {
        var cost = new Dictionary<ResourceType, float>
        {
            { ResourceType.Power, 50 }
        };

        var result = _economySystem.PayResourceIfSufficient(cost);

        Assert.IsTrue(result);
        Assert.AreEqual(0, _economySystem.CurrentResources[ResourceType.Power].CurrentAmount);
    }

    [Test]
    public void PayResourceIfSufficient_InsufficientResources_ReturnsFalseAndNoChange()
    {
        var cost = new Dictionary<ResourceType, float>
        {
            { ResourceType.Metal, 200 }
        };

        var result = _economySystem.PayResourceIfSufficient(cost);

        Assert.IsFalse(result);
        Assert.AreEqual(80, _economySystem.CurrentResources[ResourceType.Metal].CurrentAmount);
    }

    [Test]
    public void PayResourceIfSufficient_OneResourceInsufficient_AbortsAndPreservesAll()
    {
        var cost = new Dictionary<ResourceType, float>
        {
            { ResourceType.Metal, 50 },
            { ResourceType.Magma, 10 } // Not enough
        };

        var result = _economySystem.PayResourceIfSufficient(cost);

        Assert.IsFalse(result);
        Assert.AreEqual(80, _economySystem.CurrentResources[ResourceType.Metal].CurrentAmount);
        Assert.AreEqual(0, _economySystem.CurrentResources[ResourceType.Magma].CurrentAmount);
    }

    [Test]
    public void PayResourceIfSufficient_EmptyCost_ReturnsTrue()
    {
        var result = _economySystem.PayResourceIfSufficient(new Dictionary<ResourceType, float>());

        Assert.IsTrue(result);
        Assert.AreEqual(80, _economySystem.CurrentResources[ResourceType.Metal].CurrentAmount);
        Assert.AreEqual(50, _economySystem.CurrentResources[ResourceType.Power].CurrentAmount);
    }
}
