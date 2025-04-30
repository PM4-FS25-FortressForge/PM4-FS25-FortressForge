using System.Collections;
using System.Collections.Generic;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class WeaponInputHandler : MonoBehaviour, WeaponInputAction.IWeaponInputActionsActions
{
    private WeaponInputAction _weaponInputAction;
    
    private bool _isInFightMode = false;
    private bool _isReloading = true;
    private bool _isRotating = false;
    private bool _isAdjustingCannonAngle = false;
    
    private float _rotateInput;
    private float angleInput;
    
    [SerializeField] private WeaponBuildingTemplate constants;
    [SerializeField] private GameObject cannonBallPrefab;
    [SerializeField] private Transform firePoint;
    
    void Awake()
    {
        _weaponInputAction = new WeaponInputAction();
        _weaponInputAction.WeaponInputActions.SetCallbacks(this);
    }

    void OnEnable()
    {
        _weaponInputAction.WeaponInputActions.Disable();
    }

    void OnDisable()
    {
        _weaponInputAction.WeaponInputActions.Disable();
    }

    void Update()
    {
        // Rotate the cannon tower
        if (Mathf.Abs(_rotateInput) > 0.01f)
        {
            Transform towerBase = transform.Find("Geschuetzturm");
            if (towerBase != null)
            {
                // Apply rotation
                towerBase.Rotate(Vector3.forward, _rotateInput * constants.rotationSpeed * Time.deltaTime);
            }
        }

        // Adjust cannon angle
        if (Mathf.Abs(angleInput) > 0.01f)
        {
            Transform cannonShaft = transform.Find("Geschuetzturm/Lauf");
            if (cannonShaft != null)
            {
                // Get current rotation
                Vector3 currentRotation = cannonShaft.localEulerAngles;

                // Convert to signed angle (-180 to 180)
                float currentPitch = currentRotation.x;
                if (currentPitch > 180f) currentPitch -= 360f;

                // Calculate new pitch
                float newPitch = currentPitch + angleInput * constants.pitchSpeed * Time.deltaTime;

                // Clamp angle
                newPitch = Mathf.Clamp(newPitch, constants.minCannonAngle, constants.maxCannonAngle);

                // Apply back to rotation
                cannonShaft.localEulerAngles = new Vector3(newPitch, currentRotation.x, currentRotation.z);
            }
        }
    }

    void OnMouseDown()
    {
        if (!_isInFightMode)
        {
            _isInFightMode = true;
            _weaponInputAction.WeaponInputActions.Enable();
            Debug.Log("Entered Fight Mode!");
        }
    }

    public void OnExitFightMode(InputAction.CallbackContext context)
    {
        if (context.performed && _isInFightMode)
        {
            _isInFightMode = false;
            _weaponInputAction.WeaponInputActions.Disable();
            Debug.Log("Exited Fight Mode");
        }
    }

    public void OnEnterFightMode(InputAction.CallbackContext context)
    {
        if (context.performed && !_isInFightMode)
        {
            _isInFightMode = true;
            _weaponInputAction.WeaponInputActions.Enable();
            Debug.Log("Entered Fight Mode!");
        }
    }

    public void OnRotateArtillery(InputAction.CallbackContext context)
    {
        if (!_isInFightMode || context.canceled)
        {
            _rotateInput = 0f;
            return;
        }

        _rotateInput = context.ReadValue<float>();
    }

    public void OnAdjustCannonAngle(InputAction.CallbackContext context)
    {
        if (!_isInFightMode || context.canceled)
        {
            angleInput = 0f;
            return;
        }
        angleInput = context.ReadValue<float>();
    }

    public void OnFireCannon(InputAction.CallbackContext context)
    {
        if (!_isInFightMode || !context.performed || !_isReloading)
            return;

        firePoint = transform.Find("Geschuetzturm/Lauf/FirePoint");
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint not found in hierarchy!");
            return;
        }

        GameObject cannonball = Instantiate(cannonBallPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = cannonball.GetComponent<Rigidbody>();

        Quaternion barrelRotation = transform.Find("Geschuetzturm/Lauf/FirePoint").rotation;

        rb.linearVelocity = barrelRotation * -Vector3.right * constants.cannonForce;

        _isReloading = false;
        StartCoroutine(Reload());
    }
    private IEnumerator Reload()
    {
        _isReloading = false;
        yield return new WaitForSeconds(constants.reloadSpeed);
        _isReloading = true;
    }
}