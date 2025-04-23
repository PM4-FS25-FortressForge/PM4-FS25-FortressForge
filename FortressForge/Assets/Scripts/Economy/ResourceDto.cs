using System;
using FortressForge.Economy;

namespace FortressForge.Networking.Dto
{
    /// <summary>
    /// Represents a single resource in the game, such as Iron or Power.
    /// Tracks the current amount, its type, and an optional maximum cap.
    /// Dto of Resource for network serialization.
    /// </summary>
    [Serializable]
    public struct ResourceDto
    {
        public ResourceType Type;
        public float CurrentAmount;
        public float MaxAmount;
    }
}