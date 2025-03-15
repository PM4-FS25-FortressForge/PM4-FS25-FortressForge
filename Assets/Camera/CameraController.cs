using UnityEngine;
using UnityEngine.InputSystem;

/**
 * Cammera controller for orbiting around a target.
 * Extended to support moving the target (WASD), rotating with Q/E, pitch control with arrow (up/down) keys and Zoom in/out with mouse wheel or arrow (left/right) keys.
 * The Camera will always look (be centered) at the point at 0.0.0 coordinates
 * Short Dokumentation at the end of File
 * ChatGPT Chat with help and better explanetion
 * https://chatgpt.com/share/67d04b25-5b64-8000-a988-8f1c993187a4
 */

public class CameraController : MonoBehaviour
{
    // initial camera setup
    public float yaw = 0.0f;
    public float pitch = 45; //(Maybe have no affect at all ?)
    public float distance = 6f;

    // Movement & rotation
    public float moveSpeed = 5.0f;
    public float keyboardRotationSpeed = 50.0f;
    public float keyboardPitchSpeed = 50.0f;
    public float keyboardZoomSpeed = 2.0f;

    // Limits
    public Vector2 pitchLimits = new Vector2(90, 0); // Pitch flat to fully top-dow (Maybe have no affect at all ?)
    public Vector2 distanceLimits = new Vector2(0.5f, 25.0f); // Min/Max distance from target Zoom
    //public Vector2 yawLimits = new Vector2(-360f, 360f); // Optional yaw limits

    // Internal
    private Vector3 targetPosition = Vector3.zero;     //(The Camera will always look (be centered) at the point at 0.0.0 coordinates)
    private PlayerInput playerInput;
    private InputAction moveTargetAction;
    private InputAction rotateAction;
    private InputAction zoomAction;
    private InputAction ZoomButtons;
    private InputAction pitchAction;

    void Start()
    {
        // Input initialization and error managment
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("OrbitCamera: No PlayerInput component found!");
            enabled = false;
            return;
        }

        moveTargetAction = playerInput.actions["Move"]; // WASD movement
        if (moveTargetAction == null)
        {
            Debug.LogError("OrbitCamera: No Move action found in PlayerInput!");
            enabled = false;
            return;
        }

        rotateAction = playerInput.actions["Rotate"]; // Q/E for yaw rotation
        if (rotateAction == null)
        {
            Debug.LogError("OrbitCamera: No Rotate action found in PlayerInput!");
            enabled = false;
            return;
        }

        pitchAction = playerInput.actions["Pitch"]; // Up/Down arrow keys
        if (pitchAction == null)
        {
            Debug.LogError("OrbitCamera: No Pitch action found in PlayerInput!");
            enabled = false;
            return;
        }

        zoomAction = playerInput.actions["Zoom"]; // Zoom in/out
        if (zoomAction == null)
        {
            Debug.LogError("OrbitCamera: No Zoom action found in PlayerInput!");
            enabled = false;
            return;
        }
        
        ZoomButtons = playerInput.actions["ZoomButtons"]; // Zoom in/out
        if (ZoomButtons == null)
        {
            Debug.LogError("OrbitCamera: No ZoomButtons action found in PlayerInput!");
            enabled = false;
            return;
        }

        Debug.Log($"Initial pitch: {pitch:F0}   yaw: {yaw:F0}   distance: {distance:F1}");
    }

    void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;

        // -------- Move Target (WASD) --------
        Vector2 moveInput = moveTargetAction.ReadValue<Vector2>(); // WASD input
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y); // X (A/D), Z (W/S)
        // Move relative to current yaw
        Vector3 forward = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, yaw, 0) * Vector3.right;
        targetPosition -= (forward * moveDir.z + right * moveDir.x) * moveSpeed * deltaTime; // calculate new target position

        // -------- Rotate around target (Q/E) --------
        float rotateInput = rotateAction.ReadValue<float>(); // Q/E input
        yaw += rotateInput * keyboardRotationSpeed * deltaTime;

        // -------- Pitch control (Arrow keys) --------
        float pitchInput = pitchAction.ReadValue<float>(); // Up/Down arrows
        pitch += pitchInput * keyboardPitchSpeed * deltaTime; // Invert to match camera behavior

        // -------- Zoom --------
        float zoomInput = zoomAction.ReadValue<float>(); // Zoom input with mouse wheel
        distance -= zoomInput * keyboardZoomSpeed;  // Zoom without deltaTime to make it consistent and good feeling
        
        // -------- Zoom with Buttons (Arrow keys) --------
        float zoomButtonInput = ZoomButtons.ReadValue<float>(); // Zoom input from the Buttons
        distance -= zoomButtonInput * keyboardZoomSpeed * deltaTime * 2;    // Zoom faster (multiplied by 2) with buttons but depends on the deltaTime

        // -------- Clamp values --------
        distance = Mathf.Clamp(distance, distanceLimits.x, distanceLimits.y);   //Clamp values for min max cutoff
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
        //yaw = Mathf.Clamp(yaw, yawLimits.x, yawLimits.y); // Optional for yaw limits

        // -------- Calculate camera position --------
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);      //Calculate the rotation around the centred object
        Vector3 offset = rotation * new Vector3(0, 0, distance);   

        transform.position = targetPosition + offset;       
        transform.LookAt(targetPosition); // Always look at the center point
    }

    // NOT USED
    /*
    void Update()
    {

    }*/
    
}

/***
 * Short Dokumentation for Unity setup and additional files
 * This Feature implementation includes 2 Files, the CameraController.cs and the CameraInputActions.inputactions
 * The CameraController.cs is This File
 * The CameraInputActions.inputactions is a file containing a list of all Input Actions for the CameraController.cs linked to their respective keyboard keys
 * 
 * The Camera Object in Unity needs to have the CameraController.cs (this File) attached to it as an Component
 * Also The Camera Object in Unity needs to have the PlayerInput component attached to it (PlayerInput component is a Unity component that allows you to use the new Input System)
 * And additionally needs to have the CameraInputActions.inputactions file assigned to the PlayerInput Action component
 *
 * Functions:
 * The Camera can be controlled with following inputs
 *  - WASD for moving the centred target (Camera will orbit around the target)
 *  - Q/E for rotating the camera around the target (Yaw)
 *  - Arrow keys (Up/Down) for Pitch control (Camera angle)
 *  - Mouse wheel or Arrow keys (Left/Right) for Zoom in/out (Camera distance from target)
 *
 * Therefore: 
 * The Camera will Orbit around a centred target (0.0.0 coordinates at beginning) and always look at the centred target
 */
