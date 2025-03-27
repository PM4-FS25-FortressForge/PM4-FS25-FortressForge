using System;
using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.EconomyManager
{
    public class IronMine : MonoBehaviour, IEconomyActor
    {
        private EconomyManager _economyManager;
        private bool _enabled = true;

        // Return production value based on resource type
        public Dictionary<ResourceType, float> GetResourceAmount()
        {
            if (!_enabled)
            {
                return new Dictionary<ResourceType, float>();
            }
            
            return new Dictionary<ResourceType, float>()
            {
                { ResourceType.Iron, 100 },
                { ResourceType.Power, -50 }
            };
        }

        public void Disable()
        {
            _enabled = false;
        }
    }
}