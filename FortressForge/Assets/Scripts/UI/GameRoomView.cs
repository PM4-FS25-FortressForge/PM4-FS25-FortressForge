using System;
using System.Collections.Generic;
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

        private List<PlayerClient> _players;
        
        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;

            _hostOptions = root.Q<VisualElement>("HostOptions");
            _titleLabel = root.Q<Label>("TitleLabel");
            _playerList = root.Q<ListView>("PlayerList");
            _passwordSettingsLabel = root.Q<Label>("PasswordSettings");
            _passwordContainer = root.Q<VisualElement>("PasswordContainer");
            _ipContainer = root.Q<VisualElement>("IPContainer");
            _ipSettingsLabel = root.Q<Label>("IPSettings");
            
            _players = new List<PlayerClient>();

            _playerList.itemsSource = _players;
            Func<VisualElement> makeItem = () => new Label();
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                if (i >= 0 && i < _players.Count && e is Label label)
                {
                    label.text = _players[i].PlayerID + " " + _players[i].PlayerName;
                }
            };
            _playerList.makeItem = makeItem;
            _playerList.bindItem = bindItem;
        }

        /// <summary>
        /// Set up the game room view with the specified player name and whether the player is the host or not
        /// </summary>
        /// <param name="isHost">True if the player is the host, false otherwise</param>
        /// <param name="password">The password for the game room(this is not required as it's only for the host to display)</param>
        /// <param name="serverIP">The IP address of the server</param>
        public void SetupGameRoom(bool isHost, string password = "", string serverIP = "")
        {
            _titleLabel.text = isHost ? "Game Room (Host)" : "Game Room (Client)";

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
        }

        /// <summary>
        /// Populate the player list
        /// </summary>
        private void PopulatePlayerList()
        {
            _playerList.itemsSource = _players;
            _playerList.RefreshItems();
        }

        /// <summary>
        /// Add a player to the list
        /// </summary>
        /// <param name="player">The player to add</param>
        public void AddPlayerToList(PlayerClient player)
        {
            _players.Add(player);
            PopulatePlayerList();
        }

        /// <summary>
        /// Remove a player from the list
        /// </summary>
        /// <param name="player">The player to remove</param>
        private void RemovePlayerFromList(PlayerClient player)
        {
            _players.Remove(player);
            PopulatePlayerList();
        }


        /// <summary>
        /// Remove a player from the list by their ID
        /// </summary>
        /// <param name="playerId">The ID of the player to remove</param>
        public void RemovePlayerFromListById(int playerId)
        {
            PlayerClient player = _players.Find(p => p.PlayerID == playerId);
            if (player != null)
            {
                RemovePlayerFromList(player);
            }
        }

        /// <summary>
        /// Get the players in the game room
        /// </summary>
        /// <returns>The players in the game room</returns>
        public List<PlayerClient> GetPlayers()
        {
            return _players;
        }

        /// <summary>
        /// Set the players in the game room
        /// </summary>
        /// <param name="players">The players in the game room</param>
        public void SetPlayers(List<PlayerClient> players)
        {
            _players = players;
            PopulatePlayerList();
        }
    }
}