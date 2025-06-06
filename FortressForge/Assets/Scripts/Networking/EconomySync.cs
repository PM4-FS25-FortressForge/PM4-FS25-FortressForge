using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FortressForge.Economy;
using FortressForge.Networking.Dto;
using UnityEngine;

namespace FortressForge.Networking
{
    /// <summary>
    /// Synchronizes economy resources across the network using FishNet.
    /// </summary>

    public class EconomySync : NetworkBehaviour
    {
        private readonly SyncDictionary<ResourceType, ResourceDto> _syncedResources = new();

        public SyncDictionary<ResourceType, ResourceDto> SyncedResources => _syncedResources;

        private EconomySystem _economySystem;
        
        /// <summary>
        /// Initializes the economy sync with the given economy system and sets up resource change listeners.
        /// </summary>
        /// <param name="economySystem">The economy system to synchronize.</param>

        public void Init(EconomySystem economySystem)
        {
            _economySystem = economySystem;
            GlobalEconomy globalEconomy = economySystem.GlobalEconomy;
            foreach (var resource in _economySystem.CurrentResources)
            {
                _syncedResources[resource.Key] = Convert(resource.Value);
                resource.Value.OnChanged += () =>
                {
                    _syncedResources[resource.Key] = Convert(resource.Value);
                };
            }

            // Add global resources manually
            var globalMagma = globalEconomy.CurrentResources[ResourceType.Magma];
            _syncedResources[ResourceType.GlobalMagma] = Convert(globalMagma, ResourceType.GlobalMagma);
            globalMagma.OnChanged += () =>
            {
                _syncedResources[ResourceType.GlobalMagma] = Convert(globalMagma, ResourceType.GlobalMagma);
            };
        }

        /// <summary>
        /// Converts a resource to a serializable DTO for network synchronization.
        /// </summary>
        /// <param name="resource">The resource to convert.</param>
        /// <param name="resourceType">Optional override for the resource type.</param>
        /// <returns>The converted resource DTO.</returns>

        private ResourceDto Convert(Resource resource, ResourceType? resourceType = null)
        {
            return new ResourceDto
            {
                Type = resourceType.HasValue ? resourceType.Value : resource.Type,
                CurrentAmount = resource.CurrentAmount,
                MaxAmount = resource.MaxAmount, 
                DeltaAmount = resource.DeltaAmount,
            };
        }
    }
}