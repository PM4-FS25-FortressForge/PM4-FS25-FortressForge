// ResourceManager.cs

using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.EconomyManager
{
    public class EconomyManager : MonoBehaviour
    {
        private const float RESOURCE_UPDATE_INTERVAL = 1f;

        private readonly Dictionary<ResourceType, float> _currentResources = new();

        // Lists to keep track of resource actors
        private readonly List<IEconomyActor> _economyActors = new();

        private void Start()
        {
            // Initialize resources
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                _currentResources[type] = 0f;
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
            var resourceTypes = System.Enum.GetValues(typeof(ResourceType));
            
            // Calculate resources from all actors
            foreach (var resourceActor in _economyActors)
            {
                foreach (ResourceType resourceType in resourceTypes)
                {
                    _currentResources[resourceType] += resourceActor.GetResourceAmount(resourceType);
                }
            }
        }
    }
}