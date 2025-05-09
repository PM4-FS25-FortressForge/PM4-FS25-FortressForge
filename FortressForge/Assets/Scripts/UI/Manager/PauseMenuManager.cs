using FortressForge.Network;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI.Manager
{
    /// <summary>
    /// Manages the pause menu UI, including toggling its visibility and handling button interactions.
    /// </summary>
    public class PauseMenuManager : MonoBehaviour
    {
        public UIDocument PauseMenuUIDocument;

        private VisualElement _pauseMenuRoot;
        private Button _exitGameButton;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePauseMenu();
            }
        }

        public void OnEnable()
        {
            if (PauseMenuUIDocument == null)
            {
                Debug.LogWarning("PauseMenuUIDocument is not assigned.");
                return;
            }

            _pauseMenuRoot = PauseMenuUIDocument.rootVisualElement;
            if (_pauseMenuRoot == null)
            {
                Debug.LogWarning("PauseMenuRoot is not found in the UIDocument.");
                return;
            }

            _pauseMenuRoot.style.display = DisplayStyle.None;

            _exitGameButton = _pauseMenuRoot.Q<Button>("exit-game-button");
            if (_exitGameButton == null)
            {
                Debug.LogWarning("Exit game button not found in the pause menu.");
                return;
            }

            RemoteConnectionHandler remoteConnectionHandler = FindFirstObjectByType<RemoteConnectionHandler>();
            _exitGameButton.RegisterCallback<ClickEvent>(ev => remoteConnectionHandler.LeaveServer());
        }

        /// <summary>
        /// Toggles the pause menu visibility.
        /// </summary>
        private void TogglePauseMenu()
        {
            _pauseMenuRoot.style.display = _pauseMenuRoot.style.display == DisplayStyle.Flex
                ? DisplayStyle.None
                : DisplayStyle.Flex;
        }
    }
}