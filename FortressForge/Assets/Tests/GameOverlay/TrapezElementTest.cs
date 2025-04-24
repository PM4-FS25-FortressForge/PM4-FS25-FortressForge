using System.Collections;
using FortressForge.UI.CustomVisualElements;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Tests.GameOverlay
{
    [TestFixture]
    public class TrapezElementTest : InputTestFixture
    {
        private Mouse _mouse;
        private VisualElement _root;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            InputSystem.RegisterLayout<Keyboard>();
            InputSystem.AddDevice<Keyboard>();
            Assert.That(InputSystem.devices, Has.Exactly(1).TypeOf<Keyboard>());

            InputSystem.AddDevice<Mouse>();
            _mouse = InputSystem.GetDevice<Mouse>();
            Assert.NotNull(_mouse, "Error: Mouse device not found.");
            
            InitUIDocument();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        private void InitUIDocument()
        {
            GameObject testingUiDocument = Resources.Load<GameObject>("Prefabs/Tests/UIDocumentTesting");
            Assert.IsNotNull(testingUiDocument, "TestingUiDocument konnte nicht geladen werden. Stellen Sie sicher, dass es sich im Ordner Resources/Tests befindet.");

            GameObject testingUiDocumentInstantiate = Object.Instantiate(testingUiDocument);
            Assert.IsNotNull(testingUiDocumentInstantiate, "NetworkManagerObject konnte nicht instanziiert werden.");

            UIDocument uiDocument = testingUiDocumentInstantiate.GetComponent<UIDocument>();
            Assert.IsNotNull(uiDocument, "UIDocument konnte nicht gefunden werden. Stellen Sie sicher, dass es dem GameObject hinzugefügt wurde.");

            _root= uiDocument.rootVisualElement;
            Assert.IsNotNull(_root, "Root VisualElement sollte nicht null sein.");

        }

        private TrapezElement InitializeTrapezElement(float topAngle = 90f, float rotation = 180f, string selectorName = "")
        {
            TrapezElement trapezElement = new();
            trapezElement.SetParameters(topAngle, rotation, selectorName);
            trapezElement.style.width = new StyleLength(1000);
            trapezElement.style.height = new StyleLength(100);

            return trapezElement;
        }

        [UnityTest]
        public IEnumerator TrapezElement_Constructor_ValidParameters_SetSelector()
        {
            const float topAngle = 30f;
            const float rotation = 45f;
            const string selectorName = "TestSelector";
            TrapezElement trapezElement = InitializeTrapezElement(topAngle, rotation, selectorName);

            Assert.IsNotNull(trapezElement, "TrapezElement instance should not be null.");
            Assert.IsNotEmpty(trapezElement.GetClasses(), "Selector name should be set.");
            Assert.IsTrue(trapezElement.ClassListContains(selectorName), "Selector name should be set correctly.");

            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TrapezElement_Constructor_ValidParameters_EmptySelector()
        {
            const float topAngle = 30f;
            const float rotation = 45f;
            TrapezElement trapezElement = InitializeTrapezElement(topAngle, rotation);
            
            Assert.IsNotNull(trapezElement, "TrapezElement instance should not be null.");
            Assert.IsEmpty(trapezElement.GetClasses(), "Selector name should not be set.");

            yield return null;
        }

        // [UnityTest]
        // public IEnumerator TrapezElementIsPointInTrapez_ValidClick_ReturnsTrue()
        // {
        //     TrapezElement trapezElement = InitializeTrapezElement();
        //     _root.Add(trapezElement);
        //
        //     Vector2 clickPosition = new(50f, 385f);
        //     Move(_mouse.position, clickPosition);
        //
        //     yield return new WaitUntil(() => _mouse.position.ReadValue() == clickPosition);
        //
        //     Assert.IsTrue(trapezElement.IsPointInTrapez(), "Click should be inside the trapez element.");
        //
        //     yield return null;
        // }
        [UnityTest]
        public IEnumerator TrapezElementIsPointInTrapez_ValidClick_ReturnsTrue()
        {
            TrapezElement trapezElement = InitializeTrapezElement();
            Assert.NotNull(trapezElement, "TrapezElement instance should not be null.");
            _root.Add(trapezElement);

            Vector2 clickPosition = new(50f, 385f);
            Debug.Log($"Setting mouse position to: {clickPosition}");
            Move(_mouse.position, clickPosition);

            yield return new WaitUntil(() =>
            {
                Vector2 currentMousePosition = _mouse.position.ReadValue();
                Debug.Log($"Current mouse position: {currentMousePosition}");
                return currentMousePosition == clickPosition;
            });

            Rect trapezBounds = trapezElement.worldBound;
            Debug.Log($"TrapezElement bounds: {trapezBounds}");
            Assert.IsTrue(trapezElement.IsPointInTrapez(), "Click should be inside the trapez element.");

            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TrapezElementIsPointInTrapez_InvalidClick_InsideVisualElement_ReturnsFalse()
        {
            TrapezElement trapezElement = InitializeTrapezElement();
            _root.Add(trapezElement);

            Vector2 clickPosition = new(5f, 385f);
            Move(_mouse.position, clickPosition);

            yield return new WaitUntil(() => _mouse.position.ReadValue() == clickPosition);

            Assert.IsFalse(trapezElement.IsPointInTrapez(), "Click should be outside the trapez element.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator TrapezElementIsPointInTrapez_InvalidClick_OutsideVisualElement_ReturnsFalse()
        {
            TrapezElement trapezElement = InitializeTrapezElement();
            _root.Add(trapezElement);

            Vector2 clickPosition = new(20f, 100f);
            Move(_mouse.position, clickPosition);

            yield return new WaitUntil(() => _mouse.position.ReadValue() == clickPosition);

            Assert.IsFalse(trapezElement.IsPointInTrapez(), "Click should be outside the trapez element.");

            yield return null;
        }
    }
}