using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FortressForge.EconomyManager
{
    /// <summary>
    /// Manages the economy of the game.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        private const float RESOURCE_UPDATE_INTERVAL = 1f;

        private static readonly ResourceType[] _allResourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        private readonly Dictionary<ResourceType, Resource> _currentResources = new();

        // Lists to keep track of resource actors
        private readonly List<IEconomyActor> _economyActors = new();

        private void Start()
        {
            // Initialize resources
            foreach (ResourceType type in _allResourceTypes)
            {
                _currentResources[type] = new Resource();
            }
            
            // Call update resource each second
            InvokeRepeating(nameof(UpdateEconomy), 0, RESOURCE_UPDATE_INTERVAL);
        }

        // Register a new resource actors
        public void RegisterActor(IEconomyActor economyActor)
        {
            if (!_economyActors.Contains(economyActor))
                _economyActors.Add(economyActor);
        }

        // Optionally, remove when a building is destroyed
        public void RemoveActor(IEconomyActor consumer)
        {
            _economyActors.Remove(consumer);
        }

        // Call this method periodically (e.g., every few seconds) or in Update if needed.
        public void UpdateEconomy()
        {
            var resourceChanges = CalculateNewResourcesChanges();
            var newResources = CalculateSumOfNewResources(resourceChanges);
            
            var positiveNewResources = StabilizeEconomy(newResources, resourceChanges);
            
            // Update resources
            foreach (ResourceType resourceType in _allResourceTypes)
            {
                _currentResources[resourceType].CurrentAmount = positiveNewResources[resourceType];
            }
        }

        /// <summary>
        /// Removes resource changes until all resources are positive.
        /// On each removed resource, the building is disabled.
        /// This method modifies the newResources dictionary.
        /// </summary>
        /// <param name="newResources">The sums of the total resources</param>
        /// <param name="resourceChanges">A set of all the changes made in last calculation turn</param>
        /// <returns>Sum of all resource changes, added up.</returns>
        private Dictionary<ResourceType, float> StabilizeEconomy(Dictionary<ResourceType, float> newResources,List<(IEconomyActor, Dictionary<ResourceType, float>)> resourceChanges)
        {
            do 
            {
                // Check if there are any negative resources
                foreach (ResourceType resourceType in _allResourceTypes)
                {
                    if (newResources[resourceType] >= 0) continue;
                    
                    var consumersDescendingByConsumption = resourceChanges
                        .Where(rc => rc.Item2.ContainsKey(resourceType) && rc.Item2[resourceType] < 0)
                        .OrderByDescending(rc => rc.Item2.ContainsKey(resourceType) ? rc.Item2[resourceType] : 0)
                        .ToList();
                    
                    // Disable building with highest used resource and remove from produced value until it's positive
                    foreach (var consumer in consumersDescendingByConsumption)
                    {   
                        // Disable building and remove from produced value
                        consumer.Item1.Disable();
                        Debug.Log($"Disabled {consumer.Item1} due to negative {resourceType} balance.");
                        
                        foreach (var resourceFromChange in consumer.Item2)
                        {
                            newResources[resourceFromChange.Key] -= resourceFromChange.Value;
                        }
                    }
                }
            } while (newResources.Any(r => r.Value < 0)); // Repeat until all resources are positive
            
            return newResources;
        }

        private Dictionary<ResourceType, float> CalculateSumOfNewResources(List<(IEconomyActor, Dictionary<ResourceType, float>)> resourceChanges)
        {
            var newResources = new Dictionary<ResourceType, float>();
            foreach (ResourceType type in _allResourceTypes)
            {
                newResources[type] = _currentResources[type].CurrentAmount;
            }

            // Calculate sum of new resources
            foreach (var resource in resourceChanges)
            {
                foreach (var resourceFromChange in resource.Item2)
                {
                    newResources[resourceFromChange.Key] += resourceFromChange.Value;
                }
            }
            
            return newResources;
        }

        private List<(IEconomyActor, Dictionary<ResourceType, float>)> CalculateNewResourcesChanges()
        {
            var resourceChanges = new List<(IEconomyActor, Dictionary<ResourceType, float>)>();
            
            // Calculate resources from all actors
            foreach (var resourceActor in _economyActors)
            {
                var resources = resourceActor.GetResourceAmount();
                resourceChanges.Add((resourceActor, resources));
            }
            
            return resourceChanges;
        }
    }
}