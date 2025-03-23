using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Building", menuName = "RessourceBuilding")]
    public class ResourceBuilding : BaseBuilding
    {
        [Header("Ressource Data")] public RessourceType ressourceType;
        public int ressourcePerTick;
    }
}