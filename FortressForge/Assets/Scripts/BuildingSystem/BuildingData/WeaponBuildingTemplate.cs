using System;
using FortressForge.Economy;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Buildings/WeaponBuilding")]
    public class WeaponBuildingTemplate : BaseBuildingTemplate
    {
        [Header("Weapon Data")] 
        public int baseDamage;
        public float reloadSpeed;
        
        [Header("Weapon Control Constants")]
        public float minCannonAngle = 10f;  
        public float maxCannonAngle = 90f; 
        public float rotationSpeed = 100f;
        public float pitchSpeed = 100f; 
    }
}