using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cammera controller for orbiting around a target.
/// Extended to support moving the target (WASD), rotating with Q/E, pitch control with arrow (up/down) keys and Zoom in/out with mouse wheel or arrow (left/right) keys.
/// The Camera will always look (be centered) at the point at 0.0.0 coordinates
///
/// ------------------------------------------------------------------------------------------------------------
/// Short Dokumentation for Unity setup and additional files
/// This Feature implementation includes 2 Files, the CameraController.cs and the CameraInputActions.inputactions
///
/// The CameraController.cs is This File
/// The CameraInputActions.inputactions is a file containing a list of all Input Actions for the CameraController.cs linked to their respective keyboard keys
/// The Camera Object in Unity needs to have the CameraController.cs (this File) attached to it as an Component
/// Also The Camera Object in Unity needs to have the PlayerInput component attached to it (PlayerInput component is a Unity component that allows you to use the new Input System)
/// And additionally needs to have the CameraInputActions.inputactions file assigned to the PlayerInput Action component
///
/// Functions:
/// The Camera can be controlled with following inputs
///  - WASD for moving the centred target (Camera will orbit around the target)
///  - Q/E for rotating the camera around the target (Yaw)
///  - Arrow keys (Up/Down) for Pitch control (Camera angle)
///  - Mouse wheel or Arrow keys (Left/Right) for Zoom in/out (Camera distance from target)
///
/// Therefore:
/// The Camera will Orbit around a centred target (0.0.0 coordinates at beginning) and always look at the centred target
/// ------------------------------------------------------------------------------------------------------------
/// </summary>

public class CameraController : MonoBehaviour
{
    /// Warning:
    /// Be carefull witch this public fields because they are exposed in the Unity Editor
    /// If you change the values in the Unity Editor it will override the default values in this script
    
    // Initial camera setup
    [SerializeField] public float yaw = 0.0f;
    [SerializeField] public float pitch = 45;
    [SerializeField] public float zoom = 6f;
    [SerializeField] public Vector3 targetPosition = Vector3.zero; //At the Sart the Camera will always look (be centered) at the point at 0.0.0 coordinates

    // Movement & rotation speeds
    [SerializeField] public float moveSpeed = 5.0f;
    [SerializeField] public float rotationSpeed = 50.0f;
    [SerializeField] public float pitchSpeed = 40.0f;
    [SerializeField] public float zoomSpeed = 2.0f;

    // Limits
    [Tooltip("To avoid the camera to flip or bug do not use value higher than 89\u00b0 or lower than 0\u00b0")]
    [SerializeField] public Vector2 pitchLimits = new Vector2(89, 0); // Pitch flat to fully top-dow (To avoid the camera to flip do not use value higher than 89°)
    [SerializeField] public Vector2 zoomLimits = new Vector2(2.0f, 20.0f); // Min/Max distance from target Zoom

    // Internal
    private PlayerInput _playerInput;
    private InputAction _moveTargetAction;
    private InputAction _rotateAction;
    private InputAction _zoomAction;
    private InputAction _zoomButtons;
    private InputAction _pitchAction;
    private float _deltaTime;
        
    /// <summary>
    /// Start function to initialize the PlayerInput and the InputActions
    /// The PlayerInput is a Unity component that allows you to use the new Input System
    /// This functions checks if the playerInput Unity Object is found if not it will disable this Script and print an error log
    /// Additionally it initializes the InputActions from the Unity ActionMap for the Buttons (WASD, Q/E, Arrow keys and Mouse Wheel)
    /// </summary>
    void Start()
    {
        // Input initialization of playerInput Unity Object and error managment
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)    //Test if the playerInput Unity Object is found
        {
            Debug.LogError("OrbitCamera: No PlayerInput component found!");
            enabled = false;        // Disable this Script if not found 
            return;
        }
        
        // Input initialization of Buttons and error managment
        _moveTargetAction = InitializeActionsButtons("Move");        // WASD movement
        _rotateAction = InitializeActionsButtons("Rotate");          // Q/E for yaw rotation
        _pitchAction = InitializeActionsButtons("Pitch");            // Up/Down arrow keys
        _zoomAction = InitializeActionsButtons("Zoom");              // Zoom in/out mouse Wheel
        _zoomButtons = InitializeActionsButtons("ZoomButtons");      // Zoom in/out Buttons (left/right arrow keys)
    }

    /// <summary>
    /// FixedUpdate function to handle the movement, rotation, pitch control and zoom of the camera
    /// This function is called every fixed frame-rate frame
    /// It calls the respective functions to handle the movement, rotation, pitch control and zoom of the camera
    /// Additionally it calls the UpdateCameraPosition function to calculate the new camera position
    /// </summary>
    void FixedUpdate()
    {
        _deltaTime = Time.deltaTime;
        HandleMovement();      //Move Target (WASD)
        HandleRotation();      //Rotate around target (Q/E)
        HandlePitch();         //Pitch control (Arrow keys)
        HandleZoom();          //Zoom
        UpdateCameraPosition();         //Calculate at update camera position
    }

    /// <summary>
    /// HandleMovement function to handle the horizontal movement of the camera
    /// To be more specifiv the function calculates the new horizontal position of the target object wich is followed by the camera
    /// To calculate the new target position the function uses the WASD input and the current yaw of the camera
    /// </summary>
    private void HandleMovement()
    {
        Vector2 moveInput = _moveTargetAction.ReadValue<Vector2>();  // WASD input
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y); // X (A/D), Z (W/S)
        Vector3 moveVector = Quaternion.Euler(0, yaw, 0) * moveDir; // Move relative to current yaw
        targetPosition += moveVector * moveSpeed * _deltaTime;   // calculate new target position
    }

    /// <summary>
    /// HandleRotation function to handle the rotation of the camera around the target
    /// The function calculates the new yaw of the camera
    /// To calculate the new yaw the function uses the Q/E input
    /// </summary>
    private void HandleRotation()
    {
        float rotateInput = _rotateAction.ReadValue<float>();    // Q/E input
        yaw = (yaw + rotateInput * rotationSpeed * _deltaTime) % 360f;  // Rotate around target
    }

    /// <summary>
    /// HandlePitch function to handle the pitch control of the camera
    /// The function calculates the new pitch of the camera
    /// To calculate the new pitch the function uses the Up/Down arrow keys input
    /// </summary>
    private void HandlePitch()
    {
        float pitchInput = _pitchAction.ReadValue<float>();  // Up/Down arrows
        pitch += pitchInput * pitchSpeed * _deltaTime;     // move camera in pitch angle to center
        pitch = Mathf.Clamp(pitch, pitchLimits.y, pitchLimits.x);   // Limit pitch angle
    }

    /// <summary>
    /// HandleZoom function to handle the zoom in/out of the camera
    /// The function calculates the new distance of the camera from the target
    /// To calculate the new distance the function uses the mouse wheel input and the arrow keys (left/right) input
    /// </summary>
    private void HandleZoom()
    {
        float zoomInput = _zoomAction.ReadValue<float>();    // Zoom input with mouse wheel
        zoom = Mathf.Clamp(zoom - zoomInput * zoomSpeed, zoomLimits.x, zoomLimits.y);   // Zoom without deltaTime to make it consistent and good feeling

        float zoomButtonInput = _zoomButtons.ReadValue<float>(); // Zoom input with the Buttons
        zoom = Mathf.Clamp(zoom - zoomButtonInput * zoomSpeed * _deltaTime * 2, zoomLimits.x, zoomLimits.y); // Zoom faster (multiplied by 2) with buttons but depends on the deltaTime
    }

    /// <summary>
    /// UpdateCameraPosition function to calculate the new position of the camera
    /// The function calculates the new position of the camera based on the target position, the pitch, yaw and distance of the camera
    /// It sets the new position of the camera and makes sure the camera always looks at the centred target
    /// </summary>
    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);  //Calculate the rotation around the centred object
        Vector3 offset = rotation * new Vector3(0, 0, -zoom); // Calculate the offset of the camera
        targetPosition.y = Terrain.activeTerrain.SampleHeight(targetPosition);
        transform.position = targetPosition + offset;  // Set the new position of the camera
        transform.LookAt(targetPosition);   // Always look at the center point
    }

    /// <summary>
    /// InitializeActionsButtons function to initialize the InputActions for the Buttons
    /// This functions checks if the desired action / Button is found in the playerInput ActionMap
    /// if not it will disable this Script and print an error log
    /// Additionally it initializes the InputActions from the Unity ActionMap for the Buttons (WASD, Q/E, Arrow keys and Mouse Wheel)
    /// </summary>
    /// <param name="Action"> Parameter to specify the desired action / button from the playerInput ActionMap</param> 
    /// <returns>returns the desiredButtonAction as InputAction</returns>
    private InputAction InitializeActionsButtons(string Action)
    {
        InputAction desiredButtonAction = _playerInput.actions[Action]; // Get the desired action from the playerInput ActionMap
        if (desiredButtonAction == null)    //Test if the desired action / Button is found in the playerInput ActionMap
        {
            Debug.LogError(Action + ": This action or button was not found in ActionMap");
            enabled = false;    //Disable this Script if not found 
        }
        return desiredButtonAction; 
    }
}
