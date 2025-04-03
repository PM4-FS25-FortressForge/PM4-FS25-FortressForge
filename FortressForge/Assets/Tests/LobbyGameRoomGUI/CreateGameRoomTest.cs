using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FortressForge.Network;
using FortressForge.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using WaitForSeconds = UnityEngine.WaitForSeconds;

namespace Tests.LobbyGameRoomGUI
{
    /// <summary>
    /// This test class is used to test the creation of a game room and the UI elements that are part of it. It also tests the ability to join an existing game room.
    /// </summary>
    [TestFixture]
    public class CreateGameRoomTest : LobbyGameRoomTestBaseSetup
    {
        [UnitySetUp]
        public override IEnumerator SetUp()
        {
            yield return base.SetUp();
        }

        [UnityTest]
        public IEnumerator GameRoomCompleteTest()
        {
            yield return OpenGameRoom();

            yield return new WaitForSeconds(1f);

            List<PlayerClient> playerNames = BuildPlayerClients(new List<string> { "TestString" });

            yield return TestPlayerList(playerNames);

            yield return new WaitForSeconds(1f);

            yield return StartClientForFishnet();
            yield return new WaitForSeconds(1f);
            yield return StartClientForFishnet();
            yield return new WaitForSeconds(1f);
            yield return StartClientForFishnet("NetworkManagerClientToBeRemoved");

            yield return new WaitForSeconds(1f);

            Dictionary<int, NetworkConnection> connectedClients = InstanceFinder.NetworkManager.ServerManager.Clients;

            Assert.AreEqual(4, connectedClients.Count, "Amount of connected clients is not correct!");

            yield return new WaitForSeconds(1f);

            List<PlayerClient> playerNames2 = BuildPlayerClients(new List<string>
                { "TestString", "TestString", "TestString", "TestString" });

            yield return TestPlayerList(playerNames2);

            yield return new WaitForSeconds(1f);

            RemoveNetworkManagerClients(false, "NetworkManagerClientToBeRemoved");

            yield return new WaitForSeconds(1f);

            List<PlayerClient> playerNames23 = BuildPlayerClients(new List<string>
                { "TestString", "TestString", "TestString" });

            yield return TestPlayerList(playerNames23);

            RemoveNetworkManagerClients(true);

            yield return CloseGameRoom();
        }

        [UnityTest]
        public IEnumerator TestGameRoomStartMatch()
        {
            yield return OpenGameRoom();

            yield return new WaitForSeconds(1f);

            Assert.IsTrue(SceneManager.GetScene("LobbyScene").isLoaded, "LobbyScene is not loaded!");

            Button startMatchButton = GetGameRoomRoot().Q<Button>("StartMatchButton");
            Assert.NotNull(startMatchButton, "StartMatchButton not found!");

            SendClickEvent(startMatchButton);

            yield return new WaitForSeconds(1f);

            Assert.IsFalse(SceneManager.GetScene("LobbyScene").isLoaded, "LobbyScene is still loaded!");

            RemoveNetworkManagerClients(true, "NetworkManager");
        }

        [UnityTest]
        public IEnumerator TestServerExitGameRoom()
        {
            yield return OpenGameRoom();

            yield return new WaitForSeconds(1f);

            yield return StartClientForFishnet();
            yield return new WaitForSeconds(1f);

            Button exitGameRoomButton = GetGameRoomRoot().Q<Button>("ExitButton");
            Assert.NotNull(exitGameRoomButton, "ExitButton not found!");

            SendClickEvent(exitGameRoomButton);

            yield return new WaitForSeconds(1f);

            yield return CheckAllNetworkManagers();

            Assert.IsTrue(CheckForGameObjectActive("LobbyMenu"), "LobbyMenu was not found! It should be active now!");
            Assert.IsFalse(CheckForGameObjectActive("GameRoomView"),
                "GameRoomView was found! It should not be active yet!");

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestIPDisplayInGameRoom()
        {
            yield return OpenGameRoom();

            yield return new WaitForSeconds(1f);

            Label ipSettingsLabel = GetGameRoomRoot().Q<Label>("IPSettings");
            Assert.NotNull(ipSettingsLabel, "IPSettingsLabel not found!");

            string serverIP = InstanceFinder.TransportManager.Transport.GetServerBindAddress(IPAddressType.IPv4);

            Assert.AreEqual(serverIP, ipSettingsLabel.text, "Server IP is not displayed correctly!");

            InstanceFinder.ServerManager.StopConnection(true);
        }

        [UnityTest]
        public IEnumerator TestPasswordDisplayInGameRoom()
        {
            const string password = "TestPassword";

            TextField passwordField = LobbyMenuRoot.Q<TextField>("create-password-text-input");
            Assert.NotNull(passwordField, "PasswordField not found!");
            passwordField.value = password;

            yield return OpenGameRoom();

            yield return new WaitForSeconds(1f);

            Label passwordSettingsLabel = GetGameRoomRoot().Q<Label>("PasswordSettings");
            Assert.NotNull(passwordSettingsLabel, "PasswordSettingsLabel not found!");

            Assert.AreEqual(password, passwordSettingsLabel.text, "Password is not displayed correctly!");

            InstanceFinder.ServerManager.StopConnection(true);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestGameRoomVisualElements()
        {
            yield return OpenGameRoom();

            yield return new WaitForSeconds(0.1f);

            Label titleLabel = GetGameRoomRoot().Q<Label>("TitleLabel");
            CheckIfVisualElementExistsDisplayed(titleLabel);

            VisualElement hostOptions = GetGameRoomRoot().Q<VisualElement>("HostOptions");
            CheckIfVisualElementExistsDisplayed(hostOptions);

            ListView playerList = GetGameRoomRoot().Q<ListView>("PlayerList");
            CheckIfVisualElementExistsDisplayed(playerList);

            Label passwordSettingsLabel = GetGameRoomRoot().Q<Label>("PasswordSettings");
            CheckIfVisualElementExistsDisplayed(passwordSettingsLabel);

            VisualElement passwordContainer = GetGameRoomRoot().Q<VisualElement>("PasswordContainer");
            CheckIfVisualElementExistsDisplayed(passwordContainer);

            VisualElement ipContainer = GetGameRoomRoot().Q<VisualElement>("IPContainer");
            CheckIfVisualElementExistsDisplayed(ipContainer);

            Label ipSettingsLabel = GetGameRoomRoot().Q<Label>("IPSettings");
            CheckIfVisualElementExistsDisplayed(ipSettingsLabel);

            Button startMatchButton = GetGameRoomRoot().Q<Button>("StartMatchButton");
            CheckIfVisualElementExistsDisplayed(startMatchButton);

            Button exitButton = GetGameRoomRoot().Q<Button>("ExitButton");
            CheckIfVisualElementExistsDisplayed(exitButton);

            InstanceFinder.ServerManager.StopConnection(true);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestHostOptions()
        {
            yield return OpenGameRoom();

            yield return new WaitForSeconds(0.1f);

            VisualElement hostOptions = GetGameRoomRoot().Q<VisualElement>("HostOptions");
            CheckIfVisualElementExistsDisplayed(hostOptions);

            Label titleLabelHost = GetGameRoomRoot().Q<Label>("TitleLabel");
            Assert.AreEqual("Game Room (Host)", titleLabelHost.text, "❌ TitleLabel is not correct!");

            GameObject uiDocumentObject = GameObject.Find("GameRoomView");
            GameRoomView gameRoomView = uiDocumentObject.GetComponent<GameRoomView>();
            gameRoomView.SetupGameRoom(false, "TestPassword", "TestIP");

            yield return new WaitForSeconds(0.1f);

            Assert.Zero((int)hostOptions.worldBound.height, "❌ HostOptions is visible! It should not be visible!");
            Assert.Zero((int)hostOptions.worldBound.width, "❌ HostOptions is visible! It should not be visible!");

            Label titleLabelClient = GetGameRoomRoot().Q<Label>("TitleLabel");
            Assert.AreEqual("Game Room (Client)", titleLabelClient.text, "❌ TitleLabel is not correct!");

            InstanceFinder.ServerManager.StopConnection(true);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPlayerList()
        {
            yield return OpenGameRoom();

            yield return new WaitForSeconds(0.1f);

            GameObject uiDocumentObject = GameObject.Find("GameRoomView");
            GameRoomView gameRoomView = uiDocumentObject.GetComponent<GameRoomView>();

            PlayerClient host = new PlayerClient("TestString", 0, true);

            List<PlayerClient> players = gameRoomView.PlayersList;
            // list should contain only the host
            Assert.AreEqual(1, players.Count, "❌ Player list is not correct! It should contain only the host!");
            Assert.IsTrue(players[0].IsHost, "❌ Player list is not correct! Host is not host!");
            Assert.AreEqual(host.PlayerID, players[0].PlayerID,
                "❌ Player list is not correct! Host id is not correct!");
            Assert.AreEqual(host.PlayerName, players[0].PlayerName,
                "❌ Player list is not correct! Host name is not correct!");

            PlayerClient player1 = new("TestString1", 1, false);
            PlayerClient player2 = new("TestString2", 2, false);
            PlayerClient player3 = new("TestString3", 3, false);

            List<PlayerClient> testPlayers = new() { player1, player2 };
            gameRoomView.PlayersList = new List<PlayerClient>(testPlayers);

            players = gameRoomView.PlayersList;
            Assert.IsTrue(ComparePlayerClientLists(testPlayers, players),
                "❌ Player list is not correct! It does not match the test list!");

            gameRoomView.AddPlayerToList(player3);
            players = gameRoomView.PlayersList;
            testPlayers.Add(player3);
            Assert.IsTrue(ComparePlayerClientLists(testPlayers, players),
                "❌ Player list is not correct! It does not match the test list!");

            gameRoomView.RemovePlayerFromListById(1);
            players = gameRoomView.PlayersList;
            testPlayers.RemoveAt(0);
            Assert.IsTrue(ComparePlayerClientLists(testPlayers, players),
                "❌ Player list is not correct! It does not match the test list!");

            yield return TestPlayerList(testPlayers);

            InstanceFinder.ServerManager.StopConnection(true);

            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestFailTest()
        {
            Assert.IsTrue(false);
            yield return null;
        }

        /// <summary>
        /// Compares two lists of player clients
        /// </summary>
        /// <param name="list1">The first list of player clients</param>
        /// <param name="list2">The second list of player clients</param>
        /// <returns>True if the lists are equal, false otherwise</returns>
        private bool ComparePlayerClientLists(List<PlayerClient> list1, List<PlayerClient> list2)
        {
            if (list1 == null || list2 == null || list1.Count != list2.Count) return false;

            return !list1.Where((t, i) => !t.Equals(list2[i])).Any();
        }

        /// <summary>
        /// Opens the game room
        /// </summary>
        /// <returns>An IEnumerator for the Unity Test</returns>
        private IEnumerator OpenGameRoom()
        {
            Button startGameButton = LobbyMenuRoot.Q<Button>("StartGameButton");
            Assert.NotNull(startGameButton, "StartGameButton not found!");

            Assert.IsFalse(CheckForGameObjectActive("GameRoomView"),
                "GameRoomView was found! It should not be active yet!");

            CheckTextField(LobbyMenuRoot.Q<TextField>("PlayerNameTextField"));

            Assert.IsFalse(IsFishNetServerRunning(), "FishNet server is running! It should not be running yet!");

            SendClickEvent(startGameButton);

            yield return new WaitForSeconds(1f);

            Assert.IsTrue(CheckForGameObjectActive("GameRoomView"),
                "GameRoomView was not found! It should be active now!");
            Assert.IsFalse(CheckForGameObjectActive("LobbyMenu"), "LobbyMenu was found! It should not be active yet!");

            Assert.IsTrue(IsFishNetServerRunning(), "FishNet server is not running!");

            yield return null;
        }

        /// <summary>
        /// Tests the player list
        /// </summary>
        /// <param name="playerNames">The names of the players</param>
        /// <returns>An IEnumerator for the Unity Test</returns>
        private IEnumerator TestPlayerList(List<PlayerClient> playerNames)
        {
            VisualElement gameRoomRoot = GetGameRoomRoot();
            Assert.NotNull(gameRoomRoot, "GameRoomRoot not found!");

            ListView playerList = gameRoomRoot.Q<ListView>("PlayerList");
            Assert.NotNull(playerList, "PlayerList not found!");

            Assert.NotNull(playerList.itemsSource, "ItemsSource not found!");

            Assert.Greater(playerList.itemsSource.Count, 0, "PlayerList is empty!");

            Assert.AreEqual(playerNames.Count, playerList.itemsSource.Count,
                "PlayerList has the wrong amount of players!");

            for (int i = 0; i < playerList.itemsSource.Count; i++)
            {
                Assert.AreEqual(playerNames[i], playerList.itemsSource[i], "PlayerList has the wrong player names!");
            }

            List<VisualElement> labels = gameRoomRoot.Query<VisualElement>(className: "unity-list-view__item").ToList();
            Assert.AreEqual(playerNames.Count, labels.Count, "Amount of labels is not correct!");
            for (int i = 0; i < labels.Count; i++)
            {
                Label label2 = labels[i] as Label;
                Assert.IsNotNull(label2, "Label not found!");
                string playerName = playerNames[i].PlayerID + " " + playerNames[i].PlayerName;
                Assert.AreEqual(playerName, label2.text, "Player name is not correct!");
            }

            yield return null;
        }

        /// <summary>
        /// Closes the game room
        /// </summary>
        /// <returns>An IEnumerator for the Unity Test</returns>
        private static IEnumerator CloseGameRoom()
        {
            Button returnToLobbyButton = GetRoot("GameRoomView").Q<Button>("ExitButton");
            Assert.NotNull(returnToLobbyButton, "ExitButton not found!");

            SendClickEvent(returnToLobbyButton);

            Assert.IsTrue(CheckForGameObjectActive("LobbyMenu"), "LobbyMenu was not found! It should be active now!");
            Assert.IsFalse(CheckForGameObjectActive("GameRoomView"),
                "GameRoomView was found! It should not be active yet!");

            yield return new WaitForSeconds(1f);

            Assert.IsFalse(IsFishNetServerRunning(), "FishNet server is running! It should not be running anymore!");

            yield return null;
        }

        /// <summary>
        /// Starts a client for FishNet
        /// </summary>
        /// <param name="specificName">The specific name of the client</param>
        /// <returns>An IEnumerator for the Unity Test</returns>
        private IEnumerator StartClientForFishnet(string specificName = "NetworkManagerClient")
        {
            GameObject networkManagerPrefab = Resources.Load<GameObject>("Prefabs/NetworkManager");
            Assert.IsNotNull(networkManagerPrefab, "NetworkManager prefab not found!");

            GameObject networkManagerObject = Object.Instantiate(networkManagerPrefab);
            Assert.IsNotNull(networkManagerObject, "NetworkManager object not found!");
            networkManagerObject.name = specificName;

            NetworkManager networkManager = networkManagerObject.GetComponent<NetworkManager>();
            Assert.IsNotNull(networkManager, "NetworkManager not found!");

            string ipAddress = InstanceFinder.TransportManager.Transport.GetServerBindAddress(IPAddressType.IPv4);
            networkManager.ClientManager.StartConnection(ipAddress);

            yield return new WaitUntil(
                () => networkManager.ClientManager.Started,
                new TimeSpan(0, 0, 10),
                () => Assert.IsTrue(networkManager.ClientManager.Started, "Client started timeout")
            );
        }

        /// <summary>
        /// Removes the network manager clients
        /// </summary>
        /// <param name="removeAll">True if all clients should be removed, false if only one should be removed</param>
        /// <param name="specificName">The specific name of the client, if no specific name is given, the default name is used</param>
        private void RemoveNetworkManagerClients(bool removeAll, string specificName = "NetworkManagerClient")
        {
            NetworkManager[] networkManagerObjects =
                Object.FindObjectsByType<NetworkManager>(FindObjectsSortMode.None);
            foreach (NetworkManager networkManagerObject in networkManagerObjects)
            {
                if (networkManagerObject.name != specificName)
                {
                    continue;
                }

                Object.Destroy(networkManagerObject.gameObject);
                if (!removeAll)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the root of a UI document
        /// </summary>
        /// <param name="uiDocumentName">The name of the UI document</param>
        /// <returns>The root of the UI document</returns>
        private static VisualElement GetRoot(string uiDocumentName)
        {
            GameObject uiDocumentObject = GameObject.Find(uiDocumentName);
            UIDocument uiDocumentSub = uiDocumentObject.GetComponent<UIDocument>();
            if (uiDocumentSub != null)
            {
                return uiDocumentSub.rootVisualElement;
            }

            Debug.LogError("UI Document not found!");
            return null;
        }

        /// <summary>
        /// Checks if a game object is active
        /// </summary>
        /// <param name="gameObjectName">The name of the game object</param>
        /// <returns> True if the game object is active, false otherwise</returns>
        private static bool CheckForGameObjectActive(string gameObjectName)
        {
            GameObject gameObject = GameObject.Find(gameObjectName);
            return gameObject != null && gameObject.activeSelf;
        }

        /// <summary>
        /// Checks if the FishNet server is running
        /// </summary>
        /// <returns> True if the FishNet server is running, false otherwise</returns>
        private static bool IsFishNetServerRunning()
        {
            NetworkManager networkManager = InstanceFinder.NetworkManager;
            Assert.IsNotNull(networkManager, "NetworkManager not found!");
            return networkManager.ServerManager.AnyServerStarted();
        }

        /// <summary>
        /// Sends a click event to a button
        /// </summary>
        /// <param name="button">The button to send the click event to</param>
        private static void SendClickEvent(Button button)
        {
            ClickEvent clickEvent = new();
            clickEvent.target = button;
            button.SendEvent(clickEvent);
        }

        /// <summary>
        /// Builds a list of player clients
        /// </summary>
        /// <param name="playerNames"> The names of the players</param>
        /// <returns> A list of player clients</returns>
        private List<PlayerClient> BuildPlayerClients(List<string> playerNames)
        {
            List<PlayerClient> playerClients = playerNames
                .Select((playerName, idCounter) => new PlayerClient(playerName, idCounter, true))
                .ToList();
            return playerClients;
        }

        /// <summary>
        /// Checks all network managers if the server or client are running
        /// </summary>
        /// <returns>An IEnumerator for the Unity Test</returns>
        private IEnumerator CheckAllNetworkManagers()
        {
            NetworkManager[] networkManagerObjects =
                Object.FindObjectsByType<NetworkManager>(FindObjectsSortMode.None);
            foreach (NetworkManager networkManagerObject in networkManagerObjects)
            {
                Assert.IsFalse(networkManagerObject.ServerManager.AnyServerStarted(), "Server is running!");
                Assert.IsFalse(networkManagerObject.ClientManager.Started, "Client is running!");
            }

            yield return null;
        }
    }
}