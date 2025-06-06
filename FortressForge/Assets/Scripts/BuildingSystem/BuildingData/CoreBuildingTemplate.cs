using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    /// <summary>
    /// Represents the template for a core building, including its level and inherited building properties.
    /// </summary>
    [CreateAssetMenu(fileName = "New Core Building", menuName = "Buildings/CoreBuilding")]
    public class CoreBuildingTemplate : BaseBuildingTemplate
    {
        /// <summary>
        /// The level of the core building.
        /// </summary>
        [Header("Core Data")]
        public int Level;
    }
}