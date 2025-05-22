using System.Collections.Generic;
using System.Linq;
using FortressForge.Enums;
using FortressForge.UI.CustomVisualElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
    /// <summary>
    /// Generates the overlay for the fight system.
    /// </summary>
    public class FightSystemOverlayGenerator : MonoBehaviour
    {
        public UIDocument OverlayUIDocument;
        public VisualTreeAsset OrientationButtonTree;
        public VisualTreeAsset AmmunitionSelectorButtonTree;
        private VisualElement _bottomFrame;
        private TrapezElement _trapezElement;

        private Label _selectedWeaponLabel;
        private readonly List<Label> _weaponLabels = new();
        private int _selectedWeaponIndex = -1;
        private InputAction _tabAction;
        
        public Texture2D AmmoType1Icon;
        public Texture2D AmmoType2Icon;
        public Texture2D AmmoType3Icon;
        public Texture2D AmmoType4Icon;
        public Texture2D FireIcon;


        public VisualTreeAsset WeaponSelectorViewTree;

        private Dictionary<WeaponClasses, List<string>> _weaponBuildings = new()
        {
            { WeaponClasses.Artillery, new List<string>() },
            { WeaponClasses.Rockets, new List<string>() },
            { WeaponClasses.Lasers, new List<string>() },
        };

        private void OnEnable()
        {
            PopulateWeaponsListForDevelopment(); // Todo: Remove this method in production
            if (!ValidateUIDocument()) return;

            InitializeOverlayFrame();
            InitializeTrapezFrame();
            _bottomFrame.Add(_trapezElement);

            LoadWeaponSelectorView();
            AlignTabHeadersWithTrapezBorder();
            AddTabHeaderTitles();
            RegisterTabInput();
            PopulateTabView();

            InitOrientationButtons();
            InitAmmunitionSelectorButtons();
        }

        private void OnDisable()
        {
            _tabAction?.Disable();
        }

        /// <summary>
        /// Registers the input action for the Tab key to select the next weapon label.
        /// </summary>
        private void RegisterTabInput()
        {
            _tabAction = new InputAction("Tab", binding: "<Keyboard>/tab");
            _tabAction.performed += _ => SelectNextWeaponLabel();
            _tabAction.Enable();
        }

        /// <summary>
        /// Validates if the overlay UIDocument is assigned.
        /// </summary>
        /// <returns>True if the UIDocument is valid, false otherwise.</returns>
        private bool ValidateUIDocument()
        {
            if (OverlayUIDocument != null) return true;

            Debug.LogError("Overlay UIDocument is not assigned.");
            return false;
        }

        /// <summary>
        /// Initializes the overlay frame by finding the bottom frame element.
        /// </summary>
        private void InitializeOverlayFrame()
        {
            _bottomFrame = OverlayUIDocument.rootVisualElement.Q<VisualElement>("root-frame")?.Q<VisualElement>("bottom-view");

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
            _trapezElement.AddToClassList("bottom-weapons-trapez-frame");
        }

        /// <summary>
        /// Aligns the tab headers with the trapez border by setting their margins.
        /// </summary>
        private void AlignTabHeadersWithTrapezBorder()
        {
            List<VisualElement> tabHeaderList = GetTabHeaders();
            if (tabHeaderList.Count == 0) return;

            const float maxMargin = 53f;
            const float minMargin = 13f;
            int count = tabHeaderList.Count;

            for (int i = 0; i < count; i++)
            {
                float margin = count > 1
                    ? Mathf.Lerp(maxMargin, minMargin, i / (float)(count - 1))
                    : maxMargin;
                tabHeaderList[i].style.marginLeft = Length.Percent(margin);
            }
        }

        /// <summary>
        /// Gets the tab headers from the trapez element.
        /// </summary>
        /// <returns>A list of VisualElements representing the tab headers.</returns>
        private List<VisualElement> GetTabHeaders()
        {
            VisualElement tabHeaders = _trapezElement.Q<TemplateContainer>()?
                .Q<VisualElement>("weapon-selector-root")?
                .Q<TabView>(className: "unity-tab-view")?
                .Q<VisualElement>("unity-tab-view__header-container");

            if (tabHeaders == null)
            {
                Debug.LogError("Tab headers not found.");
                return new List<VisualElement>();
            }

            List<VisualElement> tabHeaderList = tabHeaders.Children().ToList();
            return tabHeaderList;
        }

        /// <summary>
        /// Loads the weapon selector view by cloning the visual tree asset and adding it to the trapez element.
        /// </summary>
        private void LoadWeaponSelectorView()
        {
            if (WeaponSelectorViewTree == null)
            {
                Debug.LogError("Weapon Selector View Tree is not assigned.");
                return;
            }

            VisualElement weaponSelectorView = WeaponSelectorViewTree.CloneTree();
            weaponSelectorView.AddToClassList("weapon-selector-view");
            _trapezElement.Add(weaponSelectorView);
        }

        /// <summary>
        /// Adds a building name to the list of weapon buildings for a specific weapon class.
        /// </summary>
        /// <param name="weaponClass">The weapon class to which the building belongs.</param>
        /// <param name="buildingName">The name of the building to add.</param>
        public void AddToWeaponBuildingsList(WeaponClasses weaponClass, string buildingName)
        {
            _weaponBuildings ??= new Dictionary<WeaponClasses, List<string>>();

            if (!_weaponBuildings.ContainsKey(weaponClass))
                _weaponBuildings[weaponClass] = new List<string>();

            if (!_weaponBuildings[weaponClass].Contains(buildingName))
                _weaponBuildings[weaponClass].Add(buildingName);
        }

        /// <summary>
        /// Populates the tab view with weapon labels based on the weapon buildings dictionary.
        /// </summary>
        private void PopulateTabView()
        {
            _weaponLabels.Clear();

            IEnumerable<VisualElement> tabContentList = _trapezElement.Q<TemplateContainer>()?
                .Q<VisualElement>("weapon-selector-root")?
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
                if (!System.Enum.TryParse(tabContent.name, out WeaponClasses weaponClass))
                    continue;

                if (!_weaponBuildings.TryGetValue(weaponClass, out var buildingList))
                    continue;

                VisualElement tabContentListView = tabContent.Q<VisualElement>("unity-tab__content-container")?.Q<ScrollView>();
                if (tabContentListView == null)
                {
                    Debug.LogError("Tab content list view not found.");
                    continue;
                }

                const float maxMargin = 20f;
                const float minMargin = 0f;
                int labelCount = buildingList.Count;

                for (int i = 0; i < labelCount; i++)
                {
                    float margin = labelCount > 1
                        ? Mathf.Lerp(maxMargin, minMargin, i / (float)(labelCount - 1))
                        : maxMargin;

                    Label weaponLabel = new()
                    {
                        text = buildingList[i],
                        name = buildingList[i],
                        style = { marginLeft = Length.Percent(margin) }
                    };
                    weaponLabel.AddToClassList("weapon-label");
                    AddOnClickToWeaponLabels(weaponLabel);
                    tabContentListView.Add(weaponLabel);
                    _weaponLabels.Add(weaponLabel);
                }
            }
        }

        /// <summary>
        /// Adds a click event to the weapon label to select it.
        /// </summary>
        /// <param name="label">The label to which the click event will be added.</param>
        private void AddOnClickToWeaponLabels(Label label)
        {
            label.RegisterCallback<ClickEvent>(_ => SelectWeaponLabel(label));
        }

        private void SelectWeaponLabel(Label label)
        {
            _selectedWeaponLabel?.RemoveFromClassList("selected-weapon-label");
            _selectedWeaponLabel = label;
            _selectedWeaponLabel.AddToClassList("selected-weapon-label");
            _selectedWeaponIndex = _weaponLabels.IndexOf(label);

            TabView tabView = _trapezElement.Q<TabView>(className: "unity-tab-view");
            if (tabView == null) return;

            Tab tabContent = label.GetFirstAncestorOfType<Tab>();
            if (tabContent != null)
                tabView.activeTab = tabContent;
        }

        /// <summary>
        /// Selects the next weapon label in the list.
        /// </summary>
        private void SelectNextWeaponLabel()
        {
            if (_weaponLabels.Count == 0 || _bottomFrame.parent.parent.resolvedStyle.display == DisplayStyle.None) return;
            _selectedWeaponIndex = (_selectedWeaponIndex + 1) % _weaponLabels.Count;
            SelectWeaponLabel(_weaponLabels[_selectedWeaponIndex]);
        }

        /// <summary>
        /// Adds titles to the tab headers based on the weapon classes.
        /// </summary>
        private void AddTabHeaderTitles()
        {
            List<VisualElement> tabHeaderList = GetTabHeaders();
            List<VisualElement> tabList = _trapezElement.Q<TemplateContainer>()?
                .Q<VisualElement>("weapon-selector-root")?
                .Q<TabView>(className: "unity-tab-view")?
                .Q<VisualElement>("unity-tab-view__content-container")?
                .Children().ToList();
            for (int i = 0; i < tabHeaderList.Count; i++)
            {
                if (i < _weaponBuildings.Count)
                {
                    WeaponClasses weaponClass = (WeaponClasses)i;
                    tabHeaderList[i].Q<Label>().text = weaponClass.ToString();
                    if (tabList?.ElementAtOrDefault(i) != null) tabList[i].name = weaponClass.ToString();
                }
                else
                {
                    Debug.LogError("Not enough weapon classes to match the tab headers.");
                }
            }
        }

        /// <summary>
        /// Initializes the orientation buttons by cloning the visual tree asset and adding it to the trapez element.
        /// </summary>
        private void InitOrientationButtons()
        {
            if (OrientationButtonTree == null)
            {
                Debug.LogError("Orientation Button Tree is not assigned.");
                return;
            }

            VisualElement orientationButton = OrientationButtonTree.CloneTree();
            _trapezElement.Add(orientationButton);

            AddIconsToOrientationButtons(orientationButton);
        }
        
        /// <summary>
        /// Adds icons to the orientation buttons by setting their text.
        /// </summary>
        /// <param name="orientationButtons"></param>
        private void AddIconsToOrientationButtons(VisualElement orientationButtons)
        {
            Button arrowUp = orientationButtons.Q<Button>("arrow-up");
            Button arrowDown = orientationButtons.Q<Button>("arrow-down");
            Button arrowLeft = orientationButtons.Q<Button>("arrow-left");
            Button arrowRight = orientationButtons.Q<Button>("arrow-right");

            arrowUp.text = "\u2191"; // ↑
            arrowDown.text = "\u2193"; // ↓
            arrowLeft.text = "\u2190"; // ←
            arrowRight.text = "\u2192"; // →
        }
        
        /// <summary>
        /// Initializes the ammunition selector buttons by cloning the visual tree asset and adding it to the trapez element.
        /// </summary>
        private void InitAmmunitionSelectorButtons()
        {
            if (AmmunitionSelectorButtonTree == null)
            {
                Debug.LogError("Ammunition Selector Button Tree is not assigned.");
                return;
            }

            VisualElement ammunitionButtons = AmmunitionSelectorButtonTree.CloneTree();
            _trapezElement.Add(ammunitionButtons);
            
            AddIconsToAmmunitionSelectorButtons(ammunitionButtons);
        }

        private void AddIconsToAmmunitionSelectorButtons(VisualElement ammunitionButtons)
        {
            Button ammoType1Button = ammunitionButtons.Q<Button>("ammo-type-1");
            Button ammoType2Button = ammunitionButtons.Q<Button>("ammo-type-2");
            Button ammoType3Button = ammunitionButtons.Q<Button>("ammo-type-3");
            Button ammoType4Button = ammunitionButtons.Q<Button>("ammo-type-4");
            
            Button fireButton = ammunitionButtons.Q<Button>("fire-button");
            
            if (ammoType1Button == null || ammoType2Button == null || ammoType3Button == null || ammoType4Button == null || fireButton == null)
            {
                Debug.LogError("Ammunition buttons not found.");
                return;
            }
            ammoType1Button.iconImage = Background.FromTexture2D(AmmoType1Icon);
            ammoType2Button.iconImage = Background.FromTexture2D(AmmoType2Icon);
            ammoType3Button.iconImage = Background.FromTexture2D(AmmoType3Icon);
            ammoType4Button.iconImage = Background.FromTexture2D(AmmoType4Icon);
            fireButton.iconImage = Background.FromTexture2D(FireIcon);
            
            Label ammoType1Label = ammunitionButtons.Q<Label>("ammo-typ-1-label");
            Label ammoType2Label = ammunitionButtons.Q<Label>("ammo-typ-2-label");
            Label ammoType3Label = ammunitionButtons.Q<Label>("ammo-typ-3-label");
            Label ammoType4Label = ammunitionButtons.Q<Label>("ammo-typ-4-label");
            
            if (ammoType1Label == null || ammoType2Label == null || ammoType3Label == null || ammoType4Label == null)
            {
                Debug.LogError("Ammunition labels not found.");
                return;
            }
            
            ammoType1Label.text = "Normal";
            ammoType2Label.text = "Fire";
            ammoType3Label.text = "EMP";
            ammoType4Label.text = "HE";
        }


        // Todo: Remove this method in production
        private void PopulateWeaponsListForDevelopment()
        {
            AddToWeaponBuildingsList(WeaponClasses.Artillery, "ArtilleryBuilding");
            AddToWeaponBuildingsList(WeaponClasses.Artillery, "ArtilleryBuilding2");
            AddToWeaponBuildingsList(WeaponClasses.Artillery, "ArtilleryBuilding3");
            AddToWeaponBuildingsList(WeaponClasses.Rockets, "RocketLauncher");
            AddToWeaponBuildingsList(WeaponClasses.Rockets, "RocketLauncher2");
            AddToWeaponBuildingsList(WeaponClasses.Rockets, "RocketLauncher3");
            AddToWeaponBuildingsList(WeaponClasses.Rockets, "RocketLauncher4");
            AddToWeaponBuildingsList(WeaponClasses.Lasers, "LaserCannon");
            AddToWeaponBuildingsList(WeaponClasses.Lasers, "LaserCannon2");
        }
    }
}