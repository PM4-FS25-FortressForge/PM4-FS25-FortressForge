using UnityEngine;

public class Cannonball : MonoBehaviour
{
    
    void Start()
    {
        // Automatically destroy after 5 seconds, in case it doesn't hit anything
        Destroy(gameObject, 5f);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // TODO: Add damage/explosion logic here

        Destroy(gameObject);
    }
}