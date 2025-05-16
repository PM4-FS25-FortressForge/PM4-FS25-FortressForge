using System;
using FortressForge.Economy;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.BuildingSystem.BuildingData
{
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
        public int rechargeCost;

        [Header("Weapon Control Constants")] 
        public float minCannonAngle;
        public float maxCannonAngle;
        public float rotationSpeed;
        public float pitchSpeed;
        public float cannonForce;
    }
}