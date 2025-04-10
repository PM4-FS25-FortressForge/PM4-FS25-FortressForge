using System;
using FortressForge.Economy;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Buildings/WeaponBuilding")]
    public class WeaponBuildingTemplate : BaseBuildingTemplate
    {
        [Header("Weapon Data")] public int baseDamage;
        public float reloadSpeed;
        
        public float resourceCost;
        public ResourceType resourceType;
        
        public void Awake()
        {
            _resourceChange[resourceType] = resourceCost;
        }
    }
}