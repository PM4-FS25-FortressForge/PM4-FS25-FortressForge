using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FortressForge.Economy;
using FortressForge.Networking.Dto;
using UnityEngine;

namespace FortressForge.Networking
{
    public class EconomySync : NetworkBehaviour
    {
        private readonly SyncDictionary<ResourceType, ResourceDto> _syncedResources = new();

        public SyncDictionary<ResourceType, ResourceDto> SyncedResources => _syncedResources;

        private EconomySystem _economySystem;

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