using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FortressForge.EconomyManager
{
    /// <summary>
    /// Handles core logic for the in-game economy system, including resource tracking,
    /// periodic updates, and balancing based on the consumption and production of registered actors.
    /// </summary>
    public class EconomySystem
    {
        private static readonly ResourceType[] _allResourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        private readonly Dictionary<ResourceType, Resource> _currentResources = new();

        /// <summary>
        /// Provides read-only access to the current state of all resources.
        /// </summary>
        public IReadOnlyDictionary<ResourceType, Resource> CurrentResources => _currentResources;

        // List of all active resource-producing or consuming actors.
        private readonly List<IEconomyActor> _economyActors = new();

        /// <summary>
        /// Initializes the economy system with default values for all resource types.
        /// </summary>
        public EconomySystem(Dictionary<ResourceType, float> maxValues = null)
        {
            foreach (ResourceType type in _allResourceTypes)
            {
                float max = maxValues != null && maxValues.TryGetValue(type, out var value)
                    ? value
                    : float.MaxValue;

                _currentResources[type] = new Resource(type, max);
            }
        }

        /// <summary>
        /// Registers a new economy actor to participate in resource production or consumption.
        /// </summary>
        /// <param name="economyActor">The actor to register.</param>
        public void RegisterActor(IEconomyActor economyActor)
        {
            if (!_economyActors.Contains(economyActor))
                _economyActors.Add(economyActor);
        }

        /// <summary>
        /// Unregisters an economy actor, typically when it is removed or destroyed.
        /// </summary>
        /// <param name="consumer">The actor to remove.</param>
        public void RemoveActor(IEconomyActor consumer)
        {
            _economyActors.Remove(consumer);
        }

        /// <summary>
        /// Executes a full economic update: collects resource changes from all actors,
        /// stabilizes the economy (disabling actors if needed), and updates the current resources.
        /// </summary>
        public void UpdateEconomy()
        {
            var resourceChanges = CalculateNewResourcesChanges();
            var newResources = CalculateSumOfNewResources(resourceChanges);
            var positiveNewResources = StabilizeEconomy(newResources, resourceChanges);

            foreach (ResourceType resourceType in _allResourceTypes)
            {
                _currentResources[resourceType].CurrentAmount = positiveNewResources[resourceType];
            }
        }

        /// <summary>
        /// Disables actors that cause negative resource balances until all resources are non-negative.
        /// This modifies the newResources dictionary in-place.
        /// </summary>
        /// <param name="newResources">The total projected resource amounts after applying changes.</param>
        /// <param name="resourceChanges">The list of all actor resource deltas.</param>
        /// <returns>The adjusted resource totals after stabilizing the economy.</returns>
        private Dictionary<ResourceType, float> StabilizeEconomy(Dictionary<ResourceType, float> newResources, List<(IEconomyActor, Dictionary<ResourceType, float>)> resourceChanges)
        {
            do
            {
                foreach (ResourceType resourceType in _allResourceTypes)
                {
                    if (newResources[resourceType] >= 0) continue;

                    var consumersAscendingByConsumption = resourceChanges
                        .Where(rc => rc.Item2.ContainsKey(resourceType) && rc.Item2[resourceType] < 0)
                        .OrderBy(rc => rc.Item2.ContainsKey(resourceType) ? rc.Item2[resourceType] : 0)
                        .ToList();

                    foreach (var consumer in consumersAscendingByConsumption)
                    {
                        if (newResources[resourceType] >= 0) break;

                        consumer.Item1.Disable();
                        resourceChanges.Remove(consumer);
                        Debug.Log($"Disabled {consumer.Item1} due to negative {resourceType} balance.");

                        foreach (var resourceFromChange in consumer.Item2)
                        {
                            newResources[resourceFromChange.Key] -= resourceFromChange.Value;
                        }
                    }
                }
            } while (newResources.Any(r => r.Value < 0));

            return newResources;
        }

        /// <summary>
        /// Aggregates the current resource state with all actor-provided changes for the current tick.
        /// </summary>
        /// <param name="resourceChanges">All actor contributions or consumptions for this tick.</param>
        /// <returns>A dictionary representing projected new resource totals.</returns>
        private Dictionary<ResourceType, float> CalculateSumOfNewResources(List<(IEconomyActor, Dictionary<ResourceType, float>)> resourceChanges)
        {
            var newResources = new Dictionary<ResourceType, float>();
            foreach (ResourceType type in _allResourceTypes)
            {
                newResources[type] = _currentResources[type].CurrentAmount;
            }

            foreach (var resource in resourceChanges)
            {
                foreach (var resourceFromChange in resource.Item2)
                {
                    newResources[resourceFromChange.Key] += resourceFromChange.Value;
                }
            }

            return newResources;
        }

        /// <summary>
        /// Collects resource changes (both production and consumption) from all active economy actors.
        /// </summary>
        /// <returns>A list of tuples pairing actors with their respective resource changes.</returns>
        private List<(IEconomyActor, Dictionary<ResourceType, float>)> CalculateNewResourcesChanges()
        {
            var resourceChanges = new List<(IEconomyActor, Dictionary<ResourceType, float>)>();

            foreach (var resourceActor in _economyActors)
            {
                resourceChanges.Add((resourceActor, resourceActor.GetResourceAmount()));
            }

            return resourceChanges;
        }
    }
}
