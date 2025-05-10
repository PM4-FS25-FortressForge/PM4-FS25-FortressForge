using System;
using FortressForge.Economy;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Factory", menuName = "Buildings/FactoryBuilding")]
    public class FactoryBuildingTemplate : BaseBuildingTemplate
    {
        [Header("Factory Data")]
        public int MagmaPointsCost;
    }
}