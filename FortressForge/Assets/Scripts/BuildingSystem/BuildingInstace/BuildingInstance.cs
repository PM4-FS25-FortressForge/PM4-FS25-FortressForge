using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingInstance
{
    public abstract class BuildingInstance : MonoBehaviour
    {
        protected BaseBuilding buildingData;

        public int CurrentHealth;
        public bool IsActive;

        public virtual void Initialize(BaseBuilding buildingData)
        {
            this.buildingData = buildingData;

            if (buildingData != null)
            {
                CurrentHealth = buildingData.maxHealth;
            }
        }
    }
}