namespace FortressForge.UI.CustomVisualElements
{
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// A custom visual element that represents a fillable resource container.
    /// </summary>
    [UxmlElement("FillableRessourceContainer")]
    public partial class FillableRessourceContainer : VisualElement
    {
        private readonly VisualElement _fillElement;
        private float _fillPercentage;
        private bool _isHorizontal;

        /// <summary>
        /// Gets or sets the fill percentage of the container.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the container is horizontal.
        /// </summary>
        public bool IsHorizontal
        {
            get => _isHorizontal;
            set
            {
                _isHorizontal = value;
                UpdateOrientation();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FillableRessourceContainer"/> class.
        /// </summary>
        public FillableRessourceContainer()
        {
            _fillElement = new VisualElement();
            _fillElement.AddToClassList("ressource-container-fill-element");

            Add(_fillElement);
        }

        /// <summary>
        /// Updates the orientation of the container based on the current setting.
        /// </summary>
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

        /// <summary>
        /// Adds a style class to the container's class list.
        /// </summary>
        /// <param name="className">The name of the class to add.</param>
        public void AddStyleForClassList(string className)
        {
            if (!string.IsNullOrEmpty(className))
            {
                AddToClassList(className);
            }
        }
    }
}