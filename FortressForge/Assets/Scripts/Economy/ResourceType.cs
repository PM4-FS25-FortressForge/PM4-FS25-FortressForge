namespace FortressForge.Economy
{
    /// <summary>
    /// Enum representing all possible types of resources in the game.
    /// Expand this list as more resources are added.
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// A basic raw material, often produced by mines.
        /// </summary>
        Metal,

        /// <summary>
        /// Represents electrical energy, often generated or consumed by buildings.
        /// </summary>
        Power,
        
        /// <summary>
        /// The main source of energy and life. Used in various processes and buildings.
        /// </summary>
        Magma,
        
        /// <summary>
        /// Represents Magma shared across all clients. And the whole Planet.
        /// Only used for Networking.
        /// </summary>
        GlobalMagma,
    }
}