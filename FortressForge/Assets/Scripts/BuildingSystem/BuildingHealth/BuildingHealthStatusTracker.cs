using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;


public class BuildingHealthStateTracker : NetworkBehaviour
{
    [SerializeField] private BaseBuildingTemplate _constants;
    
    private int currentHealth;
    
    private MeshRenderer _renderer;
    private Collider _collider;
    private Material _buildingMaterial;

    public bool IsDestroyed => currentHealth <= 0;

    /// <summary>
    /// Caches the MeshRenderer and Collider components on initialization.
    /// </summary>
    private void Awake()
    {
        _renderer = GetComponentInChildren<MeshRenderer>();
        _collider = GetComponentInChildren<Collider>();

        if (_renderer != null)
            _buildingMaterial = _renderer.material; // This creates a unique instance
    }

    /// <summary>
    /// Sets the building's health to its maximum value on the server.
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        currentHealth = _constants.MaxHealth;
        ;
    }

    /// <summary>
    /// Server-side damage application.
    /// </summary>
    [Server]
    public void ApplyDamage(int damage)
    {
        currentHealth -= damage;
        HandleBuildingStatus();
    }

    /// <summary>
    /// Visual status handling for the building.
    /// </summary>
    private void HandleBuildingStatus()
    {
        if (currentHealth <= currentHealth / 2)
        {
            if (_buildingMaterial != null)
                _buildingMaterial.color = Color.Lerp(Color.white, Color.yellow, 0.7f); // Light orange
        }

        if (currentHealth <= currentHealth / 4)
        {
            if (_buildingMaterial != null)
                _buildingMaterial.color = Color.Lerp(Color.yellow, Color.red, 0.7f); // Light red
        }

        if (currentHealth <= 0)
        {
            if (_buildingMaterial != null)
                _buildingMaterial.color = Color.red;

            MakeInvisibleAndPassable();
        }
    }

    /// <summary>
    /// Makes the building invisible and passable.
    /// </summary>
    private void MakeInvisibleAndPassable()
    {
        if (_collider != null)
            _collider.enabled = false;
    }
}