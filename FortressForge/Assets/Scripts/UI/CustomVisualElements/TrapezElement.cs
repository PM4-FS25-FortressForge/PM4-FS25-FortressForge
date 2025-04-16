using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FortressForge.UI
{
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

        public Texture2D m_Texture;

        private readonly Vector2[] _rotatedVerts = new Vector2[4];

        public TrapezElement()
        {
            generateVisualContent += OnGenerateVisualContent;
            pickingMode = PickingMode.Position;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            // m_Texture = new Texture2D(20, 200);
            // make a random pattern
            // for (int y = 0; y < m_Texture.height; y++)
            // {
            //     for (int x = 0; x < m_Texture.width; x++)
            //     {
            //         m_Texture.SetPixel(x, y, new Color(Random.value, Random.value, Random.value));
            //         Debug.Log("SetPixel: " + x + " " + y + " " + m_Texture.GetPixel(x, y));
            //     }
            // }
            //chessboard black and white
            // for (int y = 0; y < m_Texture.height; y++)
            // {
            //     for (int x = 0; x < m_Texture.width; x++)
            //     {
            //         if ((x + y) % 4 == 0)
            //             m_Texture.SetPixel(x, y, Color.white);
            //         else
            //             m_Texture.SetPixel(x, y, Color.black);
            //     }
            // }
            // m_Texture.Apply();

            //You can also load a texture from a file.
            //m_Texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/tex.png");
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_Texture != null) m_Texture.Apply();
            MarkDirtyRepaint();
        }

        public void SetParameters(float topAngle, float rotation, string selector = "")
        {
            _topAngle = Mathf.Clamp(topAngle, 1f, 179f);
            _rotationAngle = rotation % 360f;

            if (!string.IsNullOrEmpty(selector))
                AddToClassList(selector);

            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            float width = resolvedStyle.width;
            float height = resolvedStyle.height;

            float topIndent = Mathf.Tan((180f - _topAngle) * 0.5f * Mathf.Deg2Rad) * height;

            Vector2[] baseVertices =
            {
                new(0, height), // 0: Bottom Left
                new(topIndent, 0), // 1: Top Left
                new(width - topIndent, 0), // 2: Top Right
                new(width, height) // 3: Bottom Right
            };

            Vector2 pivot = new(width * 0.5f, height * 0.5f);

            // Trapez-Fläche
            Vertex[] vertices = new Vertex[4];
            Vector2[] uvs = { new(0, 1), new(0, 0), new(1, 0), new(1, 1) }; // Proper UV mapping
            for (int i = 0; i < 4; i++)
            {
                Vector2 rotated = RotatePoint(baseVertices[i], pivot, _rotationAngle);
                _rotatedVerts[i] = rotated;

                vertices[i].position = new Vector3(rotated.x, rotated.y, Vertex.nearZ);
                vertices[i].tint = resolvedStyle.color;
                vertices[i].uv = uvs[i];
            }

            MeshWriteData mesh = mgc.Allocate(vertices.Length, K_INDICES.Length, m_Texture);
            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(K_INDICES);

            // if the _rotationAngle is over 180 degrees, we need to switch the border colors
            if (_rotationAngle >= 180f)
            {
                _colorLeft = resolvedStyle.borderRightColor;
                _colorTop = resolvedStyle.borderBottomColor;
                _colorRight = resolvedStyle.borderLeftColor;
                _colorBottom = resolvedStyle.borderTopColor;

                if (resolvedStyle.borderLeftWidth != 0) _leftBorderWidth = resolvedStyle.borderRightWidth;
                if (resolvedStyle.borderTopWidth != 0) _topBorderWidth = resolvedStyle.borderBottomWidth;
                if (resolvedStyle.borderRightWidth != 0) _rightBorderWidth = resolvedStyle.borderLeftWidth;
                if (resolvedStyle.borderBottomWidth != 0) _bottomBorderWidth = resolvedStyle.borderTopWidth;
            }
            else
            {
                _colorLeft = resolvedStyle.borderLeftColor;
                _colorTop = resolvedStyle.borderTopColor;
                _colorRight = resolvedStyle.borderRightColor;
                _colorBottom = resolvedStyle.borderBottomColor;

                if (resolvedStyle.borderLeftWidth != 0) _leftBorderWidth = resolvedStyle.borderLeftWidth;
                if (resolvedStyle.borderTopWidth != 0) _topBorderWidth = resolvedStyle.borderTopWidth;
                if (resolvedStyle.borderRightWidth != 0) _rightBorderWidth = resolvedStyle.borderRightWidth;
                if (resolvedStyle.borderBottomWidth != 0) _bottomBorderWidth = resolvedStyle.borderBottomWidth;
            }

            float brightnessBoost = 1.14f;
            if (_colorLeft.a == 0) _colorLeft = BrightenColor(resolvedStyle.color, brightnessBoost);
            if (_colorTop.a == 0) _colorTop = BrightenColor(resolvedStyle.color, brightnessBoost);
            if (_colorRight.a == 0) _colorRight = BrightenColor(resolvedStyle.color, brightnessBoost);
            if (_colorBottom.a == 0) _colorBottom = BrightenColor(resolvedStyle.color, brightnessBoost);
            DrawLine(mgc, _rotatedVerts[0], _rotatedVerts[1], _colorLeft, _leftBorderWidth);
            DrawLine(mgc, _rotatedVerts[1], _rotatedVerts[2], _colorTop, _topBorderWidth);
            DrawLine(mgc, _rotatedVerts[2], _rotatedVerts[3], _colorRight, _rightBorderWidth);
            DrawLine(mgc, _rotatedVerts[3], _rotatedVerts[0], _colorBottom, _bottomBorderWidth);
        }

        private void OnGenerateVisualContentTextured(MeshGenerationContext mgc)
        {
            float width = resolvedStyle.width;
            float height = resolvedStyle.height;

            float topIndent = Mathf.Tan((180f - _topAngle) * 0.5f * Mathf.Deg2Rad) * height;

            Vector2[] baseVertices =
            {
                new(0, height), // 0: Bottom Left
                new(topIndent, 0), // 1: Top Left
                new(width - topIndent, 0), // 2: Top Right
                new(width, height) // 3: Bottom Right
            };

            Vector2 pivot = new(width * 0.5f, height * 0.5f);

            // Create a temporary texture if none is assigned
            if (m_Texture == null)
            {
                m_Texture = new Texture2D((int)resolvedStyle.width, (int)resolvedStyle.height);
                Color baseColor = resolvedStyle.color;

                // First pass: identify dark spot centers
                bool[,] darkSpots = new bool[m_Texture.width, m_Texture.height];
                for (int y = 0; y < m_Texture.height; y++)
                {
                    for (int x = 0; x < m_Texture.width; x++)
                    {
                        // 5% chance to be a dark spot center
                        if (Random.value < 0.0003f)
                        {
                            // Mark this pixel and surrounding area as dark
                            int spotSize = Random.Range(1, 4); // Random spot size between 2-4 pixels radius
                            for (int dy = -spotSize; dy <= spotSize; dy++)
                            {
                                for (int dx = -spotSize; dx <= spotSize; dx++)
                                {
                                    int nx = x + dx;
                                    int ny = y + dy;
                                    if (nx >= 0 && nx < m_Texture.width && ny >= 0 && ny < m_Texture.height)
                                    {
                                        // Simple circular check (dx² + dy² <= r²)
                                        if (dx * dx + dy * dy <= spotSize * spotSize)
                                        {
                                            darkSpots[nx, ny] = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Second pass: apply colors
                for (int y = 0; y < m_Texture.height; y++)
                {
                    for (int x = 0; x < m_Texture.width; x++)
                    {
                        Color pixelColor = baseColor;

                        if (darkSpots[x, y])
                        {
                            // Dark spot - apply random darkness
                            float darkAmount = Random.Range(0.6f, 1.1f);
                            pixelColor.r *= darkAmount;
                            pixelColor.g *= darkAmount;
                            pixelColor.b *= darkAmount;
                        }

                        m_Texture.SetPixel(x, y, pixelColor);
                    }
                }

                m_Texture.Apply();
            }

            // Trapez-Fläche
            Vertex[] vertices = new Vertex[4];
            Vector2[] uvs = { new(0, 1), new(0, 0), new(1, 0), new(1, 1) }; // Proper UV mapping
            for (int i = 0; i < 4; i++)
            {
                Vector2 rotated = RotatePoint(baseVertices[i], pivot, _rotationAngle);
                _rotatedVerts[i] = rotated;

                vertices[i].position = new Vector3(rotated.x, rotated.y, Vertex.nearZ);
                vertices[i].tint = Color.white; // Use white tint to preserve texture colors
                vertices[i].uv = uvs[i];
            }

            MeshWriteData mesh = mgc.Allocate(vertices.Length, K_INDICES.Length, m_Texture);
            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(K_INDICES);

            // if the _rotationAngle is over 180 degrees, we need to switch the border colors
            if (_rotationAngle >= 180f)
            {
                _colorLeft = resolvedStyle.borderRightColor;
                _colorTop = resolvedStyle.borderBottomColor;
                _colorRight = resolvedStyle.borderLeftColor;
                _colorBottom = resolvedStyle.borderTopColor;
            }
            else
            {
                _colorLeft = resolvedStyle.borderLeftColor;
                _colorTop = resolvedStyle.borderTopColor;
                _colorRight = resolvedStyle.borderRightColor;
                _colorBottom = resolvedStyle.borderBottomColor;
            }

            // Make edges brighter by increasing their brightness
            float brightnessBoost = 4f;
            DrawLine(mgc, _rotatedVerts[0], _rotatedVerts[1], BrightenColor(_colorLeft, brightnessBoost), 10f);
            DrawLine(mgc, _rotatedVerts[1], _rotatedVerts[2], BrightenColor(_colorTop, brightnessBoost), 10f);
            DrawLine(mgc, _rotatedVerts[2], _rotatedVerts[3], BrightenColor(_colorRight, brightnessBoost), 10f);
            DrawLine(mgc, _rotatedVerts[3], _rotatedVerts[0], BrightenColor(_colorBottom, brightnessBoost), 10f);
        }

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
            MeshWriteData mesh = mgc.Allocate(verts.Length, indices.Length, m_Texture);
            mesh.SetAllVertices(verts);
            mesh.SetAllIndices(indices);
        }

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

        public bool IsPointInTrapez(Vector2 localPoint)
        {
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
                if ((vertices[i].y > localPoint.y) != (vertices[j].y > localPoint.y))
                {
                    float intersectX = (vertices[j].x - vertices[i].x) * (localPoint.y - vertices[i].y)
                        / (vertices[j].y - vertices[i].y) + vertices[i].x;
                    if (localPoint.x < intersectX)
                        inside = !inside;
                }
            }

            return inside;
        }
        public bool IsPointInTrapez2(Vector2 localPoint)
        {
            // Verwende die bereits rotierten Eckpunkte
            Vector2[] vertices = _rotatedVerts;

            bool inside = false;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                if ((vertices[i].y > localPoint.y) != (vertices[j].y > localPoint.y))
                {
                    float intersectX = (vertices[j].x - vertices[i].x) * (localPoint.y - vertices[i].y)
                        / (vertices[j].y - vertices[i].y) + vertices[i].x;
                    if (localPoint.x < intersectX)
                        inside = !inside;
                }
            }

            return inside;
        }
        
        public bool IsPointInTrapez3(Vector2 localPoint)
        {
            float width = resolvedStyle.width;
            float height = resolvedStyle.height;
            Vector2 pivot = new(width * 0.5f, height * 0.5f);

            // Punkt rückwärts rotieren
            Vector2 unrotatedPoint = RotatePoint(localPoint, pivot, -_rotationAngle);

            float topIndent = Mathf.Tan((180f - _topAngle) * 0.5f * Mathf.Deg2Rad) * height;

            Vector2[] vertices =
            {
                new Vector2(0, height),                  // Bottom Left
                new Vector2(topIndent, 0),               // Top Left
                new Vector2(width - topIndent, 0),       // Top Right
                new Vector2(width, height)               // Bottom Right
            };

            // Klassischer Point-in-Polygon-Test (Raycasting-Methode)
            bool inside = false;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                if ((vertices[i].y > unrotatedPoint.y) != (vertices[j].y > unrotatedPoint.y))
                {
                    float intersectX = (vertices[j].x - vertices[i].x) * (unrotatedPoint.y - vertices[i].y)
                        / (vertices[j].y - vertices[i].y) + vertices[i].x;
                    if (unrotatedPoint.x < intersectX)
                        inside = !inside;
                }
            }

            return inside;
        }

    }
}