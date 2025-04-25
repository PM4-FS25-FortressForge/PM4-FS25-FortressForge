using System.Collections.Generic;
using System.Linq;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.UI.CustomVisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
    /// <summary>
    /// Generates the bottom trapez view for the building GameOverlay
    /// </summary>
    public class BottomOverlayViewGenerator : MonoBehaviour
    {
        public UIDocument overlayUIDocument;
        private VisualElement _bottomFrame;
        private TrapezElement _trapezElement;
        public VisualTreeAsset BuildingSelectorViewTree;
        public VisualTreeAsset BuildingCardVisualTree;

        private BuildViewController _buildViewController;
        private List<BaseBuildingTemplate> _availableBuildings;
        private VisualElement _selectedCard;

        private void OnEnable()
        {
            if (!ValidateUIDocument()) return;

            InitializeOverlayFrame();
            InitializeTrapezFrame();
            _bottomFrame.Add(_trapezElement);

            LoadBuildingSelectorView();
            AlignTabHeadersWithTrapezBorder();
        }

        /// <summary>
        /// Initializes the BottomTrapezViewGenerator with the available buildings and BuildViewController.
        /// </summary>
        /// <param name="availableBuildings"> List of available buildings.</param>
        /// <param name="buildViewController"> The BuildViewController to initialize with.</param>
        public void Init(List<BaseBuildingTemplate> availableBuildings, BuildViewController buildViewController)
        {
            if (availableBuildings == null || availableBuildings.Count == 0 || buildViewController == null)
            {
                Debug.LogError("No available buildings or BuildViewController provided.");
                return;
            }

            _availableBuildings = availableBuildings;
            _buildViewController = buildViewController;

            PopulateTabViewContentContainers();
        }

        /// <summary>
        /// Selects a building based on the index and triggers the preview in the BuildViewController.
        /// </summary>
        /// <param name="index"></param>
        private void SelectBuilding(int index)
        {
            if (index >= _availableBuildings.Count)
            {
                Debug.LogError("Index out of range.");
                return;
            }

            _buildViewController.PreviewSelectedBuilding(index);
        }

        /// <summary>
        /// Validates if the overlay UIDocument is assigned.
        /// </summary>
        /// <returns>True if the UIDocument is valid, false otherwise.</returns>
        private bool ValidateUIDocument()
        {
            if (overlayUIDocument != null) return true;

            Debug.LogError("Overlay UIDocument is not assigned.");
            return false;
        }

        /// <summary>
        /// Initializes the overlay frame by finding the bottom frame element.
        /// </summary>
        private void InitializeOverlayFrame()
        {
            _bottomFrame = overlayUIDocument.rootVisualElement.Q<VisualElement>("root-frame")?.Q<VisualElement>("bottom-view");

            if (_bottomFrame == null)
                Debug.LogError("Bottom frame is missing.");
        }

        /// <summary>
        /// Initializes the trapez frame by creating a new TrapezElement and setting its parameters.
        /// </summary>
        private void InitializeTrapezFrame()
        {
            _trapezElement = new TrapezElement();
            _trapezElement.SetParameters(90f, 0f, "trapez-frame");
            _trapezElement.AddToClassList("bottom-trapez-frame");
        }

        /// <summary>
        /// Loads the building selector view by cloning the visual tree asset and adding it to the trapez element.
        /// </summary>
        private void LoadBuildingSelectorView()
        {
            if (BuildingSelectorViewTree == null)
            {
                Debug.LogError("Building selector view tree is missing.");
                return;
            }

            VisualElement buildingSelectorView = BuildingSelectorViewTree.CloneTree();
            buildingSelectorView.AddToClassList("building-selector-view");
            _trapezElement.Add(buildingSelectorView);
        }

        /// <summary>
        /// Aligns the tab headers with the trapez border by setting their margins.
        /// </summary>
        private void AlignTabHeadersWithTrapezBorder()
        {
            VisualElement tabHeaders = _trapezElement.Q<TemplateContainer>()?
                .Q<VisualElement>("building-selector-root")?
                .Q<TabView>(className: "unity-tab-view")?
                .Q<VisualElement>("unity-tab-view__header-container");

            if (tabHeaders == null)
            {
                Debug.LogError("Tab headers not found.");
                return;
            }

            List<int> tabHeaderMargins = new() { 40, 29, 16, 4 };
            List<VisualElement> tabHeaderList = tabHeaders.Children().ToList();

            for (int i = 0; i < tabHeaderList.Count && i < tabHeaderMargins.Count; i++)
                tabHeaderList[i].style.marginLeft = Length.Percent(tabHeaderMargins[i]);
        }

        /// <summary>
        /// Populates the tab view content containers with building cards.
        /// </summary>
        private void PopulateTabViewContentContainers()
        {
            IEnumerable<VisualElement> tabContentList = _trapezElement.Q<TemplateContainer>()?
                .Q<VisualElement>("building-selector-root")?
                .Q<TabView>(className: "unity-tab-view")?
                .Q<VisualElement>("unity-tab-view__content-container")?
                .Children();

            if (tabContentList == null)
            {
                Debug.LogError("Tab content containers not found.");
                return;
            }

            foreach (VisualElement tabContent in tabContentList)
            {
                VisualElement tabContentListView =
                    tabContent.Q<VisualElement>("unity-tab__content-container")?.Q<ScrollView>();

                if (tabContentListView == null)
                {
                    Debug.LogError("ListView not found in tab content.");
                    continue;
                }

                tabContentListView.Clear();
                foreach (BaseBuildingTemplate building in _availableBuildings)
                {
                    VisualElement buildingCard = MakeItem();
                    BindItem(buildingCard, _availableBuildings.IndexOf(building));
                    tabContentListView.Add(buildingCard);
                }
            }
        }

        /// <summary>
        /// Creates a new building card item.
        /// </summary>
        /// <returns></returns>
        private VisualElement MakeItem()
        {
            VisualElement buildingCard = BuildingCardVisualTree.CloneTree();
            return buildingCard;
        }

        /// <summary>
        /// Binds the data to the building card item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        private void BindItem(VisualElement item, int index)
        {
            if (index < 0 || index >= _availableBuildings.Count) return;

            BaseBuildingTemplate building = _availableBuildings[index];

            item.AddToClassList("building-card-template-container");

            VisualElement cardFrame = item.Q<VisualElement>("building-card-frame");
            VisualElement labelsContainer = cardFrame?.Q<VisualElement>("building-card-label-container");

            if (labelsContainer == null)
            {
                Debug.LogError("Labels container not found.");
                return;
            }

            labelsContainer.Q<Label>("building-card-name").text = building.name;
            labelsContainer.Q<Label>("building-card-cost").text = building.name;

            item.RegisterCallback<PointerDownEvent>(_ =>
            {
                _selectedCard?.RemoveFromClassList("selected-building-card");

                _selectedCard = item;
                _selectedCard.AddToClassList("selected-building-card");

                SelectBuilding(index);
                Debug.Log($"Selected building: {building.name}");
            });
        }
    }
}