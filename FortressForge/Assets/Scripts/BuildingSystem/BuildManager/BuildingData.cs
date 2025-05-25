using System;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.GenericElements.Data;
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
        private readonly GameObject _buildingPrefab;
        private readonly List<HexTileData> _buildingTiles;
        private readonly BaseBuildingTemplate _baseBuildingTemplate;
        public GameObject BuildingPrefab => _buildingPrefab;
        public List<HexTileData> BuildingTiles => _buildingTiles;
        public BaseBuildingTemplate BaseBuildingTemplate => _baseBuildingTemplate;
        
        public event Action<BuildingData> OnChanged;
        public event Action OnMouseLeftClick;
        
        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set {
                if (_isHighlighted == value) return;
                
                _isHighlighted = value;
                OnChanged?.Invoke(this);
            }
        }
        
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
                tile.OnMouseLeftClick += HandleMouseLeftClick;
            });
            
            OnChanged += HandleBuildingDataChange;
            OnMouseLeftClick += HandleMouseLeftClick;
        }
        
        private void HandleTileDataChange(HexTileData tileData) {
            // Handle the change in tile data here
            if (tileData.IsHighlighted) {
                _buildingTiles.ForEach(tile => {
                    tile.IsHighlighted = true;
                });
                IsHighlighted = true;
            } else {
                _buildingTiles.ForEach(tile => {
                    tile.IsHighlighted = false;
                });
                IsHighlighted = false;
            }
        }
        
        private void HandleBuildingDataChange(BuildingData buildingData) {
            // Handle the change in building data here
            if (buildingData.IsHighlighted) {
                _buildingTiles.ForEach(tile => {
                    tile.IsHighlighted = true;
                });
            } else {
                _buildingTiles.ForEach(tile => {
                    tile.IsHighlighted = false;
                });
            }
        }

        public void TriggerMouseLeftClick() {
            OnMouseLeftClick?.Invoke();
        }
        
        public void HandleMouseLeftClick() {
            // Handle the mouse left click event here, gets triggered twice if on server
            Debug.Log("Building data clicked!");
        }
    }
}