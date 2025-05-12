using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;


public class BuildingHealthStateTracker : NetworkBehaviour
{
    [SerializeField] private WeaponBuildingTemplate _constants;

    private MeshRenderer _renderer;
    private Collider _collider;
    private Material _buildingMaterial;

    public bool IsDestroyed => _constants.BuildingHealth <= 0;

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
        _constants.BuildingHealth = _constants.MaxHealth;
        ;
    }

    /// <summary>
    /// Server-side damage application.
    /// </summary>
    [Server]
    public void ApplyDamage(int damage)
    {
        _constants.BuildingHealth -= damage;
        HandleBuildingStatus();
    }

    /// <summary>
    /// Visual status handling for the building.
    /// </summary>
    private void HandleBuildingStatus()
    {
        if (_constants.BuildingHealth <= _constants.BuildingHealth / 2)
        {
            if (_buildingMaterial != null)
                _buildingMaterial.color = Color.Lerp(Color.white, Color.yellow, 0.7f); // Light orange
        }

        if (_constants.BuildingHealth <= _constants.BuildingHealth / 4)
        {
            if (_buildingMaterial != null)
                _buildingMaterial.color = Color.Lerp(Color.yellow, Color.red, 0.7f); // Light red
        }

        if (_constants.BuildingHealth <= 0)
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
        if (_renderer != null)
            _renderer.enabled = false;

        if (_collider != null)
            _collider.enabled = false;
    }
}