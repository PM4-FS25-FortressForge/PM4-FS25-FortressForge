using Random = UnityEngine.Random;

namespace FortressForge.Network
{
    /// <summary>
    /// Represents a player in the Network
    /// </summary>
    public class PlayerClient
    {
        //  These 3 field have!!! to be public for the fishnet serializer to work otherwise they won't be serialized
        public string playerName;
        public int playerId;
        public bool isHost;

        /// <summary>
        ///  Default constructor needed for the serializer
        /// </summary>
        public PlayerClient()
        {
            playerName = "DefaultPlayer";
            playerId = GeneratePlayerId();
            isHost = false;
        }

        public PlayerClient(string playerName, int playerId, bool isHost)
        {
            this.playerName = playerName;
            this.playerId = playerId;
            this.isHost = isHost;
        }

        public PlayerClient(string playerName, int playerId)
        {
            this.playerName = playerName;
            this.playerId = playerId;
            isHost = false;
        }

        public PlayerClient(string playerName)
        {
            this.playerName = playerName;
            playerId = GeneratePlayerId();
            isHost = false;
        }

        public PlayerClient(string playerName, bool isHost)
        {
            this.playerName = playerName;
            playerId = GeneratePlayerId();
            this.isHost = isHost;
        }

        /// <summary>
        /// Get the players name
        /// </summary>
        /// <returns>The players name</returns>
        public string GetPlayerName()
        {
            return playerName;
        }

        /// <summary>
        /// Get the players ID
        /// </summary>
        /// <returns>The players ID</returns>
        public int GetPlayerId()
        {
            return playerId;
        }

        /// <summary>
        /// Set the players ID
        /// </summary>
        /// <param name="playerIdParam">The players ID</param>
        public void SetPlayerId(int playerIdParam)
        {
            playerId = playerIdParam;
        }

        /// <summary>
        /// Get if the player is the host
        /// </summary>
        /// <returns>True if the player is the host</returns>
        public bool IsHost()
        {
            return isHost;
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
            return "Player Name: " + playerName + " Player Id: " + playerId + " Is Host: " + isHost;
        }
    }
}