using UnityEngine;
using UnityEngine.InputSystem;

/**
 * Cammera controller for orbiting around a target.
 * Extended to support moving the target (WASD), rotating with Q/E, and pitch control with arrow keys.
 * ChatGPT Chat with help and better explanetion
 * https://chatgpt.com/share/67d04b25-5b64-8000-a988-8f1c993187a4
 */

public class CameraController : MonoBehaviour
{
    // initial camera setup
    public float yaw = 42.0f;
    public float pitch = 5.0f;
    public float distance = 6f;

    // Movement & rotation
    public float moveSpeed = 5.0f;
    public float keyboardRotationSpeed = 50.0f;
    public float keyboardPitchSpeed = 50.0f;
    public float keyboardZoomSpeed = 2.0f;

    // Limits
    public Vector2 pitchLimits = new Vector2(-89.999f, 0);
    public Vector2 distanceLimits = new Vector2(1.0f, 10.0f);
    public Vector2 yawLimits = new Vector2(-360f, 360f); // Optional yaw limits

    // Internal
    private Vector3 targetPosition = Vector3.zero;
    private PlayerInput playerInput;
    private InputAction moveTargetAction;
    private InputAction rotateAction;
    private InputAction zoomAction;
    private InputAction pitchAction;

    void Start()
    {
        // Input initialization
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
        targetPosition += (forward * moveDir.z + right * moveDir.x) * moveSpeed * deltaTime;

        // -------- Rotate around target (Q/E) --------
        float rotateInput = rotateAction.ReadValue<float>(); // Q/E input
        yaw += rotateInput * keyboardRotationSpeed * deltaTime;

        // -------- Pitch control (Arrow keys) --------
        float pitchInput = pitchAction.ReadValue<float>(); // Up/Down arrows
        pitch -= pitchInput * keyboardPitchSpeed * deltaTime; // Invert to match camera behavior

        // -------- Zoom --------
        float zoomInput = zoomAction.ReadValue<float>(); // Zoom input
        distance -= zoomInput * keyboardZoomSpeed * deltaTime;

        // -------- Clamp values --------
        distance = Mathf.Clamp(distance, distanceLimits.x, distanceLimits.y);
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
        yaw = Mathf.Clamp(yaw, yawLimits.x, yawLimits.y); // Optional

        // -------- Calculate camera position --------
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance); // Negative to move behind target

        transform.position = targetPosition + offset;
        transform.LookAt(targetPosition); // Always look at the center point
    }

    // NOT USED
    /*
    void Update()
    {

    }*/
}
