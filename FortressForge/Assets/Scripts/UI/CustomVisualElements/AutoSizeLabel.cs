using System;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace FortressForge.UI.CustomVisualElements
{
    /// <summary>
    /// A custom label that automatically adjusts its font size to fit within its parent's dimensions.
    /// </summary>
    public class AutoSizeLabel : Label
    {
        private const int DEFAULT_FONT_SIZE = 19;

        /// <summary>
        /// A nested class for serialized data specific to the AutoSizeLabel.
        /// </summary>
        [ExcludeFromDocs]
        [Serializable]
        public new class UxmlSerializedData : TextElement.UxmlSerializedData
        {
            public override object CreateInstance() => new AutoSizeLabel();
        }

        /// <summary>
        /// Dynamically adjusts the font size of the label to ensure it fits within the dimensions
        /// of its parent element, taking into account margins and padding.
        /// The font size is only updated if the change exceeds a defined threshold to avoid
        /// excessive updates for minor changes.
        /// </summary>
        public void UpdateFontSize()
        {
            if (hierarchy.parent == null) return;

            Vector2 parentSize = hierarchy.parent.layout.size;

            if (float.IsNaN(parentSize.x) || float.IsNaN(parentSize.y))
            {
                style.fontSize = DEFAULT_FONT_SIZE;
                return;
            }

            float horizontalMargins = resolvedStyle.marginLeft + resolvedStyle.marginRight;
            float verticalMargins = resolvedStyle.marginTop + resolvedStyle.marginBottom;
            float horizontalPadding = resolvedStyle.paddingLeft + resolvedStyle.paddingRight;
            float verticalPadding = resolvedStyle.paddingTop + resolvedStyle.paddingBottom;

            float availableWidth = parentSize.x - horizontalMargins - horizontalPadding;
            float availableHeight = parentSize.y - verticalMargins - verticalPadding;

            Vector2 labelSize = MeasureTextSize(text, float.MaxValue, MeasureMode.Undefined, float.MaxValue, MeasureMode.Undefined);

            float scaleFactor = Mathf.Min(availableWidth / labelSize.x, availableHeight / labelSize.y);

            const float threshold = 0.05f;

            float currentFontSize = resolvedStyle.fontSize;
            float newFontSize = Mathf.Max(1, Mathf.FloorToInt(currentFontSize * scaleFactor));
            if (Mathf.Abs(newFontSize - currentFontSize) / currentFontSize > threshold)
            {
                style.fontSize = newFontSize;
            }
        }
    }
}