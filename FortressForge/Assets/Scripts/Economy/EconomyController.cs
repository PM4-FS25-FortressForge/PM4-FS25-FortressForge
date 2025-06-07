using System;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildManager;
using UnityEngine;

namespace FortressForge.Economy
{
    /// <summary>
    /// Unity MonoBehaviour responsible for managing the runtime lifecycle of the economy system.
    /// Initializes and periodically updates the economy logic.
    /// </summary>
    public class EconomyController : MonoBehaviour
    {
        /// <summary>
        /// Gets the current economy system instance.
        /// </summary>
        public EconomySystem EconomySystem => _economySystem;
        
        /// <summary>
        /// The interval in seconds between resource updates.
        /// </summary>
        private const float RESOURCE_UPDATE_INTERVAL = 1f;

        /// <summary>
        /// Core economy system logic container.
        /// </summary>
        private EconomySystem _economySystem;

        /// <summary>
        /// Initializes the economy manager and starts periodic economy updates.
        /// </summary>
        /// <param name="economySystem">The economy system to manage.</param>
        public void Init(EconomySystem economySystem)
        {
            _economySystem = economySystem;
            
            // Call update resource each second
            InvokeRepeating(nameof(UpdateEconomy), 0, RESOURCE_UPDATE_INTERVAL);
        }

        /// <summary>
        /// Periodically updates the economy system.
        /// </summary>
        private void UpdateEconomy()
        {
            _economySystem.UpdateEconomy();
        }
    }
}