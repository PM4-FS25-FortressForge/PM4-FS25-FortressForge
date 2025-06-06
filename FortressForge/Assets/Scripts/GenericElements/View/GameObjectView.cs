using FortressForge.GameInitialization;
using FortressForge.GenericElements.Data;
using FortressForge.GenericElements.View;
using FortressForge.UI;
using UnityEngine;

namespace FortressForge.GenericElements.View
{
    /// <summary>
    /// Represents the visual representation of a HexTile or similar selectable game object.
    /// Handles material changes and mouse interactions based on the underlying data.
    /// </summary>
    public class GameObjectView<T> : MonoBehaviour where T : ISelectableGameObjectData<T>
    {
        private T _data;
        private GameStartConfiguration _config;
        private MeshRenderer _renderer;
        private Material _originalMaterial;

        /// <summary>
        /// Initializes the view with the given data and configuration.
        /// Subscribes to data changes and sets up the renderer.
        /// </summary>
        /// <param name="data">The data object representing the game element.</param>
        /// <param name="config">The configuration for materials and visuals.</param>
        public void Init(T data, GameStartConfiguration config)
        {
            _config = config;
            _data = data;
            _data.OnChanged += UpdateVisuals;
            _renderer = GetComponentInChildren<MeshRenderer>();
            _originalMaterial = _renderer.material;
            UpdateVisuals(data);
        }
        
        /// <summary>
        /// Unsubscribes from the data change event when destroyed to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            if (!Equals(_data, default(T)))
            {
                _data.OnChanged -= UpdateVisuals;
            }
        }

        /// <summary>
        /// Updates the material and visibility of the renderer based on the data's state.
        /// </summary>
        /// <param name="data">The current data state.</param>
        private void UpdateVisuals(T data)
        { 
            _renderer.enabled = true;
            if ((data.IsBuildTarget && data.IsOccupied) || (data.IsBuildTarget && !data.IsOwned))
                _renderer.material = _config.NotAllowedMaterial;
            else if (data.IsBuildTarget) 
                _renderer.material = _config.PreviewMaterial;
            else if (data.IsHighlighted)
                _renderer.material = _config.HighlightMaterial;
            else if (data.IsInvisible)
                _renderer.enabled = false;
            else if (!data.IsOwned)
                _renderer.material = _config.UnownedHexMaterial;
            else if (data.IsOccupied)
                _renderer.material = _config.OccupiedMaterial;
            else
                _renderer.material = _originalMaterial;
        }

        /// <summary>
        /// Called when the mouse enters the collider.
        /// Sets the data as a mouse target and highlights it.
        /// </summary>
        private void OnMouseEnter()
        {
            _data.IsMouseTarget = true;
            _data.IsHighlighted = true;
        }
        
        /// <summary>
        /// Called when the mouse exits the collider.
        /// Removes mouse target and highlight states.
        /// </summary>
        private void OnMouseExit()
        {
            _data.IsMouseTarget = false;
            _data.IsHighlighted = false;
        }
        
        /// <summary>
        /// Called every frame while the mouse is over the collider.
        /// Handles overlay checks to update highlight and mouse target states.
        /// </summary>
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

        /// <summary>
        /// Called when the mouse is pressed down on the collider.
        /// Triggers a left click action on the data if not over a UI overlay.
        /// </summary>
        private void OnMouseDown() {
            if (UIClickChecker.Instance.IsMouseOnOverlay())
                return;

            if (Input.GetMouseButtonDown(0)) {
                _data.TriggerMouseLeftClick();
            }
        }
    }
}