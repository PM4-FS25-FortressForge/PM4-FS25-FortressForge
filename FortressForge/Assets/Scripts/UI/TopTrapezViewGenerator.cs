using FortressForge.UI.CustomVisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
    public class TopTrapezViewGenerator : MonoBehaviour
    {
        public UIDocument overlayUIDocument;
        private VisualElement overlayRoot;
        private VisualElement overlayFrame;
        private Image overlayImage;
        public VisualTreeAsset resourceContainerAsset;

        void OnEnable()
        {
            if (overlayUIDocument != null)
            {
                overlayRoot = overlayUIDocument.rootVisualElement.Q<VisualElement>("root-frame");
                if (overlayRoot != null)
                {
                    overlayRoot.pickingMode = PickingMode.Ignore;

                    overlayFrame = overlayRoot.Q<VisualElement>("top-view");
                    if (overlayFrame == null)
                    {
                        Debug.Log("OverlayFrame not found!");
                        return;
                    }


                    TrapezElement trapezElement = new();
                    trapezElement.SetParameters(90f, 180f, "trapez-frame");
                    trapezElement.AddToClassList("top-trapez-frame");

                    overlayFrame.Add(trapezElement);

                    TrapezElement ressourceContainerTrapezoid = new();
                    ressourceContainerTrapezoid.SetParameters(90f, 180f, "ressource-container");
                    trapezElement.Add(ressourceContainerTrapezoid);

                    ressourceContainerTrapezoid.RegisterCallback<PointerDownEvent>(_ =>
                    {
                        Debug.Log("RessourceContainer Trapezoid clicked!");
                    });

                    // load the ui element ResourceContainer
                    if (resourceContainerAsset != null)
                    {
                        VisualElement resourceContainer = resourceContainerAsset.CloneTree();
                        resourceContainer.AddToClassList("ressource-container");
                        trapezElement.Add(resourceContainer);

                        LoadRessourceFillContainer("FillableRessourceContainer-left-top", resourceContainer, "Energy");
                        LoadRessourceFillContainer("FillableRessourceContainer-right-top", resourceContainer,
                            "Munition");
                        LoadRessourceFillContainer("FillableRessourceContainer-left-bottom", resourceContainer,
                            "Metal");
                        LoadRessourceFillContainer("FillableRessourceContainer-right-bottom", resourceContainer,
                            "Stone");
                    }
                    else
                    {
                        Debug.LogError("ResourceContainer asset is not assigned!");
                    }
                }
                else
                {
                    Debug.LogError("Overlay-Root-Element mit dem Namen 'overlayRoot' nicht gefunden!");
                }
            }
            else
            {
                Debug.LogError("UIDocument für Overlay nicht zugewiesen!");
            }
        }

        private static void LoadRessourceFillContainer(string elementName, VisualElement resourceContainer,
            string ressourceTitle = "Unknown Ressource")
        {
            FillableRessourceContainer fillableRessourceContainer =
                resourceContainer.Q<FillableRessourceContainer>(elementName);
            if (fillableRessourceContainer != null)
            {
                fillableRessourceContainer.FillPercentage = 0.5f;
                fillableRessourceContainer.IsHorizontal = true;
                fillableRessourceContainer.AddStyleForClassList("ressource-container-fillable");
            }
            else
            {
                Debug.LogError("FillableRessourceContainer not found!");
            }

            Label titleLabel = resourceContainer.Q<Label>(elementName + "-title");
            if (titleLabel != null)
            {
                titleLabel.text = ressourceTitle;
            }
            else
            {
                Debug.LogError("Title label not found!");
            }

            Label currentAmountLabel = resourceContainer.Q<Label>(elementName + "-amount");
            if (currentAmountLabel != null)
            {
                currentAmountLabel.text = "100/200";
            }
            else
            {
                Debug.LogError("Current amount label not found!");
            }
        }
    }
}