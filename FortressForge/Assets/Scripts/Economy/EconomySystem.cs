using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FortressForge.BuildingSystem.BuildManager;
using UnityEngine;

namespace FortressForge.Economy
{
    /// <summary>
    /// Handles core logic for the in-game economy system, including resource tracking,
    /// periodic updates, and balancing based on the consumption and production of registered actors.
    /// </summary>
    public class EconomySystem
    {
        private const bool EnableDebugLogging = false;

        private static readonly ResourceType[] _allResourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        
        /// <summary>
        /// Provides read-only access to the current state of all resources.
        /// </summary>
        public IReadOnlyDictionary<ResourceType, Resource> CurrentResources => _currentResources;

        // List of all active resource-producing or consuming actors.
        private ReadOnlyCollection<IEconomyActor> EconomyActors => new (
            _buildingManager.PlacedBuildings.Cast<IEconomyActor>().ToList()
        );
        
        private readonly Dictionary<ResourceType, Resource> _currentResources = new();
        private readonly BuildingManager _buildingManager;

        /// <summary>
        /// Initializes the economy system with default values for all resource types.
        /// </summary>
        public EconomySystem(BuildingManager buildingManager, Dictionary<ResourceType, float> maxValues = null)
        {
            _buildingManager = buildingManager;
            
            foreach (ResourceType type in _allResourceTypes)
            {
                float max = maxValues != null && maxValues.TryGetValue(type, out var value)
                    ? value
                    : 0;

                _currentResources[type] = new Resource(type, max);
            }
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
            var changeRates = CalculateChangeRates(resourceChanges);
            
            foreach (ResourceType resourceType in _allResourceTypes)
            {
                _currentResources[resourceType].CurrentAmount = positiveNewResources[resourceType];
                _currentResources[resourceType].DeltaAmount = changeRates[resourceType];
            }
            
            if (EnableDebugLogging)
            {
                // Debug log for current resources
                string logMessage = _currentResources.Aggregate("Current Resources: ", 
                    (current, resource) => current + $"{resource.Key}: {resource.Value.CurrentAmount}, ");
                Debug.Log(logMessage);
            }
        }
        
        /// <summary>
        /// Calculates the change rates of resources based on the current state and the changes from actors.
        /// </summary>
        /// <param name="resourceChanges">Resource changes from actors.</param>
        /// <returns>Change rates for resources.</returns>
        private Dictionary<ResourceType, float> CalculateChangeRates(List<(IEconomyActor, Dictionary<ResourceType, float>)> resourceChanges)
        {
            var changeRates = new Dictionary<ResourceType, float>();
            
            foreach (var resource in _allResourceTypes)
            {
                changeRates[resource] = 0;
            }

            foreach (var resourceChange in resourceChanges)
            {
                foreach (var change in resourceChange.Item2)
                {
                    changeRates[change.Key] += change.Value;
                }
            }

            return changeRates;
        }
        

        /// <summary>
        /// Checks if there are sufficient resources available for the specified costs.
        /// </summary>
        /// <param name="resourceCosts">The resource amount.</param>
        /// <returns>True if there are sufficient resources.</returns>
        public bool CheckForSufficientResources(Dictionary<ResourceType, float> resourceCosts)
        {
            if (resourceCosts.Any(resource => 
                    _currentResources[resource.Key].CurrentAmount < resource.Value))
                return false;
            return true;
        }

        /// <summary>
        /// Deducts specified resources from the current resources. Will not check if there are sufficient resources. Use CheckForSufficientResources first.
        /// </summary>
        /// <param name="resourceCosts">The resource amount.</param>
        public void PayResource(Dictionary<ResourceType, float> resourceCosts)
        {
            foreach (var resource in resourceCosts)
            {
                _currentResources[resource.Key].CurrentAmount -= resource.Value;
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

                        // Disable must disable or reduce building drain
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

            foreach (var resourceActor in EconomyActors)
            {
                resourceChanges.Add((resourceActor, resourceActor.GetNetResourceChange()));
            }

            return resourceChanges;
        }
    }
}
