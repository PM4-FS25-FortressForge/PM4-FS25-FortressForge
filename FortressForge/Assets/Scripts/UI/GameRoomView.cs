using System.Collections.Generic;
using System.Data;
using System.Net;
using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using FortressForge.Network;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
    /// <summary>
    /// Manages the game room view and its components
    /// </summary>
    public class GameRoomView : MonoBehaviour
    {
        private VisualElement _hostOptions;
        private Label _titleLabel;
        private ListView _playerList;
        private Label _passwordSettingsLabel;
        private VisualElement _passwordContainer;
        private VisualElement _ipContainer;
        private Label _ipSettingsLabel;
        private Button _exitButton;

        private List<PlayerClient> _players = new(); // Spielerliste speichern


        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _hostOptions = root.Q<VisualElement>("HostOptions");
            _titleLabel = root.Q<Label>("TitleLabel");
            _playerList = root.Q<ListView>("PlayerList");
            _passwordSettingsLabel = root.Q<Label>("PasswordSettings");
            _passwordContainer = root.Q<VisualElement>("PasswordContainer");
            _ipContainer = root.Q<VisualElement>("IPContainer");
            _ipSettingsLabel = root.Q<Label>("IPSettings");
            _exitButton = root.Q<Button>("ExitButton");


            _titleLabel.text = "Game Room Testing";

            // InitializePlayerList();
        }


        // public override void OnStartServer()
        // {
        //     base.OnStartServer();
        //
        //     //_titleLabel.text = "Game Room (Host)";
        //
        //     Debug.Log("OnStartServer called" + ServerManager.Clients.Count + " " + ServerManager.Clients);
        //
        //     foreach (var client in ServerManager.Clients)
        //     {
        //         Debug.Log(client);
        //     }
        //
        //     //AddPlayerToRoom("test");
        //
        //     //SetupGameRoom(true, "Host", "", GetLocalIPAddress());
        // }

        // public override void OnStartClient()
        // {
        //     base.OnStartClient();
        //
        //     //_titleLabel.text = "Game Room (Client)";
        //
        //     Debug.Log("OnStartClient called " + base.IsController + " " + base.IsOwner + " " + base.HasAuthority + " " + base.Owner);
        //     if (IsOwner)
        //     {
        //         SetupGameRoom(true, "Host", "", GetLocalIPAddress());
        //     }
        //     else
        //     {
        //         //SetupGameRoom(false, "Client");
        //         gameObject.GetComponent<GameRoomView>().enabled = false;
        //     }
        //
        // }

        /// <summary>
        /// Set up the game room view with the specified player name and whether the player is the host or not
        /// </summary>
        /// <param name="isHost">True if the player is the host, false otherwise</param>
        /// <param name="playerName"> The name of the player</param>
        /// <param name="password">The password for the game room(this is not required as it's only for the host to display)</param>
        /// <param name="serverIP">The IP address of the server</param>
        public void SetupGameRoom(bool isHost, string playerName, string password = "", string serverIP = "")
        {
            Debug.Log("SSDSDSDDS");
            Debug.Log("Null check " + _titleLabel);
            _titleLabel.text = isHost ? "Game Room (Host)" : "Game Room (Client)";

            // Host kann zusätzliche Optionen sehen
            _hostOptions.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;
            _passwordContainer.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;
            _ipContainer.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;

            if (!string.IsNullOrEmpty(password))
            {
                _passwordSettingsLabel.text = password;
            }

            if (!string.IsNullOrEmpty(serverIP))
            {
                _ipSettingsLabel.text = serverIP;
            }

            // _exitButton.RegisterCallback<ClickEvent>(evt =>
            // {
            //     if (isHost)
            //     {
            //         AddPlayerToRoom("dsdsddsd");
            //         //InstanceFinder.ServerManager.StopConnection(true);
            //     }
            //     else
            //     {
            //         //InstanceFinder.ClientManager.StopConnection();
            //         AddPlayerToRoom("baum");
            //     }
            //     // ShowLobbyView();
            // });


            // Debug.Log("ServerManager.Clients.Count " + InstanceFinder.ServerManager.Clients.Count + " " +
            //           InstanceFinder.ServerManager.Clients.Values);
            // foreach (var client in InstanceFinder.ServerManager.Clients.Values)
            // {
            //     Debug.Log(client);
            // }


            // if (!isHost) return;
            // InstanceFinder.ServerManager.OnRemoteConnectionState += (connectionId, state) =>
            // {
            //     // check if a client has disconnected if so remove the player from the list
            //     if (state.ConnectionState == RemoteConnectionState.Stopped)
            //     {
            //         var player = _players.Find(p => p.GetPlayerId() == connectionId.ClientId);
            //         if (player != null)
            //         {
            //             RemovePlayerFromList(player);
            //         }
            //     }
            //     
            //     
            // };
        }

        private void PopulatePlayerList(List<PlayerClient> playerList)
        {
            _playerList.Clear();
            _playerList.hierarchy.Clear();
            foreach (var player in playerList)
            {
                var playerElement = new VisualElement();
                playerElement.Add(new Label(player.GetPlayerName()));
                AddPlayer(playerElement);
            }

            // _playerList.itemsSource = playerList;
            _playerList.Rebuild();
        }

        private void AddPlayer(VisualElement playerElement)
        {
            _playerList.hierarchy.Add(playerElement);
        }

        public void AddPlayerToList(PlayerClient player)
        {
            _players.Add(player);
            PopulatePlayerList(_players);
        }

        private void RemovePlayerFromList(PlayerClient player)
        {
            _players.Remove(player);
            PopulatePlayerList(_players);
        }


        public void RemovePlayerFromListById(int playerId)
        {
            var player = _players.Find(p => p.GetPlayerId() == playerId);
            if (player != null)
            {
                RemovePlayerFromList(player);
            }
        }

        public List<PlayerClient> GetPlayers()
        {
            return _players;
        }

        public void SetPlayers(List<PlayerClient> players)
        {
            _players = players;
            PopulatePlayerList(_players);
        }
    }
}