using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using NUnit.Framework;
using UnityEngine;

namespace Tests.HexGrid {
    public class GlobalGridMeshRendererTests
    {
        private GameObject _gridRendererObject;
        private GlobalGridMeshRenderer _renderer;
        private MockTerrainHeightProvider _heightProvider;
        private GameStartConfiguration _config;

        [SetUp]
        public void SetUp()
        {
            _gridRendererObject = new GameObject("TestRenderer", typeof(MeshFilter), typeof(MeshRenderer), typeof(GlobalGridMeshRenderer));
            _renderer = _gridRendererObject.GetComponent<GlobalGridMeshRenderer>();
            _heightProvider = new MockTerrainHeightProvider();

            _config = ScriptableObject.CreateInstance<GameStartConfiguration>();
            _config.TileSize = 100f;
            _config.TileHeight = 100f;
            _config.MeshTilesMaterial = new Material(Shader.Find("Standard"));
            _config.Terrain = Terrain.CreateTerrainGameObject(new TerrainData { size = new Vector3(20, 10, 20) }).GetComponent<Terrain>();

            _renderer.Init(_heightProvider, _config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gridRendererObject);
            Object.DestroyImmediate(_config.Terrain.gameObject);
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Mesh_IsGenerated_WithVerticesAndIndices()
        {
            var mesh = _gridRendererObject.GetComponent<MeshFilter>().mesh;

            Assert.IsNotNull(mesh, "Mesh should not be null.");
            Assert.IsTrue(mesh.vertexCount > 0, "Mesh should contain vertices.");
            Assert.IsTrue(mesh.GetIndices(0).Length > 0, "Mesh should contain indices.");
        }

        private class MockTerrainHeightProvider : ITerrainHeightProvider
        {
            public float SampleHexHeight(Vector3 center, float tileHeight, float tileRadius) => 0f;
            public HexTileCoordinate GetHexTileCoordinate(Vector3 position, float tileHeight, float tileRadius) {
                throw new System.NotImplementedException();
            }
            public float SampleHeight(Vector3 position) => 0f;
        }
    }
}