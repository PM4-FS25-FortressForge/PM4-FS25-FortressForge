using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class WeaponController : MonoBehaviour, ArtilleryAction.IArtilleryActionsActions
{
    private ArtilleryAction artilleryControls;
    private bool isInFightMode = false;
    private bool isRotating = false;
    private bool isAdjustingCannonAngle = false;
    private float rotateInput;
    private float angleInput;

    [SerializeField] private WeaponBuildingTemplate constants;
    [SerializeField] private GameObject cannonBallPrefab;
    [SerializeField] private Transform firePoint;


    void Awake()
    {
        artilleryControls = new ArtilleryAction();
        artilleryControls.ArtilleryActions.SetCallbacks(this);
    }

    void OnEnable()
    {
        artilleryControls.ArtilleryActions.Disable();
    }

    void OnDisable()
    {
        artilleryControls.ArtilleryActions.Disable();
    }

    void Update()
    {
        // Rotate the cannon tower
        if (Mathf.Abs(rotateInput) > 0.01f)
        {
            Transform towerBase = transform.Find("Geschuetzturm");
            if (towerBase != null)
            {
                towerBase.Rotate(Vector3.forward);
                towerBase.Rotate(Vector3.forward, rotateInput * constants.rotationSpeed * Time.deltaTime);
            }
        }

        // Adjust cannon angle
        if (Mathf.Abs(angleInput) > 0.01f)
        {
            Transform cannonShaft = transform.Find("Geschuetzturm/Lauf");
            if (cannonShaft != null)
            {
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
        if (!isInFightMode)
        {
            isInFightMode = true;
            artilleryControls.ArtilleryActions.Enable();
            Debug.Log("Entered Fight Mode!");
        }
    }

    public void OnExitFightMode(InputAction.CallbackContext context)
    {
        if (context.performed && isInFightMode)
        {
            isInFightMode = false;
            artilleryControls.ArtilleryActions.Disable();
            Debug.Log("Exited Fight Mode");
        }
    }

    public void OnEnterFightMode(InputAction.CallbackContext context)
    {
        if (context.performed && !isInFightMode)
        {
            isInFightMode = true;
            artilleryControls.ArtilleryActions.Enable();
            Debug.Log("Entered Fight Mode!");
        }
    }

    public void OnRotateArtillery(InputAction.CallbackContext context)
    {
        if (!isInFightMode || context.canceled)
        {
            rotateInput = 0f;
            return;
        }

        rotateInput = context.ReadValue<float>();
    }

    public void OnAdjustCannonAngle(InputAction.CallbackContext context)
    {
        if (!isInFightMode || context.canceled)
        {
            angleInput = 0f;
            return;
        }

        angleInput = context.ReadValue<float>();
    }

    public void OnFireCannon(InputAction.CallbackContext context)
    {
        if (!isInFightMode || !context.performed)
            return;

        firePoint = transform.Find("Geschuetzturm/Lauf/FirePoint");
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint not found in hierarchy!");
            return;
        }

        GameObject cannonball = Instantiate(cannonBallPrefab, firePoint.position, firePoint.rotation);
        cannonball.GetComponent<Renderer>().material.color = Color.red;
        Rigidbody rb = cannonball.GetComponent<Rigidbody>();

        Quaternion barrelRotation = transform.Find("Geschuetzturm/Lauf/FirePoint").rotation;

        rb.linearVelocity = barrelRotation * -Vector3.right * constants.cannonForce;
    }
}