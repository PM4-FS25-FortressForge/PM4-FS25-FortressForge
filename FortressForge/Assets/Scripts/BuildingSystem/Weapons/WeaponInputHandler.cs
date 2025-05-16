using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.Economy;
using FortressForge.HexGrid.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Handles input, aiming, and firing logic for a networked deployable weapon.
/// This script is attached to each deployed weapon instance, managing local input and syncing actions across the network.
/// </summary>
public class WeaponInputHandler : NetworkBehaviour, WeaponInputAction.IWeaponInputActionsActions
{
    
    [SerializeField] private WeaponBuildingTemplate _constants;
    [SerializeField] private Transform _firePoint;
    private Transform _towerBase;
    private Transform _cannonShaft;

    private WeaponInputAction _weaponInputAction;
    private Coroutine _autoFireCoroutine;
    private HexGridData _hexGridData;
    private Material _buildingMaterial;
    private Color _originalColor;

    private bool _isInFightMode = false;
    private bool _singleShotReload = true;
    private bool _isAutoFiring = false;
    private bool _canReload = false;

    private float _rotateInput;
    private float _angleInput;
    private int _currentAmmo;

    /// <summary>
    /// Initializes the input system and finds the required child transforms.
    /// Throws an exception if any of them are missing.
    /// </summary>
    void Awake()
    {
        _weaponInputAction = new WeaponInputAction();
        _weaponInputAction.WeaponInputActions.SetCallbacks(this);

        _firePoint = transform.Find("Geschuetzturm/Lauf/FirePoint");
        _towerBase = transform.Find("Geschuetzturm");
        _cannonShaft = transform.Find("Geschuetzturm/Lauf");
        if (_firePoint == null || _towerBase == null || _cannonShaft == null)
        {
            throw new System.Exception("Could not find required transforms!");
        }

        _buildingMaterial = GetComponentInChildren<MeshRenderer>().material;
        _originalColor = _buildingMaterial.color;
        _currentAmmo = _constants.maxAmmo;
    }

    public void Init(HexGridData hexGridData)
    {
        _hexGridData = hexGridData;
    }

    /// <summary>
    /// Ensures input actions are disabled when the component is turned off.
    /// </summary>
    void OnDisable()
    {
        _weaponInputAction.WeaponInputActions.Disable();
    }

    /// <summary>
    /// Processes input each frame to rotate the weapon base and adjust weapon angle.
    /// Syncs these changes with the server via RPC.
    /// </summary>
    void Update()
    {
        if (Mathf.Abs(_rotateInput) > 0.01f)
        {
            RotateTowerBase(_rotateInput, Time.deltaTime);
        }

        if (Mathf.Abs(_angleInput) > 0.01f)
        {
            AdjustCannonAngle(_angleInput, Time.deltaTime);
        }
    }

    /// <summary>
    /// Called when the weapon prefab is clicked with the mouse.
    /// Enters fight mode and enables input handling.
    /// </summary>
    void OnMouseDown()
    {
        if (!_isInFightMode && IsOwner)
        {
            _isInFightMode = true;
            _weaponInputAction.WeaponInputActions.Enable();
            Debug.Log("Entered Fight Mode!");
        }
    }

    /// <summary>
    /// Handles input to exit fight mode. Disables weapon input when the action is performed.
    /// </summary>
    public void OnExitFightMode(InputAction.CallbackContext context)
    {
        if (context.performed && _isInFightMode && IsOwner)
        {
            _isInFightMode = false;
            _weaponInputAction.WeaponInputActions.Disable();
            Debug.Log("Exited Fight Mode");
        }
    }

    /// <summary>
    /// Handles rotation input from the input system and stops auto-firing if active.
    /// </summary>
    public void OnRotateWeapon(InputAction.CallbackContext context)
    {
        if (context.performed && IsOwner)
        {
            _rotateInput = context.ReadValue<float>();
            stopAutoFire();
        }
    }

    /// <summary>
    /// Handles cannon pitch (angle) input from the input system and stops auto-firing if active.
    /// </summary>
    public void OnAdjustWeaponAngle(InputAction.CallbackContext context)
    {
        if (context.performed && IsOwner)
        {
            _angleInput = context.ReadValue<float>();
            stopAutoFire();
        }
    }

    /// <summary>
    /// Rotation logic
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RotateTowerBase(float rotationInput, float deltaTime)
    {
        _towerBase.Rotate(Vector3.forward, rotationInput * _constants.rotationSpeed * deltaTime);
    }

    /// <summary>
    /// Angle logic
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void AdjustCannonAngle(float angleInput, float deltaTime)
    {
        // current rotation
        Vector3 currentRotation = _cannonShaft.localEulerAngles;

        // Convert to signed angle (-180 to 180)
        float currentPitch = currentRotation.x;

        if (currentPitch > 180f) currentPitch -= 360f;

        // Calculate new pitch
        float newPitch = currentPitch + angleInput * _constants.pitchSpeed * deltaTime;

        // Clamp angle
        newPitch = Mathf.Clamp(newPitch, _constants.minCannonAngle, _constants.maxCannonAngle);

        // Apply adjusted angle
        _cannonShaft.localEulerAngles = new Vector3(newPitch, currentRotation.x, currentRotation.z);
    }

    /// <summary>
    /// Starts auto-firing when fire input is received, if not already firing and reloaded.
    /// </summary>
    public void OnFireWeapon(InputAction.CallbackContext context)
    {
        if (context.performed && IsOwner)
        {
            if (!_isAutoFiring && _singleShotReload && _currentAmmo > 0)
            {
                _isAutoFiring = true;
                StartCoroutine(AutoFire());
            }
        }
    }

    /// <summary>
    /// Coroutine to continuously fire the weapon at a fixed rate until stopped.
    /// Waits for reload time between shots.
    /// </summary>
    private IEnumerator AutoFire()
    {
        while (_isAutoFiring && _currentAmmo > 0)
        {
            FireCannonServerRpc();
            _currentAmmo--;
            yield return new WaitForSeconds(_constants.automaticReloadSpeed);
        }
        
        _isAutoFiring = false;
        StopCoroutine(AutoFire());
        _buildingMaterial.color = Color.blue;
        reloadWeapon();
        yield return new WaitForSeconds(_constants.weaponReload);
        _buildingMaterial.color = _originalColor;
    }

    /// <summary>
    /// Server-side method to spawn ammunition, set its velocity, and broadcast the shot to all clients.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void FireCannonServerRpc()
    {
        GameObject ammunition = Instantiate(_constants.ammunitionPrefab, _firePoint.position, _firePoint.rotation);
        NetworkObject netObj = ammunition.GetComponent<NetworkObject>();

        if (netObj != null)
        {
            base.Spawn(ammunition);
        }

        Vector3 velocity = _firePoint.rotation * -Vector3.right * _constants.cannonForce;

        Ammunition ammoScript = ammunition.GetComponent<Ammunition>();
        ammoScript.SetInitialVelocity(velocity);
    }

    private void reloadWeapon()
    {
        var cost = new Dictionary<ResourceType, float>
        {
            { ResourceType.Amunition, _constants.rechargeCost }
        };

        _hexGridData.EconomySystem.PayResource(cost);
        _currentAmmo = _constants.maxAmmo;
    }

    /// <summary>
    /// Stops auto-firing and ends the firing coroutine.
    /// Called when aim is adjusted.
    /// </summary>
    private void stopAutoFire()
    {
        if (_isAutoFiring)
        {
            _isAutoFiring = false;
            StopCoroutine(AutoFire());
        }
    }
}