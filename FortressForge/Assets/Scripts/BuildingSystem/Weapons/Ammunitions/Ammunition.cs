using UnityEngine;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;

public class Ammunition : NetworkBehaviour
{
    [SerializeField] private WeaponBuildingTemplate _constants;

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Projectile hit: " + collision.gameObject.name + " (layer: " +
                  LayerMask.LayerToName(collision.gameObject.layer) + ")");

        // Damage logic (server-side only)
        if (IsServer)
        {
            BuildingHealthStateTracker health = collision.gameObject.GetComponentInParent<BuildingHealthStateTracker>();

            if (health != null && !health.IsDestroyed)
            {
                health.ApplyDamage(_constants.baseDamage);
            }

            base.Despawn();
        }
    }
}