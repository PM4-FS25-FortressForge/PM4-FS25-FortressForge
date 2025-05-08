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

            if (uiDocument is null)
            {
                _topTrapezOverlay = null;
                _bottomTrapezOverlay = null;
                return;
            }

            _topTrapezOverlay = uiDocument.rootVisualElement.Q<TrapezElement>(className: "top-trapez-frame");
            _bottomTrapezOverlay = uiDocument.rootVisualElement.Q<TrapezElement>(className: "bottom-trapez-frame");

            if (_topTrapezOverlay == null || _bottomTrapezOverlay == null)
                Debug.Log("TrapezOverlay not found!");
        }

        /// <summary>
        /// Checks if the mouse is on the overlay.
        /// </summary>
        /// <returns>True if the mouse is on the overlay, false otherwise.</returns>
        public bool IsMouseOnOverlay()
        {
            if (_topTrapezOverlay == null || _bottomTrapezOverlay == null)
            {
                InitializeOverlays();
            }

            return _topTrapezOverlay?.IsPointInTrapez() == true || _bottomTrapezOverlay?.IsPointInTrapez() == true;
        }
    }
}