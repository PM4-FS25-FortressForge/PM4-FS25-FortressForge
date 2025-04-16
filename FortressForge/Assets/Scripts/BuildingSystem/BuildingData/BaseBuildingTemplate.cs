using System.Collections.Generic;
using FortressForge.Economy;
using FortressForge.HexGrid;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    public class BaseBuildingTemplate : ScriptableObject, IEconomyActor
    {
        /// <summary>
        /// Information about the shape of the building, in the form of a list of HexTileCoordinates.
        /// </summary>
        [Header("Shape Data")] 
        [SerializeField]
        public List<HexTileCoordinate> ShapeData = new() { new HexTileCoordinate(0,0,0) };

        public GameObject BuildingPrefab;

        [Header("Building Data")] 
        public string Name;
        public int MetalCost;
        public int MaxHealth; 
        
        [Header("Resource Data")]
        protected Dictionary<ResourceType, float> _resourceChange = new();
        protected Dictionary<ResourceType, float> _buildCost = new();
        protected bool _enabled = true;
        
        public float resourceRate;
        public ResourceType resourceRateType;
        public float resourceCost;
        public ResourceType resourceCostType;
        
        
        public void Awake()
        {
            _resourceChange[resourceRateType] = resourceRate;
            _buildCost[resourceCostType] = resourceCost;
        }
        
        public virtual Dictionary<ResourceType, float> GetNetResourceChange()
        {
            return _enabled ? _resourceChange : new Dictionary<ResourceType, float>();
        }
        
        public virtual void Disable()
        {
            _enabled = false;
            // Implement any necessary cleanup or disabling logic here
            Debug.Log($"{Name} has been disabled.");
        }

        public virtual Dictionary<ResourceType, float> GetBuildCost()
        {
            return _buildCost;
        } 
    }
}