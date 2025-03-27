namespace FortressForge.EconomyManager
{
    /// <summary>
    /// Represents a single resource in the game, such as Iron or Power.
    /// Tracks the current amount and its type.
    /// </summary>
    public class Resource
    {
        private readonly ResourceType _type;

        /// <summary>
        /// Creates a new resource instance of the specified type.
        /// </summary>
        /// <param name="type">The type of resource this instance represents.</param>
        public Resource(ResourceType type)
        {
            _type = type;
        }

        /// <summary>
        /// The current quantity of this resource.
        /// Can be increased or decreased by economy actors.
        /// </summary>
        public float CurrentAmount { get; set; } = 0;
    }
}