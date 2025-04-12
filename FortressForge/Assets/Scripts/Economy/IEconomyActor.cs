using System.Collections.Generic;

namespace FortressForge.Economy
{
    /// <summary>
    /// Interface that all economy-related actors (e.g., mines, factories) must implement.
    /// Defines how they provide resource changes and handle disabling.
    /// </summary>
    public interface IEconomyActor
    {
        /// <summary>
        /// Returns a dictionary mapping each resource type to the amount produced or consumed.
        /// Positive values indicate production, negative values indicate consumption.
        /// </summary>
        Dictionary<ResourceType, float> GetNetResourceChange();

        /// <summary>
        /// Returns a dictionary mapping each resource type to the amount required for building or operating.
        /// </summary>
        Dictionary<ResourceType, float> GetBuildCost();
         
        /// <summary>
        /// Disables the actor, typically when it's consuming too many resources or has been shut down.
        /// </summary>
        void Disable();
    }
}