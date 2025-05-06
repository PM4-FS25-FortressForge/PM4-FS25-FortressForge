using FishNet.Object;
using UnityEngine;

public class Cannonball : NetworkBehaviour
{
    private Rigidbody _rigidbody;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    [ObserversRpc]
    public void SetInitialVelocity(Vector3 velocity)
    {
        if (_rigidbody != null)
        {
            _rigidbody.velocity = velocity;
        }
    }

    public void Launch(Vector3 velocity)
    {
        if (_rigidbody != null)
        {
            _rigidbody.velocity = velocity;
        }

        if (IsServer)
        {
            SetInitialVelocity(velocity);
        }
    }

    // Despawn when hitting something
    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            Despawn();
        }
    }
}