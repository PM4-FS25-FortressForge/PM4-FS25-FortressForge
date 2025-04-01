using Random = UnityEngine.Random;

namespace FortressForge.Network
{
    /// <summary>
    /// Represents a player in the Network
    /// </summary>
    public class PlayerClient
    {
        //  These 3 field have!!! to be public for the fishnet serializer to work otherwise they won't be serialized
        public readonly string PlayerName;
        public int PlayerId;
        public readonly bool IsHost;

        /// <summary>
        ///  Default constructor needed for the serializer
        /// </summary>
        public PlayerClient()
        {
            PlayerName = "DefaultPlayer";
            PlayerId = GeneratePlayerId();
            IsHost = false;
        }

        public PlayerClient(string playerName, int playerId, bool isHost)
        {
            PlayerName = playerName;
            PlayerId = playerId;
            IsHost = isHost;
        }

        public PlayerClient(string playerName, int playerId)
        {
            PlayerName = playerName;
            PlayerId = playerId;
            IsHost = false;
        }

        public PlayerClient(string playerName)
        {
            PlayerName = playerName;
            PlayerId = GeneratePlayerId();
            IsHost = false;
        }

        public PlayerClient(string playerName, bool isHost)
        {
            PlayerName = playerName;
            PlayerId = GeneratePlayerId();
            IsHost = isHost;
        }

        /// <summary>
        /// Get the players name
        /// </summary>
        /// <returns>The players name</returns>
        public string GetPlayerName()
        {
            return PlayerName;
        }

        /// <summary>
        /// Get the players ID
        /// </summary>
        /// <returns>The players ID</returns>
        public int GetPlayerId()
        {
            return PlayerId;
        }

        /// <summary>
        /// Set the players ID
        /// </summary>
        /// <param name="playerIdParam">The players ID</param>
        public void SetPlayerId(int playerIdParam)
        {
            PlayerId = playerIdParam;
        }

        /// <summary>
        /// Get if the player is the host
        /// </summary>
        /// <returns>True if the player is the host</returns>
        public bool GetIsHost()
        {
            return IsHost;
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
            return "Player Name: " + PlayerName + " Player Id: " + PlayerId + " Is Host: " + IsHost;
        }

        public override bool Equals(object obj)
        {
            if (obj is PlayerClient player)
            {
                return PlayerName == player.PlayerName && PlayerId == player.PlayerId && IsHost == player.IsHost;
            }

            return false;
        }
        
        public override int GetHashCode()
        {
            return PlayerName.GetHashCode() + PlayerId.GetHashCode() + IsHost.GetHashCode();
        }
    }
}