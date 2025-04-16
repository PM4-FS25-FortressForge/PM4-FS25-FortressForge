namespace FortressForge.UI.CustomVisualElements
{
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement("FillableRessourceContainer")]
    public partial class FillableRessourceContainer : VisualElement
    {
        private readonly VisualElement _fillElement;
        private float _fillPercentage;
        private bool _isHorizontal;

        public float FillPercentage
        {
            get => _fillPercentage;
            set
            {
                _fillPercentage = Mathf.Clamp01(value);
                if (_isHorizontal)
                {
                    _fillElement.style.width = Length.Percent(_fillPercentage * 100);
                }
                else
                {
                    _fillElement.style.height = Length.Percent(_fillPercentage * 100);
                }
            }
        }

        public bool IsHorizontal
        {
            get => _isHorizontal;
            set
            {
                _isHorizontal = value;
                UpdateOrientation();
            }
        }

        public FillableRessourceContainer()
        {
            _fillElement = new VisualElement();
            _fillElement.AddToClassList("ressource-container-fill-element");

            Add(_fillElement);
        }

        private void UpdateOrientation()
        {
            if (_isHorizontal)
            {
                style.flexDirection = FlexDirection.Row;
                _fillElement.style.height = Length.Percent(100);
                _fillElement.style.width = Length.Percent(_fillPercentage * 100);
            }
            else
            {
                style.flexDirection = FlexDirection.ColumnReverse;
                _fillElement.style.width = Length.Percent(100);
                _fillElement.style.height = Length.Percent(_fillPercentage * 100);
            }
        }

        public void AddStyleForClassList(string className)
        {
            if (!string.IsNullOrEmpty(className))
            {
                AddToClassList(className);
            }
        }
    }
}