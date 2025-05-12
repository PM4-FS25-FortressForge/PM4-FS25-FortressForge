using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;

/// <summary>
/// Represents a network-synced projectile fired from a weapon.
/// Handles physics-based movement and automatic despawning upon collision.
/// </summary>
public class Ammunition : NetworkBehaviour
{
    [SerializeField] private WeaponBuildingTemplate _constants;
    private Rigidbody _rigidbody;

    /// <summary>
    /// Caches the Rigidbody component on initialization.
    /// </summary>
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Automatically despawns the projectile when it collides with another object.
    /// Only executed on the server and broadcasted to all clients.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        
        BuildingHealthStateTracker target = collision.gameObject.GetComponentInParent<BuildingHealthStateTracker>();
        if (target != null)
        {
            target.ApplyDamage(_constants.baseDamage);
        }

        Despawn();
    }
}