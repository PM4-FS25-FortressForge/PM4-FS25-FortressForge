using System.Collections.Generic;
using System.Linq;
using FortressForge.Economy;
using FortressForge.HexGrid;
using FortressForge.Serializables;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Base Building", menuName = "Buildings/BaseBuilding")]
    public class BaseBuildingTemplate : ScriptableObject, IEconomyActor
    {
        /// <summary>
        /// Information about the shape of the building, in the form of a list of HexTileCoordinates.
        /// </summary>
        [Header("Shape Data")]
        public virtual List<HexTileCoordinate> ShapeData
        {
            get { return ShapeDataEntries.Select(data => data.Coordinate).ToList(); }
        }

        public List<HexTileEntry> ShapeDataEntries = new();
        
        public GameObject BuildingPrefab;

        [Header("Building Data")] 
        public string Name;
        public int MaxHealth; 
        
        [Header("Resource Data")]
        protected Dictionary<ResourceType, float> _resourceChange = new();
        protected Dictionary<ResourceType, float> _buildCost = new();
        protected bool _enabled = true;
        
        public List<RessourceTypeRate> ResourceRates = new();
        public List<RessourceTypeRate> BuildCosts = new();
        
        public void OnEnable()
        {
            ResourceRates
                .GroupBy(r => r.Type)
                .ToList()
                .ForEach(g =>
                {
                    _resourceChange[g.Key] = g.Sum(x => x.Rate);
                });
            BuildCosts
                .GroupBy(r => r.Type)
                .ToList()
                .ForEach(g =>
                {
                    _buildCost[g.Key] = g.Sum(x => x.Rate);
                });
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