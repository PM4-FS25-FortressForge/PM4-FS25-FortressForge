using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FortressForge.BuildingSystem.BuildingData;

namespace FortressForge.BuildingSystem.BuildManager
{
    public class BuildingManager
    {
        private readonly List<BaseBuildingTemplate> _placedBuildings = new();
        public ReadOnlyCollection<BaseBuildingTemplate> PlacedBuildings { get; private set; }
        
        public BuildingManager()
        {
            PlacedBuildings = _placedBuildings.AsReadOnly();
        }
        
        /// <summary>
        /// Adds a building to the list of placed buildings.
        /// </summary>
        /// <param name="building"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddBuilding(BaseBuildingTemplate building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building), "Building cannot be null");
            }
            
            _placedBuildings.Add(building);
        }
    }
}