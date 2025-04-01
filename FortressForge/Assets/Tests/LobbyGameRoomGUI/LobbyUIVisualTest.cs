using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Tests.LobbyGameRoomGUI
{
    [TestFixture]
    public class LobbyUIVisualTest : LobbyGameRoomBaseSetup
    {
        private const string CREATE_TAB_VIEW = "Create-Tab";
        private const string JOIN_TAB_VIEW = "Join-Tab";
        private const string IP_JOIN_TAB_VIEW = "IP-Join";


        [UnitySetUp]
        public override IEnumerator SetUp()
        {
            yield return base.SetUp();
        }

        [UnityTest]
        public IEnumerator CheckUIElements()
        {
            Assert.NotNull(LobbyMenuRoot, "❌ LobbyMenuRoot is null!");
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerNameInputFieldTest()
        {
            CheckTextField(LobbyMenuRoot.Q<TextField>("PlayerNameTextField"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateServerPasswortInputFieldTest()
        {
            CheckTextField(LobbyMenuRoot.Q<TextField>("create-password-text-input"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateServerMaxPlayersSliderInputTest()
        {
            SliderInt createServerMaxPlayersSlider = LobbyMenuRoot.Q<SliderInt>("max-player-amount");
            Assert.NotNull(createServerMaxPlayersSlider, "❌ CreateServerMaxPlayersSlider not found!");

            createServerMaxPlayersSlider.value = 4;
            Assert.AreEqual(4, createServerMaxPlayersSlider.value,
                "❌ CreateServerMaxPlayersSlider value is not correct!");

            createServerMaxPlayersSlider.value = 10000;
            Assert.AreEqual(10, createServerMaxPlayersSlider.value,
                "❌ CreateServerMaxPlayersSlider value is not correct!");


            createServerMaxPlayersSlider.value = -10000;
            Assert.AreEqual(2, createServerMaxPlayersSlider.value,
                "❌ CreateServerMaxPlayersSlider value is not correct!");

            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckTabViewChange()
        {
            CheckTabViewVisibility(CREATE_TAB_VIEW, true);
            CheckTabViewVisibility(JOIN_TAB_VIEW, false);
            CheckTabViewVisibility(IP_JOIN_TAB_VIEW, false);

            ChangeActiveTab(JOIN_TAB_VIEW);

            yield return new WaitForSeconds(0.01f);

            CheckTabViewVisibility(JOIN_TAB_VIEW, true);
            CheckTabViewVisibility(CREATE_TAB_VIEW, false);
            CheckTabViewVisibility(IP_JOIN_TAB_VIEW, false);

            ChangeActiveTab(IP_JOIN_TAB_VIEW);

            yield return new WaitForSeconds(0.01f);

            CheckTabViewVisibility(IP_JOIN_TAB_VIEW, true);
            CheckTabViewVisibility(CREATE_TAB_VIEW, false);
            CheckTabViewVisibility(JOIN_TAB_VIEW, false);

            yield return null;
        }

        /// <summary>
        /// Checks if a TabView is visible.
        /// </summary>
        /// <param name="tabName">The name of the TabView.</param>
        /// <param name="shouldBeVisible">True if the TabView should be visible, false otherwise.</param>
        private void CheckTabViewVisibility(string tabName, bool shouldBeVisible)
        {
            VisualElement tabView = LobbyMenuRoot.Q(tabName);
            Assert.NotNull(tabView, $"❌ {tabName} not found!");
            DisplayStyle displayStyle = shouldBeVisible ? DisplayStyle.Flex : DisplayStyle.None;
            string errorMessage = shouldBeVisible
                ? $"❌ {tabName} is not visible!"
                : $"❌ {tabName} is visible but should not be!";
            Assert.IsTrue(tabView.resolvedStyle.display == displayStyle, errorMessage);
        }

        /// <summary>
        /// Changes the active tab of the TabView.
        /// </summary>
        /// <param name="tabString">The name of the tab to activate.</param>
        private void ChangeActiveTab(string tabString)
        {
            TabView tabView = LobbyMenuRoot.Q<TabView>();
            Assert.NotNull(tabView, "❌ TabView not found!");

            VisualElement contentContainer = LobbyMenuRoot.Q("unity-tab-view__content-container");
            Assert.NotNull(contentContainer, "❌ ContentContainer not found!");
            Tab tab = contentContainer.Q<Tab>(tabString);
            Assert.NotNull(tab, $"❌ {tabString} not found!");
            tabView.activeTab = tab;
        }
    }
}