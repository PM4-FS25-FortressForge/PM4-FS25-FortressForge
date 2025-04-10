using System.Collections.Generic;
using FortressForge.Economy;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Buildings/ResourceBuilding")]
    public class ResourceBuildingTemplate : BaseBuildingTemplate
    {
        [Header("Resource Data")] 
        public ResourceType resourceType;
        public int resourceProduction;
        
        
        public void Awake()
        {
            _resourceChange[resourceType] = resourceProduction;
        }
    }
}