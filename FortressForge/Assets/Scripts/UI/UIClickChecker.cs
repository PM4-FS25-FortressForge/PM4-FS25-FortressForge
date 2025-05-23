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
        private VisualElement _bottomTrapezRoot;
        private TrapezElement _fightSystemOverlay;
        private VisualElement _fightSystemOverlayRoot;

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
            UIDocument uiDocument = uiDocuments?.FirstOrDefault(document => document.name == "BuildingOverlay");
            UIDocument topOverlayUiDocument = uiDocuments?.FirstOrDefault(document => document.name == "TopOverlay");
            UIDocument fightSystemOverlayUiDocument = uiDocuments?.FirstOrDefault(document => document.name == "FightSystemOverlay");
            UIDocument pauseMenuDocument = uiDocuments?.FirstOrDefault(document => document.name == "PauseMenu");

            _pauseMenuRoot = pauseMenuDocument?.rootVisualElement;

            _topTrapezOverlay = topOverlayUiDocument?.rootVisualElement.Q<TrapezElement>(className: "top-trapez-frame");
            _bottomTrapezRoot = uiDocument?.rootVisualElement;
            _bottomTrapezOverlay = _bottomTrapezRoot?.Q<TrapezElement>(className: "bottom-trapez-frame");

            _fightSystemOverlayRoot = fightSystemOverlayUiDocument?.rootVisualElement;
            _fightSystemOverlay = _fightSystemOverlayRoot?.Q<TrapezElement>(className: "bottom-weapons-trapez-frame");
        }

        /// <summary>
        /// Checks if the mouse is on the overlay.
        /// </summary>
        /// <returns>True if the mouse is on the overlay, false otherwise.</returns>
        public bool IsMouseOnOverlay()
        {
            if (_topTrapezOverlay == null || _bottomTrapezOverlay == null || _pauseMenuRoot == null || _fightSystemOverlay == null)
            {
                InitializeOverlays();
            }

            if (_pauseMenuRoot is not null && _pauseMenuRoot.resolvedStyle.display == DisplayStyle.Flex)
            {
                return true;
            }

            if (_bottomTrapezOverlay != null && _bottomTrapezRoot != null && _bottomTrapezRoot.resolvedStyle.display == DisplayStyle.Flex)
            {
                return _bottomTrapezOverlay?.IsPointInTrapez() == true;
            }

            if (_fightSystemOverlay != null && _fightSystemOverlayRoot != null && _fightSystemOverlayRoot.resolvedStyle.display == DisplayStyle.Flex)
            {
                return _fightSystemOverlay?.IsPointInTrapez() == true;
            }

            return _topTrapezOverlay?.IsPointInTrapez() == true;
        }
    }
}