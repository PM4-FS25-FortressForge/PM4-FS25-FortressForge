using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingInstance {
    public abstract class BuildingInstance : MonoBehaviour {
        protected BaseBuilding buildingData;

        protected int currentHealth;
        protected bool isActive;

        public virtual void Initialize(BaseBuilding buildingData) {
            this.buildingData = buildingData;

            if (buildingData != null) {
                currentHealth = buildingData.maxHealth;
            }
        }

        void Update() {
        }
    }
}