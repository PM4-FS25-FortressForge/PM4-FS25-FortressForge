using System;
using System.Collections;
using System.Linq;
using FortressForge.BuildingSystem.Weapons;
using FortressForge.Enums;
using FortressForge.HexGrid.Data;
using FortressForge.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Tests.GameOverlay
{
    [TestFixture]
    public class FightSystemOverlayGeneratorTests
    {
        private const string OVERLAY_SCENE_NAME = "GameOverlayTesting";
        private VisualElement _fightSystemOverlayRoot;
        private FightSystemOverlayGenerator _fightSystemOverlayGenerator;
        
        private GameViewStateMachine _gameViewStateMachine;

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
            _fightSystemOverlayRoot = Array.Find(uiDocuments, doc => doc.name == "FightSystemOverlay")?.rootVisualElement;
            Assert.IsNotNull(_fightSystemOverlayRoot, "FightSystemOverlay root visual element not found in the scene.");

            _fightSystemOverlayGenerator = UnityEngine.Object.FindFirstObjectByType<FightSystemOverlayGenerator>();
            Assert.IsNotNull(_fightSystemOverlayGenerator, "FightSystemOverlayGenerator component not found in the scene.");

            _gameViewStateMachine = UnityEngine.Object.FindFirstObjectByType<GameViewStateMachine>();
            Assert.IsNotNull(_gameViewStateMachine, "GameViewStateMachine component not found in the scene.");
            _gameViewStateMachine.SetOverlayState(GameOverlayState.FightingSystemOverlay);
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator Overlay_IsVisible()
        {
            yield return SetUp();
            Assert.AreEqual(DisplayStyle.Flex, _fightSystemOverlayRoot.resolvedStyle.display, "FightSystemOverlay should be visible after setup.");
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TabHeaders_ArePresent_And_HaveTitles()
        {
            yield return SetUp();
            var tabHeaders = _fightSystemOverlayRoot.Query(className: "unity-tab-view__header-container").Children<VisualElement>().ToList();
            Assert.IsTrue(tabHeaders.Count > 0, "No TabHeaders found.");
            foreach (var header in tabHeaders)
            {
                var label = header.Q<Label>();
                Assert.IsNotNull(label, "TabHeader label missing.");
                Assert.IsFalse(string.IsNullOrEmpty(label.text), "TabHeader label text is empty.");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator TabContent_Visibility_Changes_OnTabSwitch()
        {
            yield return SetUp();
            var tabView = _fightSystemOverlayRoot.Query<TabView>(className: "unity-tab-view").First();
            Assert.IsNotNull(tabView, "TabView not found.");

            var tabs = tabView.Query<Tab>().ToList();
            Assert.IsTrue(tabs.Count > 1, "Not enough tabs for switching.");

            foreach (var tab in tabs)
            {
                tabView.activeTab = tab;
                yield return null;
                foreach (var t in tabs)
                {
                    var expected = t == tab ? DisplayStyle.Flex : DisplayStyle.None;
                    Assert.AreEqual(expected, t.resolvedStyle.display, $"Tab {t.name} visibility incorrect after switching.");
                }
            }
        }

        [UnityTest]
        public IEnumerator WeaponLabels_ArePresent_And_Selectable()
        {
            yield return SetUp();
            SpawnWeaponBuildingPrefab();
            var weaponLabels = _fightSystemOverlayGenerator
                .GetType()
                .GetField("_weaponLabels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_fightSystemOverlayGenerator) as IEnumerable;

            Assert.IsNotNull(weaponLabels, "Weapon labels list is null.");
            var labelList = weaponLabels.Cast<Label>().ToList();
            Assert.IsTrue(labelList.Count > 0, "No weapon labels found.");

            foreach (var label in labelList)
            {
                ClickEvent clickEvent = new ();
                clickEvent.target = label;
                label.SendEvent(clickEvent);
                yield return null;
                Assert.IsTrue(label.ClassListContains("selected-weapon-label"), "Weapon label was not selected after click.");
            }
        }

        [UnityTest]
        public IEnumerator OrientationButtons()
        {
            yield return SetUp();
            var orientationButtons = _fightSystemOverlayRoot.Query<VisualElement>().Where(e => e.name == "arrow-up" || e.name == "arrow-down" || e.name == "arrow-left" || e.name == "arrow-right")
                .ToList();
            Assert.IsTrue(orientationButtons.Count == 4, "Not all orientation buttons found.");
        }

        /// <summary>
        /// Spawns the weapon building prefab and registers it with the WeaponBuildingManager.
        /// </summary>
        private void SpawnWeaponBuildingPrefab()
        {
            GameObject weaponPrefab = Resources.Load<GameObject>("Prefabs/Tier_1/Tier_1_Konzept");
            Assert.IsNotNull(weaponPrefab, "Weapon prefab not found in Resources.");
            GameObject weaponInstance = UnityEngine.Object.Instantiate(weaponPrefab);
            Assert.IsNotNull(weaponInstance, "Weapon prefab instance could not be created.");
            
            WeaponInputHandler weaponInputHandler = weaponInstance.GetComponent<WeaponInputHandler>();
            Assert.IsNotNull(weaponInputHandler, "WeaponInputHandler component not found on the weapon prefab.");
            WeaponBuildingManager.Instance?.RegisterWeaponBuilding(weaponInputHandler);
        }

        [UnityTest]
        public IEnumerator AmmunitionButtons_And_FireButton_ArePresent_And_Interactable()
        {
            yield return SetUp();
            var ammoButtons = new[] { "ammo-type-1", "ammo-type-2", "ammo-type-3", "ammo-type-4" };
            foreach (var btnName in ammoButtons)
            {
                var btn = _fightSystemOverlayRoot.Q<Button>(btnName);
                Assert.IsNotNull(btn, $"Ammunition button {btnName} not found.");
                yield return null;
            }
        
            var fireButton = _fightSystemOverlayRoot.Q<Button>("fire-button");
            Assert.IsNotNull(fireButton, "Fire button not found.");
            yield return null;
        }
    }
}