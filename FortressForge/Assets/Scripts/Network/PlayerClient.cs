using Random = UnityEngine.Random;

namespace FortressForge.Network
{
    /// <summary>
    /// Represents a player in the Network
    /// </summary>
    public class PlayerClient
    {
        //  These 3 field have!!! to be public for the fishnet serializer to work otherwise they won't be serialized
        // also don't remove the set accessor otherwise the serializer won't work
        public string PlayerName { get; set; }
        public int PlayerID { get; set; }
        public bool IsHost { get; set; }

        /// <summary>
        ///  Default constructor needed for the serializer
        /// </summary>
        public PlayerClient()
        {
            PlayerName = "DefaultPlayer";
            PlayerID = GeneratePlayerId();
            IsHost = false;
        }

        public PlayerClient(string playerName = "DefaultPlayer", int? playerId = null, bool isHost = false)
        {
            PlayerName = playerName;
            PlayerID = playerId ?? GeneratePlayerId();
            IsHost = isHost;
        }

        /// <summary>
        /// Generates a random player ID
        /// </summary>
        /// <returns>A random int between 0 and 100000</returns>
        private static int GeneratePlayerId()
        {
            return Random.Range(0, 100000);
        }

        /// <summary>
        /// Returns a string representation of the player for debugging
        /// </summary>
        /// <returns>A string representation of the player</returns>
        public override string ToString()
        {
            return "Player Name: " + PlayerName + " Player Id: " + PlayerID + " Is Host: " + IsHost;
        }

        public override bool Equals(object obj)
        {
            if (obj is PlayerClient player)
            {
                return PlayerName == player.PlayerName && PlayerID == player.PlayerID && IsHost == player.IsHost;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return PlayerName.GetHashCode() + PlayerID.GetHashCode() + IsHost.GetHashCode();
        }
    }
}