using FortressForge.Economy;

namespace FortressForge.Serializables
{
    /// <summary>
    /// Represents a serializable pair of a resource type and its associated rate.
    /// </summary>
    [System.Serializable]
    public struct RessourceTypeRate
    {
        public ResourceType Type;
        public float Rate;
    }
}