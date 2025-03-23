using System.Collections.Generic;

namespace FortressForge.EconomyManager
{
    public interface IEconomyActor
    {
        Dictionary<ResourceType, float> GetResourceAmount();
        
        void Disable();
    }
}