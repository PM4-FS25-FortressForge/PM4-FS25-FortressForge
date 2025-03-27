using System.Collections.Generic;
using FortressForge.Serializables;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    public abstract class BaseBuilding : ScriptableObject
    {
        [Header("Shape Data")] public List<ShapeData> shapeData;

        public GameObject buildingPrefab;

        [Header("Building Data")] public string name;
        public int metallCost;
        public int maxHealth;
    }
}