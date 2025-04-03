using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Buildings/WeaponBuilding")]
    public class WeaponBuildingTemplate : BaseBuildingTemplate
    {
        [Header("Weapon Data")] public int baseDamage;
        public float reloadSpeed;
    }
}