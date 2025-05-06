using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace FortressForge.UI.CustomVisualElements
{
    /// <summary>
    /// Custom Visual Element that represents a trapezium shape.
    /// </summary>
    [UxmlElement("TrapezElement")]
    public partial class TrapezElement : VisualElement
    {
        private static readonly ushort[] K_INDICES = { 0, 1, 2, 2, 3, 0 };

        private float _rotationAngle;
        private float _topAngle = 120f;

        private Color _colorLeft;
        private Color _colorTop;
        private Color _colorRight;
        private Color _colorBottom;

        private float _leftBorderWidth = 2f;
        private float _topBorderWidth = 2f;
        private float _rightBorderWidth = 2f;
        private float _bottomBorderWidth = 2f;

        private const float BRIGHTNESS_BOOST = 1.14f;

        private readonly Vector2[] _rotatedVerts = new Vector2[4];

        public TrapezElement()
        {
            generateVisualContent += OnGenerateVisualContent;
            pickingMode = PickingMode.Position;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// Called when the geometry of the element changes.
        /// </summary>
        /// <param name="evt">The geometry changed event.</param>
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            MarkDirtyRepaint();
        }

        /// <summary>
        /// Sets the parameters for the trapezium.
        /// </summary>
        /// <param name="topAngle"> The angle at the top of the trapezium.</param>
        /// <param name="rotation"> The rotation angle of the trapezium.</param>
        /// <param name="selector"> Optional selector to add to the class list.</param>
        public void SetParameters(float topAngle, float rotation, string selector = "")
        {
            _topAngle = Mathf.Clamp(topAngle, 1f, 179f);
            _rotationAngle = rotation % 360f;

            if (!string.IsNullOrEmpty(selector))
            {
                AddToClassList(selector);
            }

            MarkDirtyRepaint();
        }

        /// <summary>
        /// Generates the visual content of the trapezium.
        /// </summary>
        /// <param name="mgc">The mesh generation context.</param>
        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            float width = resolvedStyle.width;
            float height = resolvedStyle.height;

            float topIndent = Mathf.Tan((180f - _topAngle) * 0.5f * Mathf.Deg2Rad) * height;

            Vector2[] baseVertices =
            {
                new(0, height),
                new(topIndent, 0),
                new(width - topIndent, 0),
                new(width, height)
            };

            Vector2 pivot = new(width * 0.5f, height * 0.5f);

            Vertex[] vertices = new Vertex[4];
            Vector2[] uvs = { new(0, 1), new(0, 0), new(1, 0), new(1, 1) };
            for (int i = 0; i < 4; i++)
            {
                Vector2 rotated = RotatePoint(baseVertices[i], pivot, _rotationAngle);
                _rotatedVerts[i] = rotated;

                vertices[i].position = new Vector3(rotated.x, rotated.y, Vertex.nearZ);
                vertices[i].tint = resolvedStyle.color;
                vertices[i].uv = uvs[i];
            }

            MeshWriteData mesh = mgc.Allocate(vertices.Length, K_INDICES.Length);
            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(K_INDICES);

            if (_rotationAngle >= 180f)
            {
                _colorLeft = resolvedStyle.borderRightColor;
                _colorTop = resolvedStyle.borderBottomColor;
                _colorRight = resolvedStyle.borderLeftColor;
                _colorBottom = resolvedStyle.borderTopColor;

                _leftBorderWidth = resolvedStyle.borderRightWidth != 0 ? resolvedStyle.borderRightWidth : _leftBorderWidth;
                _topBorderWidth = resolvedStyle.borderBottomWidth != 0 ? resolvedStyle.borderBottomWidth : _topBorderWidth;
                _rightBorderWidth = resolvedStyle.borderLeftWidth != 0 ? resolvedStyle.borderLeftWidth : _rightBorderWidth;
                _bottomBorderWidth = resolvedStyle.borderTopWidth != 0 ? resolvedStyle.borderTopWidth : _bottomBorderWidth;
            }
            else
            {
                _colorLeft = resolvedStyle.borderLeftColor;
                _colorTop = resolvedStyle.borderTopColor;
                _colorRight = resolvedStyle.borderRightColor;
                _colorBottom = resolvedStyle.borderBottomColor;

                _leftBorderWidth = resolvedStyle.borderLeftWidth != 0 ? resolvedStyle.borderLeftWidth : _leftBorderWidth;
                _topBorderWidth = resolvedStyle.borderTopWidth != 0 ? resolvedStyle.borderTopWidth : _topBorderWidth;
                _rightBorderWidth = resolvedStyle.borderRightWidth != 0 ? resolvedStyle.borderRightWidth : _rightBorderWidth;
                _bottomBorderWidth = resolvedStyle.borderBottomWidth != 0 ? resolvedStyle.borderBottomWidth : _bottomBorderWidth;
            }

            _colorLeft = _colorLeft.a == 0 ? BrightenColor(resolvedStyle.color, BRIGHTNESS_BOOST) : _colorLeft;
            _colorTop = _colorTop.a == 0 ? BrightenColor(resolvedStyle.color, BRIGHTNESS_BOOST) : _colorTop;
            _colorRight = _colorRight.a == 0 ? BrightenColor(resolvedStyle.color, BRIGHTNESS_BOOST) : _colorRight;
            _colorBottom = _colorBottom.a == 0 ? BrightenColor(resolvedStyle.color, BRIGHTNESS_BOOST) : _colorBottom;
            DrawLine(mgc, _rotatedVerts[0], _rotatedVerts[1], _colorLeft, _leftBorderWidth);
            DrawLine(mgc, _rotatedVerts[1], _rotatedVerts[2], _colorTop, _topBorderWidth);
            DrawLine(mgc, _rotatedVerts[2], _rotatedVerts[3], _colorRight, _rightBorderWidth);
            DrawLine(mgc, _rotatedVerts[3], _rotatedVerts[0], _colorBottom, _bottomBorderWidth);
        }

        /// <summary>
        /// Brightens a color by a given factor.
        /// </summary>
        /// <param name="original">The original color.</param>
        /// <param name="factor">The factor to brighten the color.</param>
        /// <returns></returns>
        private Color BrightenColor(Color original, float factor)
        {
            Color brightColor = new(
                Mathf.Min(1f, original.r * factor),
                Mathf.Min(1f, original.g * factor),
                Mathf.Min(1f, original.b * factor),
                original.a
            );
            return brightColor;
        }


        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        /// <param name="mgc">The mesh generation context.</param>
        /// <param name="start">The start point of the line.</param>
        /// <param name="end">The end point of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        private void DrawLine(MeshGenerationContext mgc, Vector2 start, Vector2 end, Color color, float thickness = 2f)
        {
            Vector2 dir = (end - start).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

            Vertex[] verts = new Vertex[4];
            verts[0].position = new Vector3(start.x + normal.x, start.y + normal.y, Vertex.nearZ);
            verts[1].position = new Vector3(start.x - normal.x, start.y - normal.y, Vertex.nearZ);
            verts[2].position = new Vector3(end.x - normal.x, end.y - normal.y, Vertex.nearZ);
            verts[3].position = new Vector3(end.x + normal.x, end.y + normal.y, Vertex.nearZ);

            for (int i = 0; i < 4; i++)
            {
                verts[i].tint = color;
                verts[i].uv = Vector2.zero;
            }

            ushort[] indices = { 0, 1, 2, 2, 3, 0 };
            MeshWriteData mesh = mgc.Allocate(verts.Length, indices.Length);
            mesh.SetAllVertices(verts);
            mesh.SetAllIndices(indices);
        }

        /// <summary>
        /// Rotates a point around a pivot by a given angle.
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="pivot">The pivot point around which to rotate.</param>
        /// <param name="angle">The angle in degrees.</param>
        /// <returns>The rotated point.</returns>
        private Vector2 RotatePoint(Vector2 point, Vector2 pivot, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            float sin = Mathf.Sin(rad);
            float cos = Mathf.Cos(rad);

            Vector2 dir = point - pivot;

            return new Vector2(
                dir.x * cos - dir.y * sin + pivot.x,
                dir.x * sin + dir.y * cos + pivot.y
            );
        }

        /// <summary>
        /// Checks if a point is inside the trapezium.
        /// </summary>
        /// <returns>True if the point is inside the trapezium, false otherwise.</returns>
        public bool IsPointInTrapez()
        {
            Vector2 localPoint = GetVisualElementLocalCoordinates();
            float width = resolvedStyle.width;
            float height = resolvedStyle.height;
            Vector2 pivot = new(width * 0.5f, height * 0.5f);

            float topIndent = Mathf.Tan((180f - _topAngle) * 0.5f * Mathf.Deg2Rad) * height;

            Vector2[] vertices =
            {
                RotatePoint(new Vector2(0, height), pivot, -_rotationAngle),
                RotatePoint(new Vector2(topIndent, 0), pivot, -_rotationAngle),
                RotatePoint(new Vector2(width - topIndent, 0), pivot, -_rotationAngle),
                RotatePoint(new Vector2(width, height), pivot, -_rotationAngle)
            };

            bool inside = false;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                if ((vertices[i].y > localPoint.y) == vertices[j].y > localPoint.y) continue;

                float intersectX = (vertices[j].x - vertices[i].x) * (localPoint.y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x;
                if (localPoint.x < intersectX)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// Gets the local coordinates of the VisualElement.
        /// </summary>
        /// <returns>The local coordinates of the VisualElement.</returns>
        private Vector2 GetVisualElementLocalCoordinates()
        {
            Vector2 mousePosition = MousePositionNdc;
            Vector2 flippedPosition = new(mousePosition.x, 1 - mousePosition.y);
            Vector2 panelSize;
            try
            {
                panelSize = panel.visualTree.layout.size;
            }
            catch (System.Exception e)
            {
                return Vector2.zero;
            }

            Vector2 adjustedPosition = flippedPosition * panelSize;
            return this.WorldToLocal(adjustedPosition);
        }

        /// <summary>
        /// Gets the mouse position in normalized device coordinates (NDC).
        /// </summary>
        private static Vector2 MousePositionNdc
        {
            get
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                return new Vector2(mousePosition.x / Screen.width, mousePosition.y / Screen.height);
            }
        }
    }
}