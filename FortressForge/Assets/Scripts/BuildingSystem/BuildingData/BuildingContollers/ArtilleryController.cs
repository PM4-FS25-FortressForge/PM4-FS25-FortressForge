using FortressForge.BuildingSystem.BuildManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;

public partial class ArtilleryController : MonoBehaviour, ArtilleryAction.IArtilleryActionsActions
{
    private ArtilleryAction artilleryControls;
    private bool isInFightMode = false;
    private bool isRotating = false;
    private bool isAdjustingCannonAngle = false;
    private float rotateInput;
    private float angleInput;
    
    public float minCannonAngle = 10f;  
    public float maxCannonAngle = 90f; 
    public float rotationSpeed = 100f; //TODO make this in scriptable object
    public float pitchSpeed = 100f; //TODO make this in scriptable object

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
        if (Mathf.Abs(rotateInput) > 0.01f)
        {
            Transform towerBase = transform.Find("Geschuetzturm");
            if (towerBase != null)
            {
                towerBase.Rotate(Vector3.forward, rotateInput * rotationSpeed * Time.deltaTime);
            }
        }
        
        if (Mathf.Abs(angleInput) > 0.01f)  //TODO max and min angles dont work as expected (they work somehow but to be fixed)
        {
            Transform cannonShaft = transform.Find("Geschuetzturm/Lauf");
            if (cannonShaft != null)
            {
                // Get current local rotation
                Vector3 currentRotation = cannonShaft.localEulerAngles;

                // Convert to signed angle (-180 to 180)
                float currentPitch = currentRotation.x;
                if (currentPitch > 180f) currentPitch -= 360f;

                // Calculate new pitch
                float newPitch = currentPitch + angleInput * pitchSpeed * Time.deltaTime;

                // Clamp angle
                newPitch = Mathf.Clamp(newPitch, minCannonAngle, maxCannonAngle);

                // Apply back to rotation
                cannonShaft.localEulerAngles = new Vector3(newPitch, currentRotation.x, currentRotation.z);
            }
        }
/*
        if (Mathf.Abs(angleInput) > 0.01f)
        {
            Transform cannonShaft = transform.Find("Geschuetzturm/Lauf");
            if (cannonShaft != null)
            {
                float newPitch = angleInput * pitchSpeed * Time.deltaTime;
                newPitch = Mathf.Clamp(newPitch, minCannonAngle, maxCannonAngle);
                
                cannonShaft.Rotate(cannonShaft.localRotation.x,newPitch, cannonShaft.localRotation.z);
            }
        }*/
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
}