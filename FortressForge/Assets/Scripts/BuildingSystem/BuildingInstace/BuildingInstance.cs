using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingInstance
{
    public abstract class BuildingInstance : MonoBehaviour
    {
        protected BaseBuildingTemplate BuildingTemplate;

        public int CurrentHealth;
        public bool IsActive;

        public virtual void Initialize(BaseBuildingTemplate buildingTemplateTemplate)
        {
            BuildingTemplate = buildingTemplateTemplate;

            if (buildingTemplateTemplate != null)
            {
                CurrentHealth = buildingTemplateTemplate.maxHealth;
            }
        }
    }
}