using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using FortressForge.UI;
using FortressForge.UI.Manager;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.Network
{
    /// <summary>
    /// Synchronises the game room between the server and the clients
    /// </summary>
    public class GameRoomSynchronisation : NetworkBehaviour
    {
        /// <summary>
        /// Called when the client starts
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner) return;
            ViewManager viewManager = GameObject.Find("ViewManager").GetComponent<ViewManager>();
            PlayerClient player = viewManager.GetPlayerClient();
            player.PlayerID = OwnerId;
            InformServerAboutNewPlayer(player);

            ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;
        }
        
        public override void OnStopClient()
        {
            base.OnStopClient();
            ServerManager.OnRemoteConnectionState -= HandleRemoteConnectionState;
        }
        
        
        /// <summary>
        /// Handle the remote connection state
        /// </summary>
        /// <param name="connection">The connection to handle</param>
        /// <param name="state">The state of the connection</param>
        private void HandleRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs state)
        {
            if (state.ConnectionState == RemoteConnectionState.Stopped)
            {
                InformServerAboutPlayerLeaving(connection.ClientId);
            }
        }

        /// <summary>
        /// Inform the server about a new player
        /// </summary>
        /// <param name="player">The player to inform the server about</param>
        [ServerRpc]
        private void InformServerAboutNewPlayer(PlayerClient player)
        {
            GameRoomView gameRoomView = GetGameRoomViewUIDocument();
            gameRoomView.AddPlayerToList(player);
            List<PlayerClient> players = gameRoomView.PlayersList;
            InformClientsAboutNewPlayer(players);
        }

        /// <summary>
        /// Inform the clients about a new player
        /// </summary>
        /// <param name="players">The list of players to inform the clients about</param>
        [ObserversRpc]
        private void InformClientsAboutNewPlayer(List<PlayerClient> players)
        {
            GetGameRoomViewUIDocument().PlayersList = players;
        }

        /// <summary>
        /// Inform the server about a player leaving
        /// </summary>
        /// <param name="playerId">The ID of the player that left</param>
        [ServerRpc]
        private void InformServerAboutPlayerLeaving(int playerId)
        {
            GetGameRoomViewUIDocument().RemovePlayerFromListById(playerId);
            InformObserversAboutPlayerLeaving(playerId);
        }

        /// <summary>
        /// Inform the observers about a player leaving
        /// </summary>
        /// <param name="playerId">The ID of the player that left</param>
        [ObserversRpc]
        private void InformObserversAboutPlayerLeaving(int playerId)
        {
            GetGameRoomViewUIDocument().RemovePlayerFromListById(playerId);
        }

        /// <summary>
        /// Get the GameRoomView from the UIDocument
        /// </summary>
        /// <returns>The GameRoomView Object</returns>
        private GameRoomView GetGameRoomViewUIDocument()
        {
            return GameObject.Find("GameRoomView").GetComponent<UIDocument>().GetComponent<GameRoomView>();
        }
    }
}