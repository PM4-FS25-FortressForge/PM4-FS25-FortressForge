using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.BuildManager;
using UnityEngine;


public class BuildingHealthStateTracker : NetworkBehaviour
{
    private BuildingData _buildingData;
    [SerializeField] private BaseBuildingTemplate _constants;

    private int _currentHealth;

    private MeshRenderer _renderer;
    private Collider _collider;
    private Material _buildingMaterial;

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
        _currentHealth = _constants.MaxHealth;
        
    }

    /// <summary>
    /// Server-side damage application.
    /// </summary>
    [Server]
    public void ApplyDamage(int damage)
    {
        _currentHealth -= damage;
        HandleBuildingStatusRpc(_currentHealth);
    }

    /// <summary>
    /// Visual status handling for the building.
    /// </summary>
    [ObserversRpc]
    private void HandleBuildingStatusRpc(int currentHealth)
    {
        if (currentHealth <= _constants.MaxHealth / 2)
        {
            _buildingMaterial.color = Color.yellow;
        }

        if (currentHealth <= _constants.MaxHealth / 4)
        {
            _buildingMaterial.color = Color.Lerp(Color.white, Color.yellow, 0.6f); // Light orange
        }

        if (currentHealth <= 0)
        {
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