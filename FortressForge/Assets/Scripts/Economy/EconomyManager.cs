using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using FortressForge.BuildingSystem.BuildManager;
using UnityEngine;

namespace FortressForge.Economy
{
    /// <summary>
    /// Unity MonoBehaviour responsible for managing the runtime lifecycle of the economy system.
    /// Initializes and periodically updates the economy logic.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public EconomySystem EconomySystem => _economySystem;
        
        private const float RESOURCE_UPDATE_INTERVAL = 1f;

        // Core economy system logic container
        private EconomySystem _economySystem;

        /// <summary>
        /// Initializes the economy manager and starts periodic economy updates.
        /// Registers default actors for demonstration.
        /// </summary>
        public void Init(BuildingManager buildingManager)
        {
            // Example for max value application
            var maxValues = new Dictionary<ResourceType, float>
            {
                { ResourceType.Power, 2000f },
                { ResourceType.Metal, 2000f },
                { ResourceType.Amunition, 2000f },
                { ResourceType.Concrete, 2000f },
            };
            
            _economySystem = new EconomySystem(buildingManager, maxValues);
            
            // Call update resource each second
            InvokeRepeating(nameof(UpdateEconomy), 0, RESOURCE_UPDATE_INTERVAL);
        }

        private void UpdateEconomy()
        {
            _economySystem.UpdateEconomy();
        }
    }
}