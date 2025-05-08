using System;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;
using NUnit.Framework;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildManager {
    /// <summary>
    /// Class to contain all data related to a build building.
    /// </summary>
    public class BuildingData : ISelectableGameObjectData<BuildingData> {
        private GameObject _buildingPrefab;
        private List<HexTileData> _buildingTiles = new();
        private BaseBuildingTemplate _baseBuildingTemplate;
        public GameObject BuildingPrefab => _buildingPrefab;
        public List<HexTileData> BuildingTiles => _buildingTiles;
        public BaseBuildingTemplate BaseBuildingTemplate => _baseBuildingTemplate;
        
        public event Action<BuildingData> OnChanged;
        
        private bool _isMouseTarget;
        public bool IsMouseTarget
        {
            get => _isMouseTarget;
            set {
                if (_isMouseTarget == value) return;
                
                _isMouseTarget = value;
                OnChanged?.Invoke(this);
            }
        }

        public BuildingData(GameObject buildingPrefab, List<HexTileData> buildingTiles, BaseBuildingTemplate baseBuildingTemplate) {
            _buildingPrefab = buildingPrefab;
            _buildingTiles = buildingTiles;
            _baseBuildingTemplate = baseBuildingTemplate;

            _buildingTiles.ForEach(tile => {
                tile.OnChanged += HandleTileDataChange;
            });
            OnChanged += HandleBuildingDataChange;
        }
        
        private void HandleTileDataChange(HexTileData tileData) {
            // Handle the change in tile data here
            if (tileData.IsMouseTarget) {
                _buildingTiles.ForEach(tile => {
                    tile.IsMouseTarget = true;
                });
                IsMouseTarget = true;
            } else {
                _buildingTiles.ForEach(tile => {
                    tile.IsMouseTarget = false;
                });
                IsMouseTarget = false;
            }
        }
        
        private void HandleBuildingDataChange(BuildingData buildingData) {
            // Handle the change in building data here
            if (buildingData.IsMouseTarget) {
                _buildingTiles.ForEach(tile => {
                    tile.IsMouseTarget = true;
                });
            } else {
                _buildingTiles.ForEach(tile => {
                    tile.IsMouseTarget = false;
                });
            }
        }
    }
}