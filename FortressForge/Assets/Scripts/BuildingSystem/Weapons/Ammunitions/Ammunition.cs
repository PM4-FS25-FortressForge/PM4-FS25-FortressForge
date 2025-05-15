using UnityEngine;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;

public class Ammunition : NetworkBehaviour
{
    [SerializeField] private WeaponBuildingTemplate _constants;
    
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void SetInitialVelocity(Vector3 velocity)
    {
        _rb.velocity = velocity;
        SetVelocityClientRpc(velocity);
    }

    [ObserversRpc]
    private void SetVelocityClientRpc(Vector3 velocity)
    {
        if (IsServer) return; 
        _rb.velocity = velocity;
    }
    
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