using System;
using FortressForge.Economy;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.BuildingSystem.BuildingData
{
    /// <summary>
    /// Represents the template for a weapon building, including weapon stats, ammunition, and control constants.
    /// Inherits all properties and methods from <see cref="BaseBuildingTemplate"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "New Building", menuName = "Buildings/WeaponBuilding")]
    public class WeaponBuildingTemplate : BaseBuildingTemplate
    {
        [SerializeField] 
        public GameObject ammunitionPrefab;

        [Header("Weapon Data")] 
        public int baseDamage;
        public float automaticReloadSpeed;
        public int weaponReload;
        public int maxAmmo;
        public int reloadCost;

        [Header("Weapon Control Constants")] 
        public float minCannonAngle;
        public float maxCannonAngle;
        public float rotationSpeed;
        public float pitchSpeed;
        public float cannonForce;
    }
}