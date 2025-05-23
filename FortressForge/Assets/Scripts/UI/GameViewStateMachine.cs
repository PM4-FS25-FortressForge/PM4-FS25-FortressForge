using System;
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

        private GameState _currentState;

        public static event Action OnSpacePressed;

        private enum GameState
        {
            BuildingOverlay,
            FightingSystemOverlay
        }

        private void OnEnable()
        {
            _currentState = GameState.BuildingOverlay;

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
            Debug.Log("Toggle overlay");
            _currentState = _currentState == GameState.BuildingOverlay
                ? GameState.FightingSystemOverlay
                : GameState.BuildingOverlay;

            UpdateOverlay();
        }

        /// <summary>
        /// Updates the overlay based on the current state.
        /// </summary>
        private void UpdateOverlay()
        {
            switch (_currentState)
            {
                default:
                case GameState.BuildingOverlay:
                    if (BottomOverlayViewGenerator is not null)
                    {
                        BottomOverlayViewGenerator.overlayUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                        FightSystemOverlayGenerator.OverlayUIDocument.rootVisualElement.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        Debug.LogError("BuildingOverlayTree is not assigned.");
                    }

                    break;

                case GameState.FightingSystemOverlay:
                    if (FightSystemOverlayGenerator is not null)
                    {
                        FightSystemOverlayGenerator.OverlayUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                        BottomOverlayViewGenerator.overlayUIDocument.rootVisualElement.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        Debug.LogError("FightingSystemOverlayTree is not assigned.");
                    }

                    break;
            }
        }
    }
}