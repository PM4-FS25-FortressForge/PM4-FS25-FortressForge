using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Buildings/ResourceBuilding")]
    public class ResourceBuildingTemplate : BaseBuildingTemplate
    {
        [Header("Resource Data")] public RessourceType ressourceType;
        public int ressourcePerTick;
    }
}