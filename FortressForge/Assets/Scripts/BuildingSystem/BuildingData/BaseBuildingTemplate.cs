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
        
        
        /// <summary>
        /// Initializes the resource change and build cost dictionaries when the object is enabled.
        /// </summary>
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

        /// <summary>
        /// Returns the net resource change for this building if it is enabled.
        /// </summary>
        /// <returns>A dictionary of resource types and their net change rates.</returns>
        public virtual Dictionary<ResourceType, float> GetNetResourceChange()
        {
            return _enabled ? _resourceChange : new Dictionary<ResourceType, float>();
        }

        /// <summary>
        /// Disables the building and performs any necessary cleanup logic.
        /// </summary>
        public virtual void Disable()
        {
            _enabled = false;
            // Implement any necessary cleanup or disabling logic here
            Debug.Log($"{Name} has been disabled.");
        }

        /// <summary>
        /// Returns the build cost for this building as a dictionary of resource types and amounts.
        /// </summary>
        /// <returns>A dictionary of resource types and their build costs.</returns>
        public virtual Dictionary<ResourceType, float> GetBuildCost()
        {
            return _buildCost;
        }
    }
}