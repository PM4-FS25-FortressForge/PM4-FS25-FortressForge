using System.Linq;
using Codice.CM.Common;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    [SerializeField] private WeaponBuildingTemplate constants;

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}