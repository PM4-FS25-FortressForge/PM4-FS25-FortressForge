using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using FishNet;
using FishNet.Transporting;
using FortressForge.Network;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace FortressForge.UI.Manager
{
    /// <summary>
    /// Manages the two Lobby views; the lobby view and the game room view
    /// </summary>
    public class ViewManager : MonoBehaviour
    {
        [SerializeField] private UIDocument lobbyViewDoc;
        [SerializeField] private UIDocument gameRoomViewDoc;
        [SerializeField] private string lobbyScene = "LobbyScene";

        private string _nextScene = "GameView";

        public string NextScene
        {
            get => _nextScene;
            set => _nextScene = value;
        }

        private UIDocument _currentView;
        private GameRoomView _gameRoomView;

        private Button _startGameButton;
        private Button _joinGameButton;
        private Button _returnToLobbyButton;
        private Button _startMatchButton;

        private string _currentPlayerName = "";

        private const float SHAKE_STRENGTH = 5f;

        private string _serverIP = "127.0.0.1";

        private PlayerClient _playerClient;

        private void OnEnable()
        {
            gameRoomViewDoc.gameObject.SetActive(false);

            ShowView(lobbyViewDoc);
            SetupLobbyButtons();

            _gameRoomView = FindFirstObjectByType<GameRoomView>();

            PopulateTextFieldsForFasterDevelopment();
        }

        public void OnDisable()
        {
            UnregisterCallbacks();
        }

        /// <summary>
        /// Show the specified view and set up the buttons
        /// </summary>
        /// <param name="newView">The view to show</param>
        private void ShowView(UIDocument newView)
        {
            if (_currentView != null)
            {
                _currentView.gameObject.SetActive(false);
            }

            newView.gameObject.SetActive(true);
            _currentView = newView;

            if (newView == lobbyViewDoc)
            {
                RestorePlayerName();
                SetupLobbyButtons();
            }
            else if (newView == gameRoomViewDoc)
            {
                SetupGameRoomButtons();
            }
        }

        /// <summary>
        ///  Set up the buttons in the lobby view
        /// </summary>
        private void SetupLobbyButtons()
        {
            VisualElement lobbyRoot = lobbyViewDoc.rootVisualElement;

            _startGameButton = lobbyRoot.Q<Button>("StartGameButton");
            _joinGameButton = lobbyRoot.Q<Button>("JoinGameButton");

            _startGameButton?.RegisterCallback((ClickEvent _) => StartGame());

            _joinGameButton?.RegisterCallback((ClickEvent _) => JoinGame());
        }

        /// <summary>
        ///  Set up the buttons in the game room view
        /// </summary>
        private void SetupGameRoomButtons()
        {
            VisualElement gameRoomRoot = gameRoomViewDoc.rootVisualElement;
            _returnToLobbyButton = gameRoomRoot.Q<Button>("ExitButton");

            _returnToLobbyButton?.RegisterCallback((ClickEvent _) => ReturnToLobby());

            _startMatchButton = gameRoomRoot.Q<Button>("StartMatchButton");
            _startMatchButton?.RegisterCallback((ClickEvent _) => StartMatch());
        }

        /// <summary>
        ///  Create a new game room
        /// </summary>
        private void StartGame()
        {
            if (IsPortInUse())
            {
                Debug.LogError("Port 7777 ist bereits in Benutzung!");
                ShowConnectionError();
                return;
            }

            Debug.Log("🎮 Spiel wird gestartet!");
            string playerName = GetPlayerName();
            string password = GetPassword();

            if (!ValidatePlayerName(playerName)) return;

            _playerClient = new PlayerClient(playerName, null, true);
            StartServer();

            EnterGameRoom(true, password, _serverIP);
        }

        /// <summary>
        /// Check if the port is already in use
        /// </summary>
        /// <returns>True if the port is in use, false otherwise</returns>
        private static bool IsPortInUse()
        {
            int portToCheck = InstanceFinder.TransportManager.Transport.GetPort();
            try
            {
                using UdpClient udpClient = new(new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), portToCheck));
                return false;
            }
            catch (SocketException)
            {
                return true;
            }
        }

        /// <summary>
        /// Join an existing game room
        /// </summary>
        private void JoinGame()
        {
            string playerName = GetPlayerName();
            if (!ValidateJoinIPGameFields()) return;

            _playerClient = new PlayerClient(playerName);

            TextField ipField = lobbyViewDoc.rootVisualElement.Q<TextField>("ip-join-text-input");
            TextField passwordField = lobbyViewDoc.rootVisualElement.Q<TextField>("create-password-text-input");
            string enteredIP = ipField?.value ?? "";
            string enteredPassword = passwordField?.value ?? "";
            JoinServer(enteredIP);
        }

        /// <summary>
        /// Start the server
        /// </summary>
        private void StartServer()
        {
            _serverIP = GetLocalIPAddress();
            InstanceFinder.TransportManager.Transport.SetServerBindAddress(_serverIP, IPAddressType.IPv4);
            try
            {
                InstanceFinder.ServerManager.StartConnection();
                Debug.Log("ServerManager StartConnection: ");
            }
            catch (Exception e)
            {
                Debug.LogError("❌ Server konnte nicht gestartet werden: " + e.Message);
                ShowConnectionError();
                return;
            }

            InstanceFinder.ClientManager.StartConnection(_serverIP);
            Debug.Log($"✅ Server gestartet auf: {_serverIP}:{InstanceFinder.TransportManager.Transport.GetPort()}");
        }

        /// <summary>
        /// Join the server with the given IP
        /// </summary>
        /// <param name="ip">The IP address of the server in the IPV4 format</param>
        private void JoinServer(string ip)
        {
            ShowLoadingAnimation(true);
            InstanceFinder.ClientManager.StartConnection(ip);
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnected;
        }

        /// <summary>
        /// Event when the client is connected to the server
        /// </summary>
        /// <param name="args">The connection state arguments</param>
        private void OnClientConnected(ClientConnectionStateArgs args)
        {
            ShowLoadingAnimation(false);
            Debug.Log("🔍 Verbindung zu Server wird versucht: " + args.ConnectionState);
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                Debug.Log("✅ Verbindung erfolgreich!");
                EnterGameRoom(false, _currentPlayerName);
            }
            else
            {
                Debug.LogError("❌ Verbindung fehlgeschlagen! Stelle sicher, dass die IP-Adresse korrekt ist.");
                ShowConnectionError();
            }

            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnected;

            InstanceFinder.ClientManager.OnClientConnectionState += OnServerEndedConnection;
        }

        /// <summary>
        /// Event when the server ended the connection
        /// </summary>
        /// <param name="args">The connection state arguments</param>
        private void OnServerEndedConnection(ClientConnectionStateArgs args)
        {
            InstanceFinder.ClientManager.OnClientConnectionState -= OnServerEndedConnection;
            HashSet<LocalConnectionState> stoppedStates = new()
            {
                LocalConnectionState.StoppedError,
                LocalConnectionState.StoppedClosed,
                LocalConnectionState.Stopped,
                LocalConnectionState.Stopping
            };

            if (!stoppedStates.Contains(args.ConnectionState)) return;

            Debug.Log("❌ Verbindung zum Server verloren! ClientConnection");
            ShowView(lobbyViewDoc);
        }

        /// <summary>
        /// Show a visual error when the connection to the server failed for a specified duration
        /// </summary>
        /// <param name="duration">The duration in milliseconds to show the error</param>
        private void ShowConnectionError(int duration = 1000)
        {
            TextField ipField = lobbyViewDoc.rootVisualElement.Q<TextField>("ip-join-text-input");

            if (ipField != null)
            {
                ipField.style.borderBottomColor = Color.red;
                ipField.style.borderLeftColor = Color.red;
                ipField.style.borderRightColor = Color.red;
                ipField.style.borderTopColor = Color.red;

                ipField.schedule.Execute(() =>
                {
                    ipField.style.borderBottomColor = Color.white;
                    ipField.style.borderLeftColor = Color.white;
                    ipField.style.borderRightColor = Color.white;
                    ipField.style.borderTopColor = Color.white;
                }).StartingIn(duration);
            }

            ShakeLobbyContainer();
        }


        /// <summary>
        /// Start the match and load the next scene
        /// </summary>
        private void StartMatch()
        {
            BootstrapSceneManager sceneManager = FindAnyObjectByType<BootstrapSceneManager>();
            sceneManager.LoadScene(_nextScene);
        }

        /// <summary>
        /// Get the player name from the lobby view input field and return it
        /// </summary>
        /// <returns>The player name</returns>
        private string GetPlayerName()
        {
            TextField playerNameField = lobbyViewDoc.rootVisualElement.Q<TextField>("PlayerNameTextField");

            _currentPlayerName = playerNameField == null ? "" : GetStringFromInputField(playerNameField);
            return _currentPlayerName;
        }

        /// <summary>
        ///  Get the string from the given input field
        /// </summary>
        /// <param name="inputField">The input field to get the string from</param>
        /// <returns>The string from the input field</returns>
        private static string GetStringFromInputField(TextField inputField)
        {
            return inputField?.value.Trim();
        }

        /// <summary>
        /// Show the game room view, the permissions depend on the host status of the player set in the parameter
        /// </summary>
        /// <param name="isHost">true if the player is the host, false if the player is a client</param>
        /// <param name="password">The password for the game room</param>
        /// <param name="serverIP">The IP of the server</param>
        private void EnterGameRoom(bool isHost, string password = "", string serverIP = "")
        {
            ShowView(gameRoomViewDoc);

            if (_gameRoomView == null)
            {
                _gameRoomView = gameRoomViewDoc.GetComponent<GameRoomView>();
                if (_gameRoomView == null)
                {
                    Debug.LogError("GameRoomView konnte nicht gefunden werden!");
                    return;
                }
            }

            _gameRoomView.SetupGameRoom(isHost, password, serverIP);
        }

        /// <summary>
        /// Return to the lobby view
        /// </summary>
        private void ReturnToLobby()
        {
            if (InstanceFinder.IsServerStarted)
            {
                InstanceFinder.ServerManager.StopConnection(true);
                if (InstanceFinder.IsClientStarted)
                {
                    InstanceFinder.ClientManager.StopConnection();
                }
            }
            else
            {
                InstanceFinder.ClientManager.StopConnection();
            }

            ShowView(lobbyViewDoc);
        }

        /// <summary>
        /// Restore the player name in the player name input field
        /// </summary>
        private void RestorePlayerName()
        {
            TextField playerNameField = lobbyViewDoc.rootVisualElement.Q<TextField>("PlayerNameTextField");

            if (playerNameField != null)
            {
                playerNameField.value = _currentPlayerName;
            }
        }

        /// <summary>
        /// Shake the lobby container visually to indicate an error
        /// </summary>
        private void ShakeLobbyContainer()
        {
            VisualElement lobbyContainer = lobbyViewDoc.rootVisualElement.Q<VisualElement>("Lobby_Transparent_Back");

            if (lobbyContainer == null) return;

            Vector3 originalPosition = lobbyContainer.transform.position;

            lobbyContainer.schedule.Execute(() => { lobbyContainer.transform.position = originalPosition + new Vector3(SHAKE_STRENGTH, 0, 0); }).StartingIn(50);

            lobbyContainer.schedule.Execute(() => { lobbyContainer.transform.position = originalPosition - new Vector3(SHAKE_STRENGTH, 0, 0); }).StartingIn(100);

            lobbyContainer.schedule.Execute(() => { lobbyContainer.transform.position = originalPosition + new Vector3(SHAKE_STRENGTH / 2, 0, 0); }).StartingIn(150);

            lobbyContainer.schedule.Execute(() => { lobbyContainer.transform.position = originalPosition; })
                .StartingIn(200);
        }

        /// <summary>
        /// Validate the player name and return true if the player name is valid, false otherwise
        /// </summary>
        /// <param name="playerName">The player name to validate</param>
        /// <returns>True if the player name is valid, false otherwise</returns>
        private bool ValidatePlayerName(string playerName)
        {
            if (!string.IsNullOrEmpty(playerName)) return true;
            MarkInputFieldAsInvalid(lobbyViewDoc.rootVisualElement.Q<TextField>("PlayerNameTextField"));
            ShakeLobbyContainer();
            return false;
        }

        /// <summary>
        /// Mark the input field as invalid by changing the border color to red and changing it back after the first change event
        /// </summary>
        /// <param name="inputField">The input field to mark as invalid</param>
        private static void MarkInputFieldAsInvalid(TextField inputField)
        {
            if (inputField == null) return;

            IStyle textFieldStyle = inputField.Q("unity-text-input").style;

            textFieldStyle.borderBottomColor = new StyleColor(Color.red);
            textFieldStyle.borderLeftColor = new StyleColor(Color.red);
            textFieldStyle.borderRightColor = new StyleColor(Color.red);
            textFieldStyle.borderTopColor = new StyleColor(Color.red);

            textFieldStyle.borderBottomWidth = 2;
            textFieldStyle.borderLeftWidth = 2;
            textFieldStyle.borderRightWidth = 2;
            textFieldStyle.borderTopWidth = 2;

            textFieldStyle.backgroundColor = new StyleColor(new Color(1, 0, 0, 0.1f));

            inputField.RegisterCallbackOnce<ChangeEvent<string>>(_ =>
            {
                textFieldStyle.borderBottomColor = new StyleColor(Color.white);
                textFieldStyle.borderLeftColor = new StyleColor(Color.white);
                textFieldStyle.borderRightColor = new StyleColor(Color.white);
                textFieldStyle.borderTopColor = new StyleColor(Color.white);

                textFieldStyle.borderBottomWidth = 1;
                textFieldStyle.borderLeftWidth = 1;
                textFieldStyle.borderRightWidth = 1;
                textFieldStyle.borderTopWidth = 1;

                textFieldStyle.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.1f));
            });
        }

        /// <summary>
        /// Validate all the necessary fields to join a game
        /// </summary>
        /// <returns>True if all fields are valid, false if at least one field is invalid</returns>
        private bool ValidateJoinIPGameFields()
        {
            string playerName = GetPlayerName();
            return ValidatePlayerName(playerName) && CheckIPAddressField();
        }

        /// <summary>
        /// Check the IP address field
        /// </summary>
        /// <returns>True if the IP address in the IP address field is valid, false otherwise</returns>
        private bool CheckIPAddressField()
        {
            TextField ipAddressField = lobbyViewDoc.rootVisualElement.Q<TextField>("ip-join-text-input");
            string ipAddress = GetStringFromInputField(ipAddressField);

            if (ValidateIPAddress(ipAddress)) return true;

            MarkInputFieldAsInvalid(ipAddressField);
            ShakeLobbyContainer();
            return false;
        }

        /// <summary>
        /// Validate the IP address and return true if the IP address is valid, false otherwise
        /// Checks if the IP Address is a IPv4 address
        /// </summary>
        /// <param name="ipAddress">The IP address to validate</param>
        /// <returns>True if the IP address is valid, false otherwise</returns>
        private bool ValidateIPAddress(string ipAddress)
        {
            return !string.IsNullOrWhiteSpace(ipAddress) &&
                   ipAddress.Split('.').Length == 4 &&
                   ipAddress.Split('.').All(r => byte.TryParse(r, out _));
        }

        /// <summary>
        /// Get the password from the password field in the lobby view inside the create-game tabview
        /// </summary>
        /// <returns></returns>
        private string GetPassword()
        {
            TextField passwordField = lobbyViewDoc.rootVisualElement.Q<TextField>("create-password-text-input");
            return GetStringFromInputField(passwordField);
        }

        /// <summary>
        /// Get the local IP address of the player
        /// </summary>
        /// <returns>The local IP address of the player in the IPV4 format</returns>
        private static string GetLocalIPAddress()
        {
            string localIP = "127.0.0.1";

            foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily != AddressFamily.InterNetwork) continue;
                localIP = ip.ToString();
                break;
            }

            return localIP;
        }

        /// <summary>
        /// Display a loading animation gif
        /// </summary>
        /// <param name="show">true to show the loading animation, false to hide it</param>
        private void ShowLoadingAnimation(bool show)
        {
            VisualElement loadingSpinner = lobbyViewDoc.rootVisualElement.Q<VisualElement>("LoadingSpinner");

            if (loadingSpinner != null)
            {
                loadingSpinner.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>
        /// Get the player client
        /// </summary>
        /// <returns>A PlayerClient object of the Player</returns>
        public PlayerClient GetPlayerClient()
        {
            return _playerClient;
        }

        /// <summary>
        /// Populate the text fields with test data for faster development
        /// This is only for development purposes and should be removed in production
        /// </summary>
        private void PopulateTextFieldsForFasterDevelopment()
        {
            TextField playerNameField = lobbyViewDoc.rootVisualElement.Q<TextField>("PlayerNameTextField");
            if (playerNameField != null)
            {
                playerNameField.value = "TestPlayer" + Random.Range(0, 1000);
            }

            TextField ipField = lobbyViewDoc.rootVisualElement.Q<TextField>("ip-join-text-input");
            if (ipField != null)
            {
                ipField.value = GetLocalIPAddress();
            }
        }

        /// <summary>
        /// Unregister all callbacks to avoid memory leaks
        /// </summary>
        private void UnregisterCallbacks()
        {
            try
            {
                InstanceFinder.ClientManager.OnClientConnectionState -= OnServerEndedConnection;
                _startGameButton?.UnregisterCallback((ClickEvent _) => StartGame());
                _joinGameButton?.UnregisterCallback((ClickEvent _) => JoinGame());
                _returnToLobbyButton?.UnregisterCallback((ClickEvent _) => ReturnToLobby());
                _startMatchButton?.UnregisterCallback((ClickEvent _) => StartMatch());
            }
            catch (Exception e)
            {
                Debug.Log($"Error unregistering callbacks: {e.Message}");
            }
        }
    }
}