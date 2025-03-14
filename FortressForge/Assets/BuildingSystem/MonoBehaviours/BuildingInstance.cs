using UnityEngine;

public abstract class BuildingInstance : MonoBehaviour 
{
    [SerializeField] private BaseBuilding buildingData;

    protected int currentHealth;

    protected virtual void Start() 
    {
        if (buildingData != null) 
        {
            currentHealth = buildingData.maxHealth;
        }
    }

    void Update() {
    }
}