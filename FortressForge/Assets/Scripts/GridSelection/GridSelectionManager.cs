using System;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;
using UnityEngine;

namespace FortressForge.GridSelection
{
    public class GridSelectionManager : MonoBehaviour
    {
        [Header("Game Start Configuration")]
        [SerializeField] private GameStartConfiguration _gameStartConfiguration;
        
        private BaseBuildingTemplate _coreBuilding;

        private void Awake()
        {
            Instantiate(_gameStartConfiguration.Terrain);
            _coreBuilding = _gameStartConfiguration.coreBuilding;
            gameObject.AddComponent<HexGridManager>();
        }

        private void Start()
        {
            Vector3 terrainOrigin = _gameStartConfiguration.Terrain.transform.position;
            int terrainWidth = Mathf.CeilToInt(_gameStartConfiguration.Terrain.terrainData.size.x / (2 * _gameStartConfiguration.TileSize));
            int terrainLength = Mathf.CeilToInt(_gameStartConfiguration.Terrain.terrainData.size.z / (2 * _gameStartConfiguration.TileSize));
            
            HexGridData terrainGridData = new HexGridData(
                0,
                terrainOrigin,
                terrainWidth,
                terrainLength,
                _gameStartConfiguration.TileSize,
                _gameStartConfiguration.TileHeight,
                new TerrainHeightProvider()
            );
            
            HexGridView hexGridView = new GameObject("HexGridView_" + terrainGridData.Id)
                .AddComponent<HexGridView>();
            hexGridView.transform.SetParent(transform);
            hexGridView.Initialize(_gameStartConfiguration.TilePrefab, terrainGridData, _gameStartConfiguration);
            
            List<HexTileView> validTiles = new List<HexTileView>();
            
            // Iterate over every tile
            foreach (var kvp in terrainGridData.TileMap)
            {
                var coords = kvp.Key;
                var data = kvp.Value;
                
                HexTileView tileView = hexGridView.GetTileView(coords);
                tileView.GetComponent<MeshRenderer>().enabled = false;
                
                bool isValid = terrainGridData.ValidateBuildingPlacement(coords, _gameStartConfiguration.coreBuilding.ShapeData);

                if (isValid)
                {
                    validTiles.Add(tileView);
                }
            }
            
            // Liste zum Kombinieren
            List<CombineInstance> combineInstances = new List<CombineInstance>();
            Material sharedMaterial = null;

            foreach (HexTileView tile in validTiles)
            {
                MeshFilter mf = tile.GetComponent<MeshFilter>();
                MeshRenderer mr = tile.GetComponent<MeshRenderer>();
    
                if (mf == null || mr == null) continue;

                CombineInstance ci = new CombineInstance();
                ci.mesh = mf.sharedMesh;
                ci.transform = mf.transform.localToWorldMatrix;
                combineInstances.Add(ci);

                if (sharedMaterial == null)
                    sharedMaterial = mr.sharedMaterial;
            }

            // Kombiniertes Mesh erzeugen
            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Bei vielen Tiles
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

            // Neues GameObject für das kombinierte Mesh
            GameObject combinedObj = new GameObject("InvalidTilesCombined");
            combinedObj.transform.position = Vector3.zero;
            MeshFilter filter = combinedObj.AddComponent<MeshFilter>();
            filter.sharedMesh = combinedMesh;

            MeshRenderer renderer = combinedObj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = sharedMaterial;
            renderer.material = _gameStartConfiguration.FreeMaterial;

            Destroy(hexGridView);
        }
    }
}
