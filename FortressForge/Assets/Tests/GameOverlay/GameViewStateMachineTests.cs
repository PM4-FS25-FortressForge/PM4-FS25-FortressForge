using System;
using System.Collections;
using FortressForge.Enums;
using FortressForge.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Tests.GameOverlay
{
    [TestFixture]
    public class GameViewStateMachineTests
    {
        private const string OVERLAY_SCENE_NAME = "GameOverlayTesting";
        private VisualElement _fightSystemOverlayRoot;
        private VisualElement _buildingOverlayRoot;

        private GameViewStateMachine _stateMachine;

        private IEnumerator SetUp()
        {
            SceneManager.LoadScene(OVERLAY_SCENE_NAME);
            yield return new WaitUntil(
                () => SceneManager.GetActiveScene().name == OVERLAY_SCENE_NAME,
                new TimeSpan(0, 0, 10),
                () => Assert.AreEqual(OVERLAY_SCENE_NAME, SceneManager.GetActiveScene().name,
                    "Failed to load GameOverlay scene within the timeout period.")
            );

            UIDocument[] uiDocuments = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            _buildingOverlayRoot = Array.Find(uiDocuments, doc => doc.name == "BuildingOverlay")?.rootVisualElement;
            Assert.IsNotNull(_buildingOverlayRoot, "BuildingOverlay root visual element not found in the scene.");

            _fightSystemOverlayRoot = Array.Find(uiDocuments, doc => doc.name == "FightSystemOverlay")?.rootVisualElement;
            Assert.IsNotNull(_fightSystemOverlayRoot, "FightSystemOverlay root visual element not found in the scene.");

            _stateMachine = UnityEngine.Object.FindFirstObjectByType<GameViewStateMachine>();
            Assert.IsNotNull(_stateMachine, "GameViewStateMachine component not found in the scene.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetOverlayState_InitialState()
        {
            yield return SetUp();
            Assert.IsTrue(_fightSystemOverlayRoot.style.display == DisplayStyle.None, "FightSystemOverlay should not be active initially.");
            Assert.IsTrue(_buildingOverlayRoot.style.display == DisplayStyle.Flex, "BuildingOverlay should not be active initially.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SetOverlayState_FightingSystemOverlay()
        {
            yield return SetUp();
            _stateMachine.SetOverlayState(GameOverlayState.FightingSystemOverlay);
            Assert.IsTrue(_fightSystemOverlayRoot.style.display == DisplayStyle.Flex, "FightingSystemOverlay is not active.");
            Assert.IsTrue(_buildingOverlayRoot.style.display == DisplayStyle.None, "BuildingOverlay should not be active when FightingSystemOverlay is active.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SetOverlayState()
        {
            yield return SetUp();
            _stateMachine.SetOverlayState(GameOverlayState.BuildingOverlay);
            Assert.IsTrue(_buildingOverlayRoot.style.display == DisplayStyle.Flex, "BuildingOverlay is not active.");
            Assert.IsTrue(_fightSystemOverlayRoot.style.display == DisplayStyle.None, "FightingSystemOverlay should not be active when BuildingOverlay is active.");
            yield return null;
        }
    }
}