using System.Collections.Generic;
using UnityEngine;


public abstract class BaseBuilding : ScriptableObject
{
    [Header("Shape Data")]
    public List<ShapeData> shapeData;
    
    public GameObject buildingPrefab;
    
    [Header("Building Data")]
    public string name;
    public int metallCost;
    public int maxHealth;
    
}
