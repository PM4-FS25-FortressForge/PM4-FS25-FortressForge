using System;
using FortressForge.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
    /// <summary>
    /// A state machine for managing the game view overlays.
    /// </summary>
    public class GameViewStateMachine : MonoBehaviour
    {
        public BottomOverlayViewGenerator BottomOverlayViewGenerator;
        public FightSystemOverlayGenerator FightSystemOverlayGenerator;

        private GameOverlayState _currentState;

        public static event Action OnSpacePressed;

        private void OnEnable()
        {
            _currentState = GameOverlayState.BuildingOverlay;

            UpdateOverlay();

            OnSpacePressed += ToggleOverlay;
        }

        private void OnDisable()
        {
            OnSpacePressed -= ToggleOverlay;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnSpacePressed?.Invoke();
            }
        }

        /// <summary>
        /// Toggles the overlay between BuildingOverlay and FightingSystemOverlay.
        /// </summary>
        private void ToggleOverlay()
        {
            _currentState = _currentState == GameOverlayState.BuildingOverlay
                ? GameOverlayState.FightingSystemOverlay
                : GameOverlayState.BuildingOverlay;

            UpdateOverlay();
        }

        /// <summary>
        /// Sets the overlay state to the specified state.
        /// </summary>
        /// <param name="state">The state to set.</param>
        public void SetOverlayState(GameOverlayState state)
        {
            _currentState = state;
            UpdateOverlay();
        }

        /// <summary>
        /// Updates the overlay based on the current state.
        /// </summary>
        private void UpdateOverlay()
        {
            if (BottomOverlayViewGenerator?.overlayUIDocument is null || FightSystemOverlayGenerator?.OverlayUIDocument is null)
            {
                Debug.LogWarning("Overlay UIDocuments are not set.");
                return;
            }

            switch (_currentState)
            {
                default:
                case GameOverlayState.BuildingOverlay:
                    BottomOverlayViewGenerator.overlayUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                    FightSystemOverlayGenerator.OverlayUIDocument.rootVisualElement.style.display = DisplayStyle.None;
                    break;

                case GameOverlayState.FightingSystemOverlay:
                    FightSystemOverlayGenerator.OverlayUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                    BottomOverlayViewGenerator.overlayUIDocument.rootVisualElement.style.display = DisplayStyle.None;

                    break;
            }
        }
    }
}