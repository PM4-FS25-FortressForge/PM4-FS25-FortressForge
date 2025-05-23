using System;
using System.Collections;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests.BuildingSystem.BuildingManager
{
    public class PreviewControllerTests
    {
        private GameObject _root;
        private PreviewController _controller;
        private TestHexGridManager _gridManager;
        private TestHoverController _hoverController;
        private GameStartConfiguration _config;
        private GameSessionStartConfiguration _sessionConfig;
        private BaseBuildingTemplate _template;
        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            // create controller
            _root = new GameObject("TestRoot");
            _controller = _root.AddComponent<PreviewController>();

            // create and wire stubs
            _gridManager     = new GameObject("TestHexGridManager").AddComponent<TestHexGridManager>();
            _hoverController = new GameObject("TestHoverController").AddComponent<TestHoverController>();

            // simple config
            _sessionConfig = ScriptableObject.CreateInstance<GameSessionStartConfiguration>();
            _sessionConfig.HexGridOrigins = new List<Vector3> { };
            _config = ScriptableObject.CreateInstance<GameStartConfiguration>();
            _config.GridRadius = 1;
            _config.TileHeight = 1;
            _config.TileSize   = 1;

            _controller.Init(_config, _gridManager, _hoverController);
            _gridManager.InitializeHexGrids(_sessionConfig, _config);
            
            // build a one-tile template prefab
            _prefab = new GameObject("BuildingPrefab");
            var meshGO = new GameObject("Mesh"); 
            meshGO.AddComponent<MeshCollider>();
            meshGO.AddComponent<MeshRenderer>();
            meshGO.transform.SetParent(_prefab.transform);

            _template = ScriptableObject.CreateInstance<BaseBuildingTemplate>();
            _template.ShapeDataEntries = new List<HexTileEntry> { new HexTileEntry(new HexTileCoordinate(1, 1), true) };
            _template.BuildingPrefab = _prefab;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_gridManager.gameObject);
            Object.DestroyImmediate(_hoverController.gameObject);
            ScriptableObject.DestroyImmediate(_config);
            ScriptableObject.DestroyImmediate(_template);
            Object.DestroyImmediate(_prefab);
        }

        [UnityTest]
        public IEnumerator PreviewBuilding_CreatesPreviewObject()
        {
            _controller.PreviewBuilding(_template);

            // access the private _previewBuilding
            var preview = typeof(PreviewController)
                .GetField("_previewBuilding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_controller) as GameObject;
            Assert.IsNotNull(preview, "Preview object should be instantiated.");

            // mesh should be disabled until hover
            var collider = preview.GetComponentInChildren<Collider>();
            Assert.IsFalse(collider.enabled, "Collider should start disabled.");

            yield break;
        }

        [UnityTest]
        public IEnumerator Hovering_MarksTileAsBuildTarget()
        {
            _controller.PreviewBuilding(_template);

            // simulate hover on (0,0)
            var coord   = new HexTileCoordinate(1, 1);
            var tileData = _gridManager.GetHexTileDataOrCreate(coord);
            tileData.IsMouseTarget = true;
            _hoverController.ExposedInvokeHoverChanged(tileData);

            // wait a frame for MovePreviewObject to run
            yield return null;

            Assert.IsTrue(tileData.IsBuildTarget, "The hovered tile should be marked as build target.");

            // mesh should now be visible
            var preview = typeof(PreviewController)
                .GetField("_previewBuilding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_controller) as GameObject;
            var meshRenderer = preview.GetComponentInChildren<MeshRenderer>();
            Assert.IsTrue(meshRenderer.enabled, "MeshRenderer should be enabled after hover.");
        }

        [Test]
        public void RotatePreviewBuilding_UpdatesRotation()
        {
            _controller.PreviewBuilding(_template);

            _controller.RotatePreviewBuilding(90f);
                
            var preview = typeof(PreviewController)
                .GetField("_previewBuilding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_controller) as GameObject;
                
            // check Y rotation
            Assert.AreEqual(90f, preview.transform.rotation.eulerAngles.y, 0.1f);
        }
    }
    
    // --- stubs for dependencies ---
    class TestHexGridManager : HexGridManager
    {
        public new HexTileData GetHexTileDataOrCreate(HexTileCoordinate coord)
        {
            if (!IndependentGrid.TileMap.ContainsKey(coord))
                IndependentGrid.TileMap[coord] = new HexTileData(coord);
            return IndependentGrid.TileMap[coord];
        }
    }

    class TestHoverController : HexTileHoverController
    {
        public void ExposedInvokeHoverChanged(HexTileData tileData) => HoverTileChanged(tileData);
    }
}
