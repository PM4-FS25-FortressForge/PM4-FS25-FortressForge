using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FortressForge.BuildingSystem.BuildingData;
using NUnit.Framework;

namespace FortressForge.BuildingSystem.BuildManager
{
    /// <summary>
    /// Manages the collection of placed buildings in the game.
    /// Provides methods to add buildings and access the current list.
    /// </summary>
    public class BuildingManager
    {
        private readonly List<BuildingData> _placedBuildings = new();

        /// <summary>
        /// Gets a read-only collection of all placed buildings.
        /// </summary>
        public ReadOnlyCollection<BuildingData> PlacedBuildings { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingManager"/> class.
        /// </summary>
        public BuildingManager()
        {
            PlacedBuildings = _placedBuildings.AsReadOnly();
        }
        
        /// <summary>
        /// Adds a building to the list of placed buildings.
        /// </summary>
        /// <param name="building">The building to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the building is null.</exception>
        public void AddBuilding(BuildingData building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building), "Building cannot be null");
            }
            
            _placedBuildings.Add(building);
        }
    }
}