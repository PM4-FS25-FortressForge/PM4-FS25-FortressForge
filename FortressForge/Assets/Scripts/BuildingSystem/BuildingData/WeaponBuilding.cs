using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Buildings/WeaponBuilding")]
    public class WeaponBuilding : BaseBuilding
    {
        [Header("Weapon Data")] public int baseDamage;
        public float reloadSpeed;
    }
}