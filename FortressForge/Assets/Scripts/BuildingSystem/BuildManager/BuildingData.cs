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
    /// Contains all data and state related to a placed building, including its prefab, tiles, and template.
    /// Handles highlight and mouse interaction logic for the building and its tiles.
    /// </summary>
    public class BuildingData : ISelectableGameObjectData<BuildingData> {
        private readonly GameObject _buildingPrefab;
        private readonly List<HexTileData> _buildingTiles;
        private readonly BaseBuildingTemplate _baseBuildingTemplate;

        /// <summary>
        /// Gets the prefab associated with this building.
        /// </summary>
        public GameObject BuildingPrefab => _buildingPrefab;

        /// <summary>
        /// Gets the list of hex tiles occupied by this building.
        /// </summary>
        public List<HexTileData> BuildingTiles => _buildingTiles;

        /// <summary>
        /// Gets the building template used to create this building.
        /// </summary>
        public BaseBuildingTemplate BaseBuildingTemplate => _baseBuildingTemplate;
        
        /// <summary>
        /// Event triggered when the building data changes (e.g., highlight state).
        /// </summary>
        public event Action<BuildingData> OnChanged;

        /// <summary>
        /// Event triggered when the building is left-clicked.
        /// </summary>
        public event Action OnMouseLeftClick;
        
        private bool _isHighlighted;

        /// <summary>
        /// Gets or sets whether the building is highlighted.
        /// Setting this will propagate the highlight state to all building tiles and trigger OnChanged.
        /// </summary>
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

        /// <summary>
        /// Gets or sets whether the building is the current mouse target.
        /// Setting this triggers OnChanged.
        /// </summary>
        public bool IsMouseTarget
        {
            get => _isMouseTarget;
            set {
                if (_isMouseTarget == value) return;
                _isMouseTarget = value;
                OnChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingData"/> class.
        /// Subscribes to tile events and sets up event handlers.
        /// </summary>
        /// <param name="buildingPrefab">The prefab for this building.</param>
        /// <param name="buildingTiles">The tiles occupied by this building.</param>
        /// <param name="baseBuildingTemplate">The template used for this building.</param>
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
        
        /// <summary>
        /// Handles changes in tile data, propagating highlight state to all tiles and the building.
        /// </summary>
        /// <param name="tileData">The tile data that changed.</param>
        private void HandleTileDataChange(HexTileData tileData) {
            if (tileData.IsHighlighted) {
                _buildingTiles.ForEach(tile => tile.IsHighlighted = true);
                IsHighlighted = true;
            } else {
                _buildingTiles.ForEach(tile => tile.IsHighlighted = false);
                IsHighlighted = false;
            }
        }
        
        /// <summary>
        /// Handles changes in building data, propagating highlight state to all tiles.
        /// </summary>
        /// <param name="buildingData">The building data that changed.</param>
        private void HandleBuildingDataChange(BuildingData buildingData) {
            if (buildingData.IsHighlighted) {
                _buildingTiles.ForEach(tile => tile.IsHighlighted = true);
            } else {
                _buildingTiles.ForEach(tile => tile.IsHighlighted = false);
            }
        }

        /// <summary>
        /// Triggers the OnMouseLeftClick event for this building.
        /// </summary>
        public void TriggerMouseLeftClick() {
            OnMouseLeftClick?.Invoke();
        }
        
        /// <summary>
        /// Handles the mouse left click event for this building.
        /// </summary>
        public void HandleMouseLeftClick() {
            // Handle the mouse left click event here, gets triggered twice if on server
            Debug.Log("Building data clicked!");
        }
    }
}