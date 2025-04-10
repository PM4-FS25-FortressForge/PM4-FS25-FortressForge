using System;
using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.Economy
{
    public class IronMine : IEconomyActor
    {
        private EconomyManager _economyManager;
        private bool _enabled = true;

        // Return production value based on resource type
        public Dictionary<ResourceType, float> GetNetResourceChange()
        {
            if (!_enabled)
            {
                return new Dictionary<ResourceType, float>();
            }
            
            return new Dictionary<ResourceType, float>()
            {
                { ResourceType.Metal, 100 },
                { ResourceType.Power, -50 }
            };
        }
        
        public Dictionary<ResourceType, float> GetBuildCost()
        {
            return new Dictionary<ResourceType, float>()
            {
                { ResourceType.Metal, 100 },
                { ResourceType.Power, 50 }
            };
        }

        public void Disable()
        {
            _enabled = false;
        }
    }
}