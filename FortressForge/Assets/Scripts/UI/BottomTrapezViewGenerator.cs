using System.Collections.Generic;
using System.Linq;
using FortressForge.UI.CustomVisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
    public class BottomTrapezViewGenerator : MonoBehaviour
    {
        public UIDocument overlayUIDocument;
        private VisualElement _bottomFrame;
        private TrapezElement _trapezElement;
        public VisualTreeAsset buildingSelectorViewTree;
        public VisualTreeAsset buildingCardVisualTree;

        private readonly Dictionary<string, string> _exampleBuildings = new();

        private void OnEnable()
        {
            if (!ValidateUIDocument()) return;

            InitializeOverlayFrame();
            InitializeTrapezFrame();
            _bottomFrame.Add(_trapezElement);

            LoadBuildingSelectorView();
            AlignTabHeadersWithTrapezBorder();
            GenerateBuildingList();
            PopulateTabViewContentContainers();
        }

        private bool ValidateUIDocument()
        {
            if (overlayUIDocument != null) return true;

            Debug.LogError("Overlay UIDocument is not assigned.");
            return false;
        }

        private void InitializeOverlayFrame()
        {
            _bottomFrame = overlayUIDocument.rootVisualElement.Q<VisualElement>("root-frame")?.Q<VisualElement>("bottom-view");

            if (_bottomFrame == null)
                Debug.LogError("Bottom frame is missing.");
        }

        private void InitializeTrapezFrame()
        {
            _trapezElement = new TrapezElement();
            _trapezElement.SetParameters(90f, 0f, "trapez-frame");
            _trapezElement.AddToClassList("bottom-trapez-frame");
        }

        private void LoadBuildingSelectorView()
        {
            if (buildingSelectorViewTree == null)
            {
                Debug.LogError("Building selector view tree is missing.");
                return;
            }

            VisualElement buildingSelectorView = buildingSelectorViewTree.CloneTree();
            buildingSelectorView.AddToClassList("building-selector-view");
            _trapezElement.Add(buildingSelectorView);
        }

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
                _exampleBuildings
                    .Select((_, index) =>
                    {
                        VisualElement buildingCard = MakeItem();
                        BindItem(buildingCard, index);
                        return buildingCard;
                    })
                    .ToList()
                    .ForEach(buildingCard => tabContentListView.Add(buildingCard));
            }
        }

        private void GenerateBuildingList()
        {
            _exampleBuildings.Clear();
            for (int i = 0; i < 40; i++)
            {
                _exampleBuildings.Add($"Building {i}", $"Description for Building {i}");
            }
        }

        private VisualElement MakeItem()
        {
            VisualElement buildingCard = buildingCardVisualTree.CloneTree();
            buildingCard.RegisterCallback<PointerDownEvent>(_ => Debug.Log("Building card clicked!"));
            return buildingCard;
        }

        private void BindItem(VisualElement item, int index)
        {
            if (!_exampleBuildings.TryGetValue(_exampleBuildings.ElementAt(index).Key, out string description))
                return;

            item.AddToClassList("building-card-template-container");

            VisualElement cardFrame = item.Q<VisualElement>("building-card-frame");
            VisualElement labelsContainer = cardFrame?.Q<VisualElement>("building-card-label-container");

            if (labelsContainer == null)
            {
                Debug.LogError("Labels container not found.");
                return;
            }

            labelsContainer.Q<Label>("building-card-name").text = _exampleBuildings.ElementAt(index).Key;
            labelsContainer.Q<Label>("building-card-cost").text = description;
        }
    }
}