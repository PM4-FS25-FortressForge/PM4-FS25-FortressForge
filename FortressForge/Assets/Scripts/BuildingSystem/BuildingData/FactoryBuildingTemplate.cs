using System;
using FortressForge.Economy;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.BuildingSystem.BuildingData
{
    /// <summary>
    /// Represents the template for a factory building, including its magma points cost and inherited building properties.
    /// </summary>
    [CreateAssetMenu(fileName = "New Factory", menuName = "Buildings/FactoryBuilding")]
    public class FactoryBuildingTemplate : BaseBuildingTemplate
    {
        /// <summary>
        /// The magma points cost required to build this factory.
        /// </summary>
        [Header("Factory Data")]
        public int MagmaPointsCost;
    }
}