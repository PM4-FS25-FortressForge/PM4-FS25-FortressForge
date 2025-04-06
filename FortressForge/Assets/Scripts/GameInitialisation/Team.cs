using System.Collections.Generic;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Represents a team in the game.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// Gets or sets the list of players in the team.
        /// </summary>
        public List<Player> Players { get; set; }
    }
}