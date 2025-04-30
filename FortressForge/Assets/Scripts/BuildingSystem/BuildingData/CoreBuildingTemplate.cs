using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Core Building", menuName = "Buildings/CoreBuilding")]
    public class CoreBuildingTemplate : BaseBuildingTemplate
    {
        [Header("Core Data")]
        public int Level;
    }
}