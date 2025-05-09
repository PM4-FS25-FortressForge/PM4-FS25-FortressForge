using System;
using FortressForge.GameInitialization;
using FortressForge.HexGrid.Data;
using FortressForge.UI;
using UnityEngine;

namespace FortressForge.HexGrid.View
{
    /// <summary>
    /// Represents the visual representation of a HexTile.
    /// </summary>
    public class GameObjectView<T> : MonoBehaviour where T : ISelectableGameObjectData<T>
    {
        private T _data;
        private GameStartConfiguration _config;
        private MeshRenderer _renderer;
        private Material _originalMaterial;

        /// <summary>
        /// Initializes the HexTileView with the given HexTileData.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config"></param>
        public void Init(T data, GameStartConfiguration config)
        {
            _config = config;
            _data = data;
            _data.OnChanged += UpdateVisuals;
            _renderer = GetComponentInChildren<MeshRenderer>();
            _originalMaterial = _renderer.material;
            UpdateVisuals(data);
        }
        
        private void OnDestroy()
        {
            if (!Equals(_data, default(T)))
            {
                _data.OnChanged -= UpdateVisuals;
            }
        }

        /// <summary>
        /// Changes the material of the HexTileView based on the IsOccupied property of the HexTileData.
        /// </summary>
        private void UpdateVisuals(T data)
        { // TODO maybe add custom colors for overlapping events such as build target and occupied
            if ((data.IsBuildTarget && data.IsOccupied) || (data.IsBuildTarget && !data.IsOwned))
                _renderer.material = _config.NotAllowedMaterial;
            else if (!data.IsOwned)
                _renderer.material = _config.UnownedHexMaterial;
            else if (data.IsBuildTarget) 
                _renderer.material = _config.PreviewMaterial;
            else if (data.IsMouseTarget)
                _renderer.material = _config.HighlightMaterial;
            else if (data.IsOccupied)
                _renderer.material = _config.OccupiedMaterial;
            else
                _renderer.material = _originalMaterial;
        }

        private void OnMouseEnter()
        {
            _data.IsMouseTarget = true;
        }
        
        private void OnMouseExit()
        {
            _data.IsMouseTarget = false;
        }
        
        private void OnMouseOver()
        {
            if (UIClickChecker.Instance.IsMouseOnOverlay() && _data.IsMouseTarget)
            {
                OnMouseExit();
            }
            else if (!UIClickChecker.Instance.IsMouseOnOverlay() && !_data.IsMouseTarget)
            {
                OnMouseEnter();
            }
        }
    }
}