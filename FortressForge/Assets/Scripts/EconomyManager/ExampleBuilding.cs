// IronMine.cs
using UnityEngine;

namespace FortressForge.EconomyManager
{
    public class IronMine : MonoBehaviour, IEconomyActor
    {
        public float productionRate = 5f; // Iron produced per update cycle
        private EconomyManager _economyManager;

        private void Start()
        {
            _economyManager = FindFirstObjectByType<EconomyManager>();
            if (_economyManager != null)
            {
                _economyManager.RegisterActor(this);
            }
        }

        // Return production value based on resource type
        public float GetResourceAmount(ResourceType resourceType)
        {
            if (resourceType == ResourceType.Iron)
                return productionRate;
            return 0;
        }

        private void OnDestroy()
        {
            if (_economyManager != null)
            {
                _economyManager.RemoveActor(this);
            }
        }
    }
}