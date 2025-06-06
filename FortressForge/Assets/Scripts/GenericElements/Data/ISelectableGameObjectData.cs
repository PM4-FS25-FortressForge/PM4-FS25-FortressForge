using System;

namespace FortressForge.GenericElements.Data
{
    /// <summary>
    /// Interface for data associated with selectable game objects.
    /// Provides events and properties for selection, ownership, and interaction states.
    /// </summary>
    /// <typeparam name="T">The type of data associated with the selectable object.</typeparam>
    public interface ISelectableGameObjectData<out T>
    {
        /// <summary>
        /// Event triggered when the data changes.
        /// </summary>
        public event Action<T> OnChanged;

        /// <summary>
        /// Indicates if the object is a valid mouse target.
        /// </summary>
        bool IsMouseTarget
        {
            get => false;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates if the object is a valid build target.
        /// </summary>
        bool IsBuildTarget
        {
            get => false;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates if the object is currently occupied.
        /// </summary>
        bool IsOccupied
        {
            get => false;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates if the object is owned by the player.
        /// </summary>
        bool IsOwned
        {
            get => true;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates if the object is invisible.
        /// </summary>
        bool IsInvisible
        {
            get => false;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates if the object is currently highlighted.
        /// </summary>
        bool IsHighlighted
        {
            get => false;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Triggers logic for a left mouse click on the object.
        /// </summary>
        void TriggerMouseLeftClick();
    }
}