using System.Collections.Generic;
using FishNet.Object;
using FishNet.Transporting;
using FortressForge.UI;
using FortressForge.UI.Manager;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.Network
{
    public class GameRoomSynchronisation : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsOwner)
            {
                var viewManager = GameObject.Find("ViewManager").GetComponent<ViewManager>();
                PlayerClient player = viewManager.GetPlayerClient();
                player.SetPlayerId(OwnerId);
                InformServerAboutNewPlayer(player);

                ServerManager.OnRemoteConnectionState += (connectionId, state) =>
                {
                    // check if a client has disconnected if so remove the player from the list
                    if (state.ConnectionState == RemoteConnectionState.Stopped)
                    {
                        InformServerAboutPlayerLeaving(connectionId.ClientId);
                    }
                };
            }
            else
            {
                //SetupGameRoom(false, "Client");
                //gameObject.GetComponent<GameRoomSyncronisation>().enabled = false;
            }
        }

        [ServerRpc]
        private void InformServerAboutNewPlayer(PlayerClient player)
        {
            UIDocument gameRoomView = GameObject.Find("GameRoomView").GetComponent<UIDocument>();
            gameRoomView.GetComponent<GameRoomView>().AddPlayerToList(player);
            List<PlayerClient> players = gameRoomView.GetComponent<GameRoomView>().GetPlayers();
            InformClientsAboutNewPlayer(players);
        }

        [ObserversRpc]
        private void InformClientsAboutNewPlayer(List<PlayerClient> players)
        {
            UIDocument gameRoomView = GameObject.Find("GameRoomView").GetComponent<UIDocument>();
            gameRoomView.GetComponent<GameRoomView>().SetPlayers(players);
        }

        [ServerRpc]
        private void InformServerAboutPlayerLeaving(int playerId)
        {
            UIDocument gameRoomView = GameObject.Find("GameRoomView").GetComponent<UIDocument>();
            gameRoomView.GetComponent<GameRoomView>().RemovePlayerFromListById(playerId);
            InformOversersAboutPlayerLeaving(playerId);
        }

        [ObserversRpc]
        private void InformOversersAboutPlayerLeaving(int playerId)
        {
            UIDocument gameRoomView = GameObject.Find("GameRoomView").GetComponent<UIDocument>();
            gameRoomView.GetComponent<GameRoomView>().RemovePlayerFromListById(playerId);
        }

        private void AddPlayerToRoom(PlayerClient player)
        {
            UIDocument gameRoomView = GameObject.Find("GameRoomView").GetComponent<UIDocument>();
            gameRoomView.GetComponent<GameRoomView>().AddPlayerToList(player);
        }
    }
}