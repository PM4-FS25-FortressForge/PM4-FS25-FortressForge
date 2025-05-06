using System.Collections;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Handles input and control logic for a deployable weapon.
/// This Script is used for each instance of the deployed weapon.
/// </summary>
public class WeaponInputHandler : NetworkBehaviour, WeaponInputAction.IWeaponInputActionsActions
{
    [SerializeField] private WeaponBuildingTemplate constants;
    [SerializeField] private GameObject cannonBallPrefab;
    [SerializeField] private Transform firePoint;
    
    private Transform _towerBase;
    private Transform _cannonShaft;
    
    private WeaponInputAction _weaponInputAction;
    private Coroutine _autoFireCoroutine;
    
    private bool _isInFightMode = false;
    private bool _isReloading = true;
    private bool _isAutoFiring = false;
    
    private float _rotateInput;
    private float _angleInput;

    /// <summary>
    /// Initializes the input system and sets this object as its callback handler.
    /// </summary>
    void Awake()
    {
        _weaponInputAction = new WeaponInputAction();
        _weaponInputAction.WeaponInputActions.SetCallbacks(this);
        firePoint = transform.Find("Geschuetzturm/Lauf/FirePoint");
        _towerBase = transform.Find("Geschuetzturm");
        _cannonShaft = transform.Find("Geschuetzturm/Lauf");
    }

    /// <summary>
    /// Disables the input system when the component is enabled,
    /// so it doesn't start accepting input until explicitly allowed.
    /// </summary>
    void OnEnable()
    {
        _weaponInputAction.WeaponInputActions.Disable();
    }

    /// <summary>
    /// Disables input when the component is disabled.
    /// </summary>
    void OnDisable()
    {
        _weaponInputAction.WeaponInputActions.Disable();
    }

    /// <summary>
    /// Handles rotation and angle changes for each frame.
    /// </summary>
    void Update()
    {
        // Rotate the cannon tower
        if (Mathf.Abs(_rotateInput) > 0.01f)
        {
            updateWeaponRotationServerRpc(_rotateInput, Time.deltaTime);
        }

        // Adjust cannon angle
        if (Mathf.Abs(_angleInput) > 0.01f)
        {
            updateWeaponAngleServerRpc(_angleInput, Time.deltaTime);
        }
    }

    /// <summary>
    /// Called when the cannon is clicked with the mouse.
    /// Enters fight mode and enables input handling.
    /// </summary>
    void OnMouseDown()
    {
        if (!_isInFightMode)
        {
            _isInFightMode = true;
            _weaponInputAction.WeaponInputActions.Enable();
            Debug.Log("Entered Fight Mode!");
        }
    }

    /// <summary>
    /// Called by the input system to enter fight mode.
    /// Enables input controls.
    /// </summary>
    public void OnExitFightMode(InputAction.CallbackContext context)
    {
        if (context.performed && _isInFightMode)
        {
            _isInFightMode = false;
            _weaponInputAction.WeaponInputActions.Disable();
            Debug.Log("Exited Fight Mode");
        }
    }

    /// <summary>
    /// Called by the input system to enter fight mode.
    /// Enables input controls.
    /// </summary>
    public void OnRotateArtillery(InputAction.CallbackContext context)
    {
        _rotateInput = context.ReadValue<float>();

        // Stop auto-firing when adjusting rotation
        if (_isAutoFiring)
        {
            _isAutoFiring = false;
            StopCoroutine(AutoFire());
            
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void updateWeaponRotationServerRpc(float rotationInput, float deltaTime)
    {
        if (_towerBase != null)
        {
            float rotationAmount = rotationInput * constants.rotationSpeed * deltaTime;
            _towerBase.Rotate(Vector3.forward, rotationAmount);
            UpdateWeaponRotationObserversRpc(_towerBase.localEulerAngles);
        }
    }
    
    [ObserversRpc]
    private void UpdateWeaponRotationObserversRpc(Vector3 newRotation)
    {
        if (!IsServer && _towerBase != null)
        {
            _towerBase.localEulerAngles = newRotation;
        }
    }
    
    /// <summary>
    /// Called by the input system to exit fight mode.
    /// Disables input controls but does not stop firing.
    /// </summary>
    public void OnAdjustCannonAngle(InputAction.CallbackContext context)
    {
        _angleInput = context.ReadValue<float>();

        // Stop auto-firing when adjusting angle
        if (_isAutoFiring)
        {
            _isAutoFiring = false;
            StopCoroutine(AutoFire());
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void updateWeaponAngleServerRpc(float angleInput, float deltaTime)
    {
        if (_cannonShaft != null)
        {
            // current rotation
            Vector3 currentRotation = _cannonShaft.localEulerAngles;

            // Convert to signed angle (-180 to 180)
            float currentPitch = currentRotation.x;
            if (currentPitch > 180f) currentPitch -= 360f;

            // Calculate new pitch
            float newPitch = currentPitch + angleInput * constants.pitchSpeed * Time.deltaTime;

            // Clamp angle
            newPitch = Mathf.Clamp(newPitch, constants.minCannonAngle, constants.maxCannonAngle);

            // Apply to rotation 
            _cannonShaft.localEulerAngles = new Vector3(newPitch, currentRotation.x, currentRotation.z);
            
            UpdateWeaponAngleObserversRpc(new Vector3(newPitch, currentRotation.x, currentRotation.z));
        }
    }

    [ObserversRpc]
    public void UpdateWeaponAngleObserversRpc(Vector3 newRotation)
    {
        if (!IsServer && _cannonShaft != null)
        {
            _cannonShaft.localEulerAngles = newRotation;
        }
    }
    
    /// <summary>
    /// Handles rotation input for the cannon's base.
    /// </summary>
    public void OnFireCannon(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!_isAutoFiring && _isReloading)
            {
                _isAutoFiring = true;
                StartCoroutine(AutoFire());
            }
        }
    }

    /// <summary>
    /// Initiates auto-fire when triggered.
    /// Will continue firing until interrupted.
    /// </summary>
    private IEnumerator AutoFire()
    {
        while (_isAutoFiring)
        {
            if (_isReloading)
            {
                FireOnce();
                _isReloading = false;
                yield return new WaitForSeconds(constants.reloadSpeed);
                _isReloading = true;
            }
            else
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// Fires one cannonball from the fire point using the configured force.
    /// </summary>
    private void FireOnce()
    {
        FireCannonServerRpc();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void FireCannonServerRpc()
    {
        if (cannonBallPrefab == null || firePoint == null) return;

        GameObject ammunition = Instantiate(cannonBallPrefab, firePoint.position, firePoint.rotation);
        NetworkObject netObj = ammunition.GetComponent<NetworkObject>();

        if (netObj != null)
        {
            base.Spawn(ammunition);
        }
        else
        {
            Debug.LogError("Cannonball prefab is missing NetworkObject!");
            return;
        }

        Rigidbody rb = ammunition.GetComponent<Rigidbody>();
        Vector3 velocity = firePoint.rotation * -Vector3.right * constants.cannonForce;

        // Apply physics on server
        if (rb != null)
        {
            rb.velocity = velocity;
        }

        // Also broadcast to clients so cannonball moves on all sides
        Cannonball cbScript = ammunition.GetComponent<Cannonball>();
        if (cbScript != null)
        {
            cbScript.SetInitialVelocity(velocity);
        }
    }
}