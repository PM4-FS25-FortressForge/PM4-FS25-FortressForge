using FortressForge.UI.CustomVisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
    /// <summary>
    /// Generates the top trapez view for the building GameOverlay
    /// </summary>
    public class TopTrapezViewGenerator : MonoBehaviour
    {
        public UIDocument overlayUIDocument;
        private VisualElement _overlayRoot;
        private VisualElement _overlayFrame;
        private Image _overlayImage;
        public VisualTreeAsset resourceContainerAsset;

        void OnEnable()
        {
            if (overlayUIDocument == null)
            {
                Debug.LogError("UIDocument für Overlay nicht zugewiesen!");
                return;
            }

            _overlayRoot = overlayUIDocument.rootVisualElement.Q<VisualElement>("root-frame");
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

            ressourceContainerTrapezoid.RegisterCallback<PointerDownEvent>(_ =>
                Debug.Log("RessourceContainer Trapezoid clicked!")
            );

            if (resourceContainerAsset == null)
            {
                Debug.LogError("ResourceContainer asset is not assigned!");
                return;
            }

            VisualElement resourceContainer = resourceContainerAsset.CloneTree();
            resourceContainer.AddToClassList("ressource-container");
            trapezElement.Add(resourceContainer);

            LoadRessourceFillContainer("FillableRessourceContainer-left-top", resourceContainer, "Energy");
            LoadRessourceFillContainer("FillableRessourceContainer-right-top", resourceContainer, "Munition");
            LoadRessourceFillContainer("FillableRessourceContainer-left-bottom", resourceContainer, "Metal");
            LoadRessourceFillContainer("FillableRessourceContainer-right-bottom", resourceContainer, "Stone");
        }

        /// <summary>
        /// Creates a new TrapezElement with the specified selector and optional class name.
        /// </summary>
        /// <param name="selector">The selector for the trapez element.</param>
        /// <param name="className">Optional class name to add to the trapez element.</param>
        /// <returns>A new instance of TrapezElement.</returns>
        private TrapezElement CreateTrapezElement(string selector, string className = null)
        {
            TrapezElement trapezElement = new TrapezElement();
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
        /// <param name="elementName"></param>
        /// <param name="resourceContainer"></param>
        /// <param name="ressourceTitle"></param>
        private static void LoadRessourceFillContainer(string elementName, VisualElement resourceContainer, string ressourceTitle = "Unknown Ressource")
        {
            FillableRessourceContainer fillableRessourceContainer = resourceContainer.Q<FillableRessourceContainer>(elementName);
            if (fillableRessourceContainer == null)
            {
                Debug.LogError("FillableRessourceContainer not found!");
            }
            else
            {
                fillableRessourceContainer.FillPercentage = 0.5f;
                fillableRessourceContainer.IsHorizontal = true;
                fillableRessourceContainer.AddStyleForClassList("ressource-container-fillable");
            }
        
            SetLabelText(resourceContainer, elementName + "-title", ressourceTitle, "Title label not found!");
            SetLabelText(resourceContainer, elementName + "-amount", "100/200", "Current amount label not found!");
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
    }
}