using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "RessourceBuilding")]
public class RessourceBuilding : BaseBuilding {
    
    [Header("Ressource Data")]
    public RessourceType ressourceType;
    public int ressourcePerTick;
    
}
