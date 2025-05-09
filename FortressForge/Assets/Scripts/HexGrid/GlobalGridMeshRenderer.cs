using System.Collections.Generic;
using UnityEngine;
using FortressForge.GameInitialization;
using FortressForge.HexGrid.Data;

namespace FortressForge.HexGrid
{
    /// <summary>
    /// Generates and renders a static hexagonal grid overlay using mesh lines.
    /// This class builds a single mesh using MeshTopology.Lines and places each hex on the terrain
    /// based on height sampling. The result is a non-interactive, efficient wireframe grid,
    /// ideal for visualizing tile placement, debug overlays, or strategy maps.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GlobalGridMeshRenderer : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private ITerrainHeightProvider _terrainHeightProvider;
        private GameStartConfiguration _gameStartConfiguration;

        /// <summary>
        /// Initializes the renderer and builds the hex grid mesh.
        /// Should be called after GameStartConfiguration is set up.
        /// </summary>
        /// <param name="terrainHeightProvider">Provides terrain height sampling per tile.</param>
        /// <param name="gameStartConfiguration">Startup config containing tile size, terrain, and material.</param>
        public void Init(ITerrainHeightProvider terrainHeightProvider, GameStartConfiguration gameStartConfiguration)
        {
            _terrainHeightProvider = terrainHeightProvider;
            _gameStartConfiguration = gameStartConfiguration;

            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.material = _gameStartConfiguration.MeshTilesMaterial;

            BakeHexEdgeMesh();
        }

        /// <summary>
        /// Builds a mesh containing the wireframe edges of all valid hex tiles on the terrain.
        /// Tiles are adjusted to match terrain height using the height provider.
        /// </summary>
        private void BakeHexEdgeMesh()
        {
            float tileRadius = _gameStartConfiguration.TileSize;
            float tileHeight = _gameStartConfiguration.TileHeight;
            var terrainSize = _gameStartConfiguration.Terrain.terrainData.size;
            float maxX = terrainSize.x;
            float maxZ = terrainSize.z;

            // Precompute unit circle directions for hex corners
            Vector3[] hexDirs = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (60f * i);
                hexDirs[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            }

            // Accumulators for building a single wireframe mesh
            List<Vector3> allVertices = new();
            List<int> allIndices = new();
            int vertexOffset = 0;

            // Estimate the grid bounds in axial coordinate space
            float hexWidth = Mathf.Sqrt(3f) * tileRadius;
            float hexHeight = 2f * tileRadius;
            float vertStep = 0.75f * hexHeight;

            int maxQ = Mathf.CeilToInt(maxX / hexWidth);
            int maxR = Mathf.CeilToInt(maxZ / vertStep);

            // Iterate all hex coordinates within estimated bounds
            for (int r = -maxR; r <= maxR; r++)
            {
                for (int q = -maxQ; q <= maxQ; q++)
                {
                    var coord = new HexTileCoordinate(q, r, 0);
                    Vector3 basePos = coord.GetWorldPosition(tileRadius, tileHeight);

                    // Sample terrain height at the tile center
                    basePos.y = _terrainHeightProvider.SampleHexHeight(basePos, tileHeight, tileRadius);

                    // Clip tiles outside terrain bounds
                    if (basePos.x < 0 || basePos.x > maxX || basePos.z < 0 || basePos.z > maxZ)
                        continue;

                    // Generate edges for each side of the hex tile
                    for (int i = 0; i < 6; i++)
                    {
                        Vector3 v0 = basePos + hexDirs[i] * tileRadius;
                        Vector3 v1 = basePos + hexDirs[(i + 1) % 6] * tileRadius;

                        allVertices.Add(v0);
                        allVertices.Add(v1);
                        allIndices.Add(vertexOffset++);
                        allIndices.Add(vertexOffset++);
                    }
                }
            }

            // If no tiles were generated, print a warning
            if (allVertices.Count == 0)
            {
                Debug.LogWarning("No hex edges generated. Check tile size or terrain bounds.");
                return;
            }

            // Create and assign the mesh
            var mesh = new Mesh
            {
                name = "HexEdgeGrid",
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 // Support >65k vertices
            };

            mesh.SetVertices(allVertices);
            mesh.SetIndices(allIndices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();

            _meshFilter.mesh = mesh;
            gameObject.isStatic = true; // Enable static batching (optional)
        }
    }
}
