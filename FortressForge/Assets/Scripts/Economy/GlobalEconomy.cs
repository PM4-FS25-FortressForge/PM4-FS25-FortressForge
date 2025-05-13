using System.Collections.Generic;

namespace FortressForge.Economy {
    /// <summary>
    /// Implements economy thats active for every client.
    /// As an example the Magma resource which is a limited resource on the map.
    /// </summary>
    public class GlobalEconomy {
        /// <summary>
        /// Provides read-only access to the current state of all resources.
        /// </summary>
        public IReadOnlyDictionary<ResourceType, Resource> CurrentResources => _currentResources;
        
        private readonly Dictionary<ResourceType, Resource> _currentResources = new();

        public GlobalEconomy(float magmaCount) {
            _currentResources[ResourceType.Magma] = new Resource(ResourceType.Magma, magmaCount, magmaCount);
        }
    }
}