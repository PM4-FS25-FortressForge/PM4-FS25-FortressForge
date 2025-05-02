using System.Collections;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Handles input and control logic for a deployable weapon.
/// This Script is used for each instance of the deployed weapon.
/// </summary>
public class WeaponInputHandler : MonoBehaviour, WeaponInputAction.IWeaponInputActionsActions
{
    [SerializeField] private WeaponBuildingTemplate constants;
    [SerializeField] private GameObject cannonBallPrefab;
    [SerializeField] private Transform firePoint;
    
    private WeaponInputAction _weaponInputAction;
    private Coroutine _autoFireCoroutine;
    
    private bool _isInFightMode = false;
    private bool _isReloading = true;
    private bool _isAutoFiring = false;
    
    private float _rotateInput;
    private float angleInput;

    /// <summary>
    /// Initializes the input system and sets this object as its callback handler.
    /// </summary>
    void Awake()
    {
        _weaponInputAction = new WeaponInputAction();
        _weaponInputAction.WeaponInputActions.SetCallbacks(this);
        firePoint = transform.Find("Geschuetzturm/Lauf/FirePoint");
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
            Transform towerBase = transform.Find("Geschuetzturm");
            if (towerBase != null)
            {
                towerBase.Rotate(Vector3.forward, _rotateInput * constants.rotationSpeed * Time.deltaTime);
            }
        }

        // Adjust cannon angle
        if (Mathf.Abs(angleInput) > 0.01f)
        {
            Transform cannonShaft = transform.Find("Geschuetzturm/Lauf");
            if (cannonShaft != null)
            {
                // current rotation
                Vector3 currentRotation = cannonShaft.localEulerAngles;

                // Convert to signed angle (-180 to 180)
                float currentPitch = currentRotation.x;
                if (currentPitch > 180f) currentPitch -= 360f;

                // Calculate new pitch
                float newPitch = currentPitch + angleInput * constants.pitchSpeed * Time.deltaTime;

                // Clamp angle
                newPitch = Mathf.Clamp(newPitch, constants.minCannonAngle, constants.maxCannonAngle);

                // Apply to rotation 
                cannonShaft.localEulerAngles = new Vector3(newPitch, currentRotation.x, currentRotation.z);
            }
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

    /// <summary>
    /// Called by the input system to exit fight mode.
    /// Disables input controls but does not stop firing.
    /// </summary>
    public void OnAdjustCannonAngle(InputAction.CallbackContext context)
    {
        angleInput = context.ReadValue<float>();

        // Stop auto-firing when adjusting angle
        if (_isAutoFiring)
        {
            _isAutoFiring = false;
            StopCoroutine(AutoFire());
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
        // Instantiate the ammunition
        GameObject ammunition = Instantiate(cannonBallPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = ammunition.GetComponent<Rigidbody>();

        // Calculate the force to apply
        Quaternion barrelRotation = transform.Find("Geschuetzturm/Lauf/FirePoint").rotation;
        rb.linearVelocity = barrelRotation * -Vector3.right * constants.cannonForce;
    }
}