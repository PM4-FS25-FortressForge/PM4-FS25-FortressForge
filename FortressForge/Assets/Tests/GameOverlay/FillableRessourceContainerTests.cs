using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;
using FortressForge.UI.CustomVisualElements;

namespace Tests.GameOverlay
{
    [TestFixture]
    public class FillableRessourceContainerTests
    {
        private FillableRessourceContainer _container;

        [SetUp]
        public void Setup()
        {
            _container = new FillableRessourceContainer();
        }

        [TearDown]
        public void TearDown()
        {
            _container = null;
        }

        [Test]
        public void Constructor_InitializesCorrectly()
        {
            Assert.IsNotNull(_container);
            Assert.AreEqual(1, _container.childCount);
            Assert.AreEqual(0f, _container.FillPercentage);
            Assert.IsFalse(_container.IsHorizontal);
        }

        [Test]
        public void FillPercentage_SetValue_ClampsBetween0And1()
        {
            _container.FillPercentage = -0.5f;
            Assert.AreEqual(0f, _container.FillPercentage);

            _container.FillPercentage = 1.5f;
            Assert.AreEqual(1f, _container.FillPercentage);

            _container.FillPercentage = 0.75f;
            Assert.AreEqual(0.75f, _container.FillPercentage);
        }

        [Test]
        public void FillPercentage_UpdatesFillElementDimensions_Vertical()
        {
            _container.IsHorizontal = false;

            _container.FillPercentage = 0.5f;

            VisualElement fillElement = _container[0];
            Assert.AreEqual(Length.Percent(100), fillElement.style.width.value);
            Assert.AreEqual(Length.Percent(50), fillElement.style.height.value);
        }

        [Test]
        public void FillPercentage_UpdatesFillElementDimensions_Horizontal()
        {
            _container.IsHorizontal = true;

            _container.FillPercentage = 0.3f;

            VisualElement fillElement = _container[0];
            Assert.AreEqual(30f, fillElement.style.width.value.value, 0.01f);
            Assert.AreEqual(100f, fillElement.style.height.value.value, 0.01f);

            Assert.AreEqual(LengthUnit.Percent, fillElement.style.width.value.unit, "Width should be in Percent");
            Assert.AreEqual(LengthUnit.Percent, fillElement.style.height.value.unit, "Height should be in Percent");
        }

        [Test]
        public void IsHorizontal_ChangesOrientation()
        {
            _container.IsHorizontal = true;

            Assert.AreEqual(FlexDirection.Row, _container.style.flexDirection.value);

            _container.IsHorizontal = false;

            Assert.AreEqual(FlexDirection.ColumnReverse, _container.style.flexDirection.value);
        }

        [Test]
        public void AddStyleForClassList_AddsClassWhenValid()
        {
            string testClass = "test-class";

            _container.AddStyleForClassList(testClass);

            Assert.IsTrue(_container.ClassListContains(testClass));
        }

        [Test]
        public void AddStyleForClassList_DoesNotAddEmptyOrNullClass()
        {
            int initialClassCount = _container.GetClasses().Count();

            _container.AddStyleForClassList(null);

            Assert.AreEqual(initialClassCount, _container.GetClasses().Count());

            _container.AddStyleForClassList("");

            Assert.AreEqual(initialClassCount, _container.GetClasses().Count());
        }

        [Test]
        public void UpdateOrientation_SetsCorrectDimensions_Vertical()
        {
            _container.FillPercentage = 0.6f;
            _container.IsHorizontal = true;

            _container.IsHorizontal = false;

            VisualElement fillElement = _container[0];
            Assert.AreEqual(FlexDirection.ColumnReverse, _container.style.flexDirection.value);

            Assert.AreEqual(100f, fillElement.style.width.value.value, 0.01f);
            Assert.AreEqual(60f, fillElement.style.height.value.value, 0.01f);

            Assert.AreEqual(LengthUnit.Percent, fillElement.style.width.value.unit, "Width should be in Percent");
            Assert.AreEqual(LengthUnit.Percent, fillElement.style.height.value.unit, "Height should be in Percent");
        }

        [Test]
        public void UpdateOrientation_SetsCorrectDimensions_Horizontal()
        {
            _container.FillPercentage = 0.4f;
            _container.IsHorizontal = false;

            _container.IsHorizontal = true;

            VisualElement fillElement = _container[0];
            Assert.AreEqual(FlexDirection.Row, _container.style.flexDirection.value);
            Assert.AreEqual(Length.Percent(100), fillElement.style.height.value);
            Assert.AreEqual(Length.Percent(40), fillElement.style.width.value);
        }
    }
}