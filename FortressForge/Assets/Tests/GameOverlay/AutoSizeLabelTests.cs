using System;
using System.Collections;
using FortressForge.UI.CustomVisualElements;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Tests.GameOverlay
{
    [TestFixture]
    public class AutoSizeLabelTests
    {
        private AutoSizeLabel _label;
        private VisualElement _parent;
        private VisualElement _root;

        [SetUp]
        public void Setup()
        {
            InitUIDocument();
            _parent = new VisualElement();
            _root.Add(_parent);
            _label = new AutoSizeLabel();
            _parent.Add(_label);
        }

        /// <summary>
        /// Initializes the UI document used for testing.
        /// Loads the prefab from the Resources folder and ensures it is properly instantiated.
        /// </summary>
        private void InitUIDocument()
        {
            GameObject testingUiDocument = Resources.Load<GameObject>("Prefabs/Tests/UIDocumentTesting");
            Assert.IsNotNull(testingUiDocument, "TestingUiDocument konnte nicht geladen werden. Stellen Sie sicher, dass es sich im Ordner Resources/Tests befindet.");

            GameObject testingUiDocumentInstantiate = Object.Instantiate(testingUiDocument);
            Assert.IsNotNull(testingUiDocumentInstantiate, "NetworkManagerObject konnte nicht instanziiert werden.");

            UIDocument uiDocument = testingUiDocumentInstantiate.GetComponent<UIDocument>();
            Assert.IsNotNull(uiDocument, "UIDocument konnte nicht gefunden werden. Stellen Sie sicher, dass es dem GameObject hinzugefügt wurde.");

            _root = uiDocument.rootVisualElement;
            Assert.IsNotNull(_root, "Root VisualElement sollte nicht null sein.");
        }

        [TearDown]
        public void TearDown()
        {
            _label = null;
            _parent = null;
        }

        [UnityTest]
        public IEnumerator Constructor_InitializesCorrectly()
        {
            AutoSizeLabel newLabel = new();

            Assert.IsNotNull(newLabel);
            Assert.IsNull(newLabel.hierarchy.parent);
            Assert.IsEmpty(newLabel.text);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateFontSize_DoesNothingWhenNoParent()
        {
            AutoSizeLabel orphanLabel = new() { text = "Test", style = { fontSize = 0f } };
            orphanLabel.UpdateFontSize();
            Assert.AreEqual(0, orphanLabel.resolvedStyle.fontSize);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateFontSize_AdjustsSizeForParentDimensions()
        {
            _parent.style.width = 200;
            _parent.style.height = 100;
            _label.text = "Test Text";

            yield return WaitForStyleUpdate(_parent, _parent.style.width.value.value, _parent.style.height.value.value);
            float initialSize = _label.resolvedStyle.fontSize;
            _label.UpdateFontSize();

            Assert.AreNotEqual(initialSize, _label.resolvedStyle.fontSize);
            Assert.Greater(_label.resolvedStyle.fontSize, 0);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateFontSize_RespectsMarginsAndPadding()
        {
            _parent.style.width = 200;
            _parent.style.height = 100;
            _label.text = "Test Text";

            yield return WaitForStyleUpdate(_parent, _parent.style.width.value.value, _parent.style.height.value.value);

            _label.UpdateFontSize();

            float sizeWithoutConstraints = _label.resolvedStyle.fontSize;

            _label.style.marginLeft = 10;
            _label.style.marginRight = 10;
            _label.style.marginTop = 5;
            _label.style.marginBottom = 5;
            _label.style.paddingLeft = 5;
            _label.style.paddingRight = 5;
            _label.style.paddingTop = 2;
            _label.style.paddingBottom = 2;

            yield return new WaitUntil(() => Math.Abs(_label.resolvedStyle.paddingBottom - _parent.resolvedStyle.paddingBottom) <= 5,
                new TimeSpan(0, 0, 5),
                () => Debug.Log($"PaddingBottom: {_label.resolvedStyle.paddingBottom} >= {_parent.resolvedStyle.paddingBottom}"));

            _label.UpdateFontSize();
            float sizeWithConstraints = _label.resolvedStyle.fontSize;

            Assert.Less(sizeWithConstraints, sizeWithoutConstraints);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateFontSize_AppliesThresholdForChanges()
        {
            _parent.style.width = 200;
            _parent.style.height = 100;
            _label.text = "Test Text";

            yield return WaitForStyleUpdate(_parent, _parent.style.width.value.value, _parent.style.height.value.value);
            _label.UpdateFontSize();
            float initialSize = _label.resolvedStyle.fontSize;

            _parent.style.width = 205;
            yield return WaitForStyleUpdate(_parent, _parent.style.width.value.value, _parent.style.height.value.value);
            _label.UpdateFontSize();
            Assert.AreEqual(initialSize, _label.resolvedStyle.fontSize);

            _parent.style.width = 300;
            yield return WaitForStyleUpdate(_parent, _parent.style.width.value.value, _parent.style.height.value.value);
            _label.UpdateFontSize();
            Assert.AreNotEqual(initialSize, _label.resolvedStyle.fontSize);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateFontSize_HandlesEmptyText()
        {
            _parent.style.width = 200;
            _parent.style.height = 100;
            _label.text = "";

            yield return WaitForStyleUpdate(_parent, _parent.style.width.value.value, _parent.style.height.value.value);
            _label.UpdateFontSize();
            Assert.AreEqual(1f, _label.resolvedStyle.fontSize);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateFontSize_MaintainsMinimumFontSize()
        {
            _parent.style.width = 10;
            _parent.style.height = 5;
            _label.text = "Very Long Text That Won't Fit";

            yield return WaitForStyleUpdate(_parent, _parent.style.width.value.value, _parent.style.height.value.value);
            _label.UpdateFontSize();

            Assert.AreEqual(1f, _label.resolvedStyle.fontSize);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateFontSize_ScalesBasedOnWidthConstraint()
        {
            _parent.style.height = 1000;
            _parent.style.width = 50;
            _label.text = "This text will be width-constrained";

            yield return WaitForStyleUpdate(_parent, _parent.style.width.value.value, _parent.style.height.value.value);
            _label.UpdateFontSize();
            float widthConstrainedSize = _label.resolvedStyle.fontSize;

            _parent.style.width = 1000;
            _parent.style.height = 50;
            yield return WaitForStyleUpdate(_parent, _parent.style.width.value.value, _parent.style.height.value.value);
            _label.UpdateFontSize();
            float heightConstrainedSize = _label.resolvedStyle.fontSize;

            Assert.Less(widthConstrainedSize, heightConstrainedSize);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UxmlSerializedData_CreatesCorrectInstance()
        {
            UxmlSerializedData uxmlData = new AutoSizeLabel.UxmlSerializedData();
            object instance = uxmlData.CreateInstance();
            Assert.IsNotNull(instance);
            Assert.IsInstanceOf<AutoSizeLabel>(instance);

            yield return null;
        }

        /// <summary>
        /// Waits for the style update of a VisualElement to complete.
        /// </summary>
        /// <param name="element">The VisualElement to check.</param>
        /// <param name="targetWidth">The target width to wait for.</param>
        /// <param name="targetHeight">The target height to wait for.</param>
        /// <returns>An IEnumerator for coroutine.</returns>
        private IEnumerator WaitForStyleUpdate(VisualElement element, float targetWidth, float targetHeight)
        {
            yield return new WaitUntil(
                () => Math.Abs(element.resolvedStyle.width - targetWidth) <= 5 && Math.Abs(element.resolvedStyle.height - targetHeight) <= 5,
                new TimeSpan(0, 0, 5),
                () => Debug.Log($"Width: {element.resolvedStyle.width} >= {targetWidth}, Height: {element.resolvedStyle.height} >= {targetHeight}")
            );
        }
    }
}