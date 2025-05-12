using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FortressForge.UI.CustomVisualElements;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Tests.GameOverlay
{
    [TestFixture]
    public class GameOverlayTest
    {
        private const string OVERLAY_SCENE_NAME = "GameOverlayTesting";
        private GameObject _document;
        private VisualElement _root;
        private TabView _tabView;

        private const string BUILDINGS_TAB_VIEW = "buildings-tab";
        private const string WEAPONS_TAB_VIEW = "weapons-tab";
        private const string DEFENSE_TAB_VIEW = "defense-tab";
        private const string FACTORY_TAB_VIEW = "factory-tab";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Load the scene containing the GameOverlay
            UnityEngine.SceneManagement.SceneManager.LoadScene(OVERLAY_SCENE_NAME);
            yield return new WaitUntil(
                () => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == OVERLAY_SCENE_NAME,
                new TimeSpan(0, 0, 10),
                () => Assert.AreEqual(OVERLAY_SCENE_NAME, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    "Failed to load GameOverlay scene within the timeout period.")
            );

            GameObject document = GameObject.Find("BuildingOverlay");
            Assert.IsNotNull(document, "UIDocument not found in the scene.");

            // Get the root VisualElement of the UIDocument
            _root = document.GetComponent<UIDocument>().rootVisualElement;
            Assert.IsNotNull(_root, "Root VisualElement not found in the scene.");

            // Check if the TabView exists in the scene
            _tabView = _root.Q<TabView>();
            Assert.IsNotNull(_tabView, "TabView not found in the scene.");

            Debug.Log("SetUp complete.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestTopTrapezFrameExists()
        {
            // Check if the TrapezView exists in the scene
            VisualElement topTrapezFrame = _root.Q<VisualElement>(className: "top-trapez-frame");
            Assert.IsNotNull(topTrapezFrame, "Top Trapez Frame not found in the scene.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestRessourceViewExists()
        {
            // get the TemplateContainer
            VisualElement resourceView = _root.Q<VisualElement>(className: "ressource-root");
            Assert.IsNotNull(resourceView, "Resource View not found in the scene.");
            
            // check for magma container
            VisualElement planetView = _root.Q<VisualElement>(className: "root-frame");
            Assert.IsNotNull(planetView, "Planet View not found in the scene.");

            // check if there are two RessourceContainers
            List<VisualElement> resourceContainersLeft = resourceView.Query<VisualElement>("ressource-container-left").Children<VisualElement>().ToList();
            Assert.IsNotNull(resourceContainersLeft, "Resource Containers not found in the scene.");
            Assert.AreEqual(2, resourceContainersLeft.Count, "Resource Containers count is not 2.");

            resourceContainersLeft.ForEach(resourceContainer =>
            {
                List<VisualElement> resourceContainersChildren = resourceContainer.Children().ToList();
                Assert.IsNotNull(resourceContainersChildren, "Resource Containers not found in the scene.");
                FillableRessourceContainer fillableRessourceContainer = resourceContainersChildren[1].Q<FillableRessourceContainer>();
                Assert.IsNotNull(fillableRessourceContainer, "Resource Container is not a FillableRessourceContainer.");
                Assert.IsTrue(fillableRessourceContainer.ClassListContains("ressource-container-fillable"), "Resource Container is not a FillableRessourceContainer.");
            });

            List<VisualElement> resourceContainersRight = resourceView.Query<VisualElement>("ressource-container-left").Children<VisualElement>().ToList();
            Assert.IsNotNull(resourceContainersRight, "Resource Containers not found in the scene.");
            Assert.AreEqual(2, resourceContainersRight.Count, "Resource Containers count is not 2.");

            resourceContainersRight.ForEach(resourceContainer =>
            {
                List<VisualElement> resourceContainersChildren = resourceContainer.Children().ToList();
                Assert.IsNotNull(resourceContainersChildren, "Resource Containers not found in the scene.");
                FillableRessourceContainer fillableRessourceContainer = resourceContainersChildren[1].Q<FillableRessourceContainer>();
                Assert.IsNotNull(fillableRessourceContainer, "Resource Container is not a FillableRessourceContainer.");
                Assert.IsTrue(fillableRessourceContainer.ClassListContains("ressource-container-fillable"), "Resource Container is not a FillableRessourceContainer.");
            });

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestBottomTrapezFrameExists()
        {
            // Check if the TrapezView exists in the scene
            VisualElement bottomTrapezFrame = _root.Q<VisualElement>(className: "bottom-trapez-frame");
            Assert.IsNotNull(bottomTrapezFrame, "Bottom Trapez Frame not found in the scene.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestBuildingSelectorViewExists()
        {
            // Check if the BuildingSelectorView exists in the scene
            VisualElement buildingSelectorView = _root.Q<VisualElement>(className: "building-selector-view");
            Assert.IsNotNull(buildingSelectorView, "Building Selector View not found in the scene.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestBuildingSelectorViewTabHeadersExists()
        {
            // Check if the BuildingSelectorView exists in the scene
            VisualElement buildingSelectorView = _root.Q<VisualElement>(className: "building-selector-view");
            Assert.IsNotNull(buildingSelectorView, "Building Selector View not found in the scene.");

            // Check if the Tab Headers exist
            VisualElement tabHeaders = buildingSelectorView.Q<TemplateContainer>()?.Q<VisualElement>("building-selector-root")?.Q<TabView>(className: "unity-tab-view")
                ?.Q<VisualElement>("unity-tab-view__header-container");
            Assert.IsNotNull(tabHeaders, "Tab Headers not found in the scene.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestTabViewChange()
        {
            yield return SetUp(); // Somehow this is needed to run here again because if not the test fails when running ith the other tests(it passes when running alone)
            
            // Check if the TabView exists in the scene
            TabView tabView = _root.Q<TabView>();
            Assert.IsNotNull(tabView, "TabView not found in the scene.");

            // Check if the Tab Headers exist
            VisualElement tabHeaders = tabView.Q<VisualElement>("unity-tab-view__header-container");
            Assert.IsNotNull(tabHeaders, "Tab Headers not found in the scene.");

            // Check if the Tab Views exist
            List<VisualElement> tabViews = tabHeaders.Children().ToList();
            Assert.IsNotNull(tabViews, "Tab Views not found in the scene.");
            Assert.AreEqual(4, tabViews.Count, "Tab Views count is not 4.");

            CheckTabViewVisibility(BUILDINGS_TAB_VIEW, true);
            CheckTabViewVisibility(WEAPONS_TAB_VIEW, false);
            CheckTabViewVisibility(DEFENSE_TAB_VIEW, false);
            CheckTabViewVisibility(FACTORY_TAB_VIEW, false);

            // Change the active tab to WEAPONS_TAB_VIEW
            yield return ChangeActiveTab(WEAPONS_TAB_VIEW);
            CheckTabViewVisibility(BUILDINGS_TAB_VIEW, false);
            CheckTabViewVisibility(WEAPONS_TAB_VIEW, true);
            CheckTabViewVisibility(DEFENSE_TAB_VIEW, false);
            CheckTabViewVisibility(FACTORY_TAB_VIEW, false);

            // Change the active tab to DEFENSE_TAB_VIEW
            yield return ChangeActiveTab(DEFENSE_TAB_VIEW);
            CheckTabViewVisibility(BUILDINGS_TAB_VIEW, false);
            CheckTabViewVisibility(WEAPONS_TAB_VIEW, false);
            CheckTabViewVisibility(DEFENSE_TAB_VIEW, true);
            CheckTabViewVisibility(FACTORY_TAB_VIEW, false);
            // Change the active tab to FACTORY_TAB_VIEW
            yield return ChangeActiveTab(FACTORY_TAB_VIEW);
            CheckTabViewVisibility(BUILDINGS_TAB_VIEW, false);
            CheckTabViewVisibility(WEAPONS_TAB_VIEW, false);
            CheckTabViewVisibility(DEFENSE_TAB_VIEW, false);
            CheckTabViewVisibility(FACTORY_TAB_VIEW, true);

            // Change the active tab to BUILDINGS_TAB_VIEW
            yield return ChangeActiveTab(BUILDINGS_TAB_VIEW);
            CheckTabViewVisibility(BUILDINGS_TAB_VIEW, true);
            CheckTabViewVisibility(WEAPONS_TAB_VIEW, false);
            CheckTabViewVisibility(DEFENSE_TAB_VIEW, false);
            CheckTabViewVisibility(FACTORY_TAB_VIEW, false);
            yield return null;
        }

        /// <summary>
        /// Checks if a TabView is visible.
        /// </summary>
        /// <param name="tabName">The name of the TabView.</param>
        /// <param name="shouldBeVisible">True if the TabView should be visible, false otherwise.</param>
        private void CheckTabViewVisibility(string tabName, bool shouldBeVisible)
        {
            VisualElement tabView = _root.Q(tabName);
            Assert.NotNull(tabView, $"{tabName} not found!");
            DisplayStyle displayStyle = shouldBeVisible ? DisplayStyle.Flex : DisplayStyle.None;
            string errorMessage = shouldBeVisible
                ? $"{tabName} is not visible!"
                : $"{tabName} is visible but should not be!";
            Assert.IsTrue(tabView.resolvedStyle.display == displayStyle, errorMessage);
        }

        /// <summary>
        /// Changes the active tab of the TabView.
        /// </summary>
        /// <param name="tabString">The name of the tab to activate.</param>
        private IEnumerator ChangeActiveTab(string tabString)
        {
            VisualElement contentContainer = _root.Q("unity-tab-view__content-container");
            Assert.NotNull(contentContainer, "ContentContainer not found!");
            Tab tab = contentContainer.Q<Tab>(tabString);
            Assert.NotNull(tab, $"{tabString} not found!");
            _tabView.activeTab = tab;

            yield return new WaitUntil(
                () => tab.resolvedStyle.display == DisplayStyle.Flex,
                new TimeSpan(0, 0, 10),
                () => Assert.AreEqual(tab.resolvedStyle.display, DisplayStyle.Flex,
                    $"Failed to set the active tab to {tabString} within the timeout period.")
            );
        }
    }
}