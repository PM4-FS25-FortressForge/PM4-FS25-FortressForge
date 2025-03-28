using System.Collections.Generic;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Building System/Building")]
    public abstract class BaseBuilding : ScriptableObject
    {
        [Header("Shape Data")] public List<ShapeData> shapeData = new();

        public GameObject buildingPrefab;

        [Header("Building Data")] public string name;
        public int metallCost;
        public int maxHealth;
    }
}