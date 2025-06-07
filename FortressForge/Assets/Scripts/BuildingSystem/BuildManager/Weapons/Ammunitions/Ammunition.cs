using UnityEngine;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;

/// <summary>
/// Represents a networked ammunition projectile used by deployable weapons.
/// Handles physics behavior, velocity synchronization, and collision-based damage application.
/// </summary>
public class Ammunition : NetworkBehaviour
{
    [SerializeField] private WeaponBuildingTemplate _constants;
    
    private Rigidbody _rb;

    /// <summary>
    /// Initializes the Rigidbody with appropriate interpolation and collision settings.
    /// </summary>
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /// <summary>
    /// Sets the initial velocity of the projectile and syncs it with all observers.
    /// </summary>
    public void SetInitialVelocity(Vector3 velocity)
    {
        _rb.linearVelocity = velocity;
        SetVelocityClientRpc(velocity);
    }

    [ObserversRpc]
    private void SetVelocityClientRpc(Vector3 velocity)
    {
        if (IsServer) return; 
        _rb.linearVelocity = velocity;
    }
    
    /// <summary>
    /// Synchronizes velocity across all clients.
    /// Skips execution on the server since the velocity was already applied.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Projectile hit: " + collision.gameObject.name + " (layer: " +
                  LayerMask.LayerToName(collision.gameObject.layer) + ")");

        // Damage logic (server-side only)
        if (IsServer)
        {
            BuildingHealthStateTracker health = collision.gameObject.GetComponentInParent<BuildingHealthStateTracker>();

            if (health != null)
            {
                health.ApplyDamage(_constants.baseDamage);
            }

            base.Despawn();
        }
    }
}