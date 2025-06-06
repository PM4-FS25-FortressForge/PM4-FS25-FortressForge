using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FortressForge.BuildingSystem.BuildManager;
using NUnit.Framework;
using UnityEngine;

namespace FortressForge.Economy
{
    /// <summary>
    /// Handles core logic for the in-game economy system, including resource tracking,
    /// periodic updates, and balancing based on the consumption and production of registered actors.
    /// </summary>
    public class EconomySystem
    {
        private const bool ENABLE_DEBUG_LOGGING = false;

        private static readonly ResourceType[] AllResourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        
        /// <summary>
        /// Provides read-only access to the current state of all resources.
        /// </summary>
        public IReadOnlyDictionary<ResourceType, Resource> CurrentResources => _currentResources;
        
        private readonly Dictionary<ResourceType, Resource> _currentResources = new();
        private readonly BuildingManager _buildingManager;
        public readonly GlobalEconomy GlobalEconomy;

        /// <summary>
        /// Initializes the economy system with default values for all resource types.
        /// </summary>
        public EconomySystem(BuildingManager buildingManager, GlobalEconomy globalEconomy, Dictionary<ResourceType, float> maxValues = null)
        {
            _buildingManager = buildingManager;
            GlobalEconomy = globalEconomy;

            foreach (ResourceType type in AllResourceTypes)
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
            List<(IEconomyActor, Dictionary<ResourceType, float>)> resourceChangesPerActor = CalculateNewResourcesChanges();
            Dictionary<ResourceType, float> totalResourceChanges = GetResourceChanges(resourceChangesPerActor);
            
            Dictionary<ResourceType, float> newResources = CalculateSumOfNewResources(totalResourceChanges);
            Dictionary<ResourceType, float> positiveNewResources = StabilizeEconomy(ref newResources, ref resourceChangesPerActor);
            
            // Revert magma changes to the global economy
            var resourceChanges = GetResourceChanges(resourceChangesPerActor);
            if (resourceChanges.TryGetValue(ResourceType.Magma, out var magmaResourceChange)) 
                ApplyMagmaChanges(magmaResourceChange);
            
            foreach (ResourceType resourceType in AllResourceTypes)
            {
                _currentResources[resourceType].SetCurrentAmountWithDeltaAmount(positiveNewResources[resourceType]);
            }
            
            if (ENABLE_DEBUG_LOGGING)
            {
                // Debug log for current resources
                string logMessage = _currentResources.Aggregate("Current Resources: ", 
                    (current, resource) => current + $"{resource.Key}: {resource.Value.CurrentAmount}, ");
                Debug.Log(logMessage);
            }
        }

        private static Dictionary<ResourceType, float> GetResourceChanges(List<(IEconomyActor, Dictionary<ResourceType, float>)> resourceChangesPerActor) {
            return resourceChangesPerActor
                .SelectMany(rc => rc.Item2)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(kvp => kvp.Value)
                );
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
                var currentResource = _currentResources[resource.Key];
                currentResource.CurrentAmount -= resource.Value;
            }
        }

        /// <summary>
        /// Disables actors that cause negative resource balances until all resources are non-negative.
        /// This modifies the newResources dictionary in-place.
        /// </summary>
        /// <param name="newResources">The total projected resource amounts after applying changes.</param>
        /// <param name="resourceChanges">The list of all actor resource deltas.</param>
        /// <returns>The adjusted resource totals after stabilizing the economy.</returns>
        private static Dictionary<ResourceType, float> StabilizeEconomy(ref Dictionary<ResourceType, float> newResources, ref List<(IEconomyActor, Dictionary<ResourceType, float>)> resourceChanges)
        {
            do
            {
                foreach (ResourceType resourceType in AllResourceTypes)
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
        private Dictionary<ResourceType, float> CalculateSumOfNewResources(Dictionary<ResourceType, float> resourceChanges)
        {
            var newResources = new Dictionary<ResourceType, float>();
            foreach (ResourceType type in AllResourceTypes) 
            {
                resourceChanges.TryGetValue(type, out var resourceChange);
                newResources[type] = _currentResources[type].CurrentAmount + resourceChange;
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

            foreach (var resourceActor in _buildingManager.PlacedBuildings.Select(data => data.BaseBuildingTemplate))
            {
                resourceChanges.Add((resourceActor, resourceActor.GetNetResourceChange()));
            }

            return resourceChanges;
        }

        /// <summary>
        /// Applies the local magma resource change to the global economy.
        /// Subtracts the local magma consumption/production from the global magma pool.
        /// If the global magma is depleted, additional handling can be implemented here.
        /// </summary>
        /// <param name="magmaResourceChange">The net change in magma for this tick (positive = produced, negative = consumed).</param>
        private void ApplyMagmaChanges(float magmaResourceChange) {
            GlobalEconomy.CurrentResources[ResourceType.Magma].AddAmountWithDeltaAmount(-magmaResourceChange);

            if (GlobalEconomy.CurrentResources[ResourceType.Magma].CurrentAmount <= 0) {
                // Handle the case when magma is depleted
            }
        }
    }
}
