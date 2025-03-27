using System;
using UnityEngine;

namespace FortressForge.EconomyManager
{
    /// <summary>
    /// Unity MonoBehaviour responsible for managing the runtime lifecycle of the economy system.
    /// Initializes and periodically updates the economy logic.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        private const float RESOURCE_UPDATE_INTERVAL = 1f;

        // Core economy system logic container
        private readonly EconomySystem _economySystem = new EconomySystem();

        /// <summary>
        /// Initializes the economy manager and starts periodic economy updates.
        /// Registers default actors for demonstration.
        /// </summary>
        public void Init()
        {
            // Call update resource each second
            InvokeRepeating(nameof(_economySystem.UpdateEconomy), 0, RESOURCE_UPDATE_INTERVAL);

            // Example actor registrations
            _economySystem.RegisterActor(gameObject.AddComponent<IronMine>());
            _economySystem.RegisterActor(gameObject.AddComponent<IronMine>());
        }
    }
}