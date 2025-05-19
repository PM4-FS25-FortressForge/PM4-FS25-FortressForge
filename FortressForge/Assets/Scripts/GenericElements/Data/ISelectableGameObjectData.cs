using System;

namespace FortressForge.GenericElements.Data {
    public interface ISelectableGameObjectData<out T> {
        public event Action<T> OnChanged;
        bool IsMouseTarget {
            get => false;
            set => throw new NotImplementedException();
        }
        
        bool IsBuildTarget {
            get => false;
            set => throw new NotImplementedException();
        }
        
        bool IsOccupied {
            get => false;
            set => throw new NotImplementedException();
        }
        
        bool IsOwned {
            get => true;
            set => throw new NotImplementedException();
        }
        
        bool IsInvisible {
            get => false;
            set => throw new NotImplementedException();
        }
        
        bool IsHighlighted {
            get => false;
            set => throw new NotImplementedException();
        }

        void TriggerMouseLeftClick();
    }
}