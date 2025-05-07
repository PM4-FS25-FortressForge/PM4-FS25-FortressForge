using System;
using FortressForge.BuildingSystem.BuildManager;

namespace FortressForge.HexGrid.View {
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
    }
}