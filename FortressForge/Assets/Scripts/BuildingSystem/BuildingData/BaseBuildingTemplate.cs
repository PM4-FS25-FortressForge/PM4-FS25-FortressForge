using System.Collections.Generic;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.HexTile;
using FortressForge.Serializables;
using UnityEngine;
using UnityEngine.Serialization;

namespace FortressForge.BuildingSystem.BuildingData
{
    public abstract class BaseBuildingTemplate : ScriptableObject
    {
        /// <summary>
        /// Information about the shape of the building, in the form of a list of HexTileCoordinates.
        /// </summary>
        [Header("Shape Data")] 
        public List<HexTileCoordinate> ShapeData = new();

        public GameObject BuildingPrefab;

        [Header("Building Data")] public string Name;
        public int MetalCost;
        public int MaxHealth;

        public void DoStuff()
        {
            Debug.Log("Doing stuff");
        }
    }
}