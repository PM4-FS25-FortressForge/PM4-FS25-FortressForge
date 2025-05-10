using System.Linq;
using FortressForge.UI.CustomVisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
    /// <summary>
    /// A class that checks if a click is on the overlay.
    /// </summary>
    public class UIClickChecker
    {
        private static UIClickChecker _instance;
        public static UIClickChecker Instance => _instance ??= new UIClickChecker();

        private TrapezElement _topTrapezOverlay;
        private TrapezElement _bottomTrapezOverlay;

        private VisualElement _pauseMenuRoot;

        private UIClickChecker()
        {
        }

        /// <summary>
        /// Resets the instance and overlays.
        /// </summary>
        public static void ResetInstance()
        {
            _instance = null;
        }

        /// <summary>
        /// Ensures the overlays are initialized for the current scene.
        /// </summary>
        private void InitializeOverlays()
        {
            UIDocument[] uiDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            UIDocument uiDocument = uiDocuments.FirstOrDefault(document => document.name == "BuildingOverlay");
            UIDocument pauseMenuDocument = uiDocuments.FirstOrDefault(document => document.name == "PauseMenu");

            if (uiDocument is null || pauseMenuDocument is null)

            {
                _topTrapezOverlay = null;
                _bottomTrapezOverlay = null;
                _pauseMenuRoot = null;
                return;
            }

            _pauseMenuRoot = pauseMenuDocument.rootVisualElement;

            _topTrapezOverlay = uiDocument.rootVisualElement.Q<TrapezElement>(className: "top-trapez-frame");
            _bottomTrapezOverlay = uiDocument.rootVisualElement.Q<TrapezElement>(className: "bottom-trapez-frame");

            if (_topTrapezOverlay == null || _bottomTrapezOverlay == null || _pauseMenuRoot == null)
                Debug.Log("TrapezOverlay or PauseContainer not found!");
        }

        /// <summary>
        /// Checks if the mouse is on the overlay.
        /// </summary>
        /// <returns>True if the mouse is on the overlay, false otherwise.</returns>
        public bool IsMouseOnOverlay()
        {
            if (_topTrapezOverlay == null || _bottomTrapezOverlay == null || _pauseMenuRoot == null)
            {
                InitializeOverlays();
            }

            if (_pauseMenuRoot is not null && _pauseMenuRoot.resolvedStyle.display == DisplayStyle.Flex)
            {
                return true;
            }

            return _topTrapezOverlay?.IsPointInTrapez() == true || _bottomTrapezOverlay?.IsPointInTrapez() == true;
        }
    }
}