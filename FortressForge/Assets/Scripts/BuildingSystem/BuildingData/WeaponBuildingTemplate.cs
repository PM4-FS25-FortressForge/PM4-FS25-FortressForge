using System;
using FortressForge.Economy;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.HexGrid.BuildingData
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Buildings/WeaponBuilding")]
    public class WeaponBuildingTemplate : BaseBuildingTemplate
    {
        [Header("Weapon Data")] public int baseDamage;
        public float reloadSpeed;
    }
}