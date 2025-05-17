using System;
using System.Collections.Generic;
using FishNet.Object.Synchronizing;
using FortressForge.Economy;
using FortressForge.Networking;
using FortressForge.Networking.Dto;
using FortressForge.UI.CustomVisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
    /// <summary>
    /// Generates the top trapez view for the building GameOverlay
    /// </summary>
    public class TopOverlayViewGenerator : MonoBehaviour
    {
        public UIDocument OverlayUIDocument;
        public VisualTreeAsset ResourceContainerAsset;
        public VisualTreeAsset PlanetViewContainerAsset;

        private VisualElement _overlayRoot;
        private VisualElement _overlayFrame;
        private Image _overlayImage;

        private readonly Dictionary<ResourceType, FillableRessourceContainer> _fillableRessourceContainers = new();
        private VisualElement _planetFill;
        private Label _planetChangeRateLabel;

        /// <summary>
        /// Initializes the TopTrapezViewGenerator with the provided EconomySync instance.
        /// </summary>
        /// <param name="economySync"> The EconomySync instance to use for synchronization.</param>
        public void Init(EconomySync economySync)
        {
            if (economySync == null)
            {
                Debug.LogError("EconomySync is null in TopOverlayViewGenerator!");
                return;
            }

            economySync.SyncedResources.OnChange += (op, key, value, asServer) =>
            {
                if (op != SyncDictionaryOperation.Set) return; // Only update on set operation
                if (key == ResourceType.GlobalMagma)
                {
                    UpdatePlanetMagmaFillableContainer(value);
                    return;
                }
                
                if (_fillableRessourceContainers.TryGetValue(key, out var container))
                {
                    UpdateRessourceFillableContainer(_overlayFrame, container, value);
                }
            };

            // Optionally preload initial values
            foreach (var entry in economySync.SyncedResources)
            {
                if (entry.Key == ResourceType.GlobalMagma)
                {
                    UpdatePlanetMagmaFillableContainer(entry.Value);
                    break;
                }
                
                if (_fillableRessourceContainers.TryGetValue(entry.Key, out var container))
                {
                    UpdateRessourceFillableContainer(_overlayFrame, container, entry.Value);
                }
            }
        }

        void OnEnable()
        {
            if (OverlayUIDocument == null)
            {
                Debug.LogError("UIDocument für Overlay nicht zugewiesen!");
                return;
            }

            _overlayRoot = OverlayUIDocument.rootVisualElement.Q<VisualElement>("root-frame");
            if (_overlayRoot == null)
            {
                Debug.LogError("Overlay-Root-Element mit dem Namen 'overlayRoot' nicht gefunden!");
                return;
            }

            _overlayRoot.pickingMode = PickingMode.Ignore;
            _overlayFrame = _overlayRoot.Q<VisualElement>("top-view");
            if (_overlayFrame == null)
            {
                Debug.Log("OverlayFrame not found!");
                return;
            }
            
            TrapezElement trapezElement = CreateTrapezElement("trapez-frame", "top-trapez-frame");
            _overlayFrame.Add(trapezElement);

            TrapezElement ressourceContainerTrapezoid = CreateTrapezElement("ressource-container");
            trapezElement.Add(ressourceContainerTrapezoid);

            if (ResourceContainerAsset == null)
            {
                Debug.LogError("ResourceContainer asset is not assigned!");
                return;
            }

            VisualElement resourceContainer = ResourceContainerAsset.CloneTree();
            VisualElement planetContainer = PlanetViewContainerAsset.CloneTree();
            
            _planetFill = planetContainer.Q<VisualElement>("PlanetFill");
            _planetChangeRateLabel = planetContainer.Q<Label>("MagmaPercentageLabel");

            if (_planetFill == null)
            {
                Debug.LogError("PlanetFill element not found in UXML!");
            }
            
            trapezElement.Add(planetContainer);
            ressourceContainerTrapezoid.Add(resourceContainer);

            LoadRessourceFillContainer("FillableRessourceContainer-left-top", resourceContainer, ResourceType.Power, "Energy");
            LoadRessourceFillContainer("FillableRessourceContainer-right-top", resourceContainer, ResourceType.Amunition, "Ammunition");
            LoadRessourceFillContainer("FillableRessourceContainer-left-bottom", resourceContainer, ResourceType.Metal, "Metal");
            LoadRessourceFillContainer("FillableRessourceContainer-right-bottom", resourceContainer, ResourceType.Concrete, "Concrete");
        }
 
        /// <summary>
        /// Creates a new TrapezElement with the specified selector and optional class name.
        /// </summary>
        /// <param name="selector">The selector for the trapez element.</param>
        /// <param name="className">Optional class name to add to the trapez element.</param>
        /// <returns>A new instance of TrapezElement.</returns>
        private TrapezElement CreateTrapezElement(string selector, string className = null)
        {
            TrapezElement trapezElement = new();
            trapezElement.SetParameters(90f, 180f, selector);
            if (!string.IsNullOrEmpty(className))
            {
                trapezElement.AddToClassList(className);
            }

            return trapezElement;
        }

        /// <summary>
        /// Loads the resource fill container with the specified parameters.
        /// </summary>
        /// <param name="elementName">The name of the element to load.</param>
        /// <param name="resourceContainer">The container element that holds the resource information.</param>
        /// <param name="resourceType">The type of resource to load.</param>
        /// <param name="ressourceTitle"> The title of the resource.</param>
        private void LoadRessourceFillContainer(string elementName, VisualElement resourceContainer, ResourceType resourceType,
            string ressourceTitle = "Unknown Ressource")
        {
            FillableRessourceContainer fillableRessourceContainer =
                resourceContainer.Q<FillableRessourceContainer>(elementName);
            if (fillableRessourceContainer == null)
            {
                Debug.LogError("FillableRessourceContainer not found!");
            }
            else
            {
                fillableRessourceContainer.FillPercentage = 0f;
                fillableRessourceContainer.IsHorizontal = true;
                fillableRessourceContainer.AddToClassList("ressource-container-fillable");
                _fillableRessourceContainers.TryAdd(resourceType, fillableRessourceContainer);
            }

            SetLabelText(resourceContainer, elementName + "-title", ressourceTitle, "Title label not found!");
            SetLabelText(resourceContainer, elementName + "-amount", "0/0", "Current amount label not found!");
        }

        /// <summary>
        /// Sets the text of a label in the specified container.
        /// </summary>
        /// <param name="container"> The container element that holds the label.</param>
        /// <param name="labelName"> The name of the label to set.</param>
        /// <param name="text"> The text to set for the label.</param>
        /// <param name="errorMessage"> The error message to log if the label is not found.</param>
        private static void SetLabelText(VisualElement container, string labelName, string text, string errorMessage)
        {
            Label label = container.Q<Label>(labelName);
            if (label == null)
            {
                Debug.LogError(errorMessage);
            }
            else
            {
                label.text = text;
            }
        }

        /// <summary>
        /// Updates the fillable resource container with the current amount of the resource.
        /// </summary>
        /// <param name="resourceContainer">The container element that holds the resource information.</param>
        /// <param name="fillableRessourceContainer">The fillable resource container to update.</param>
        /// <param name="resource">The resource to get the current amount from.</param>
        private void UpdateRessourceFillableContainer(VisualElement resourceContainer, FillableRessourceContainer fillableRessourceContainer, ResourceDto resource)
        {
            fillableRessourceContainer.FillPercentage = Mathf.Clamp(resource.CurrentAmount / resource.MaxAmount, 0f, 1f);
            AutoSizeLabel amountAutoSizeLabel = resourceContainer.Q<AutoSizeLabel>(fillableRessourceContainer.name + "-amount");
            Label changeRateLabel = resourceContainer.Q<Label>(fillableRessourceContainer.name + "-change-rate");
            if (amountAutoSizeLabel != null)
            {
                amountAutoSizeLabel.text = $"{resource.CurrentAmount}/{resource.MaxAmount}";
                amountAutoSizeLabel.UpdateFontSize();
            }

            if (changeRateLabel != null)
            {
                changeRateLabel.text = resource.DeltaAmount > 0 ? $"+{resource.DeltaAmount}" : resource.DeltaAmount < 0 ? $"{resource.DeltaAmount}" : "0";
            }
        }

        private void UpdatePlanetMagmaFillableContainer(ResourceDto resource)
        {
            if (resource.Type == ResourceType.GlobalMagma && _planetFill != null)
            {
                float fillPercent = Mathf.Clamp01(resource.CurrentAmount / resource.MaxAmount);
                _planetFill.style.height = new Length(fillPercent * 100f, LengthUnit.Percent);

                if (_planetChangeRateLabel != null)
                {
                    _planetChangeRateLabel.text = $"{Math.Round(fillPercent * 100f)}%";
                }
            }
        }
    }
}