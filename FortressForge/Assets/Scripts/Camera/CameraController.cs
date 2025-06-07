using FortressForge.GameInitialization;
using FortressForge.HexGrid;
using FortressForge.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace FortressForge.CameraControll
{
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
        [SerializeField] public float Yaw = 0.0f;

        [SerializeField] public float Pitch = 45;
        [SerializeField] public float Zoom = 6f;

        [SerializeField] public Vector3 TargetPosition = Vector3.zero; //At the Sart the Camera will always look (be centered) at the point at 0.0.0 coordinates

        // Movement & rotation speeds
        [SerializeField] public float MoveSpeed = 5.0f;
        [SerializeField] public float RotationSpeed = 50.0f;
        [SerializeField] public float PitchSpeed = 40.0f;
        [SerializeField] public float MouseWheelZoomSpeed = 1.0f; // Zoom input with mouse is smaller to make the step size of the mouse wheel smaller
        [SerializeField] public float ButtonsZoomSpeed = 5f; // Zoom input with the Buttons is bigger to make the buttons more sensitive
        private float _targetZoom;  // Zoomtarget that will be reached by SmoothDamp
        private float _zoomVelocity; // SmoothDamp (used in HandleZoom methode) needs this velocity-Reference
        private float _buildingSelectionZoom = 60f; // Default zoom level when moving to a building
        private readonly float _zoomSmoothTime = 0.2f; // the time it takes to reach the _targetZoom (bigger value more smooth but less accurate)
        
        [FormerlySerializedAs("_config")]
        [Header("Game Start Configuration")]
        [SerializeField] public GameStartConfiguration Config;

        // Limits
        [Tooltip("To avoid the camera to flip or bug do not use value higher than 89\u00b0 or lower than 0\u00b0")] [SerializeField]
        public Vector2 PitchLimits = new Vector2(89, 0); // Pitch flat to fully top-dow (To avoid the camera to flip do not use value higher than 89°)

        [SerializeField] public Vector2 zoomLimits = new Vector2(2.0f, 20.0f); // Min/Max distance from target Zoom
        
        // Internal
        private PlayerInput _playerInput;
        private InputAction _moveTargetAction;
        private InputAction _rotateAction;
        private InputAction _zoomAction;
        private InputAction _zoomButtons;
        private InputAction _pitchAction;

        private ITerrainHeightProvider _terrainHeightProvider;
        private float _targetHeight; // target terrain height that will be reached by SmoothDamp
        private float _heightVelocity; // Needed for SmoothDamp (in the UpdateCameraPosition methode)
        private readonly float _heightSmoothTime = 0.5f; // Higher = smoother but less accurate
        
        private float _moveSpeedZoomSensitivity = 1f; // Zoom speed sensitivity for the WASD movement (from 0.4f, 3f, neutral is 1.0f)
        private float _pitchAndRollSpeedZoomSensitivity = 1f; // Zoom speed sensitivity for the pitch and roll movement (from 0.85f, 1.5f, neutral is 1.0f)
        private Vector2 _moveSensitivityLimits = new Vector2(0.4f, 3f); // Min/Max sensitivity for the WASD movement
        private Vector2 _pitchAndRollSensitivityLimits = new Vector2(0.85f, 1.5f); // Min/Max sensitivity for the pitch and roll movement

        /// <summary>
        /// Start function to initialize the PlayerInput and the InputActions
        /// The PlayerInput is a Unity component that allows you to use the new Input System
        /// This functions checks if the playerInput Unity Object is found if not it will disable this Script and print an error log
        /// Additionally it initializes the InputActions from the Unity ActionMap for the Buttons (WASD, Q/E, Arrow keys and Mouse Wheel)
        /// </summary>
        void Start()
        {
            _terrainHeightProvider = new TerrainHeightProvider();
            // Input initialization of playerInput Unity Object and error managment
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput == null) //Test if the playerInput Unity Object is found
            {
                Debug.LogError("OrbitCamera: No PlayerInput component found!");
                enabled = false; // Disable this Script if not found 
                return;
            }

            // Input initialization of Buttons and error managment
            _moveTargetAction = InitializeActionsButtons("Move"); // WASD movement
            _rotateAction = InitializeActionsButtons("Rotate"); // Q/E for yaw rotation
            _pitchAction = InitializeActionsButtons("Pitch"); // Up/Down arrow keys
            _zoomAction = InitializeActionsButtons("Zoom"); // Zoom in/out mouse Wheel
            _zoomButtons = InitializeActionsButtons("ZoomButtons"); // Zoom in/out Buttons (left/right arrow keys)

            _targetZoom = Zoom; // Set the initial target zoom to the initial zoom
            // Set the initial target height to the initial height of the terrain
            _targetHeight = GetTerrainHeight(TargetPosition); 
        }

        /// <summary>
        ///Update function to handle the movement, rotation, pitch control and zoom of the camera
        /// It calls the respective functions to handle the movement, rotation, pitch control and zoom of the camera
        /// Additionally it calls the UpdateCameraPosition function to calculate the new camera position
        /// </summary>
        void Update()
        {
            float deltaTime = Time.deltaTime;
            HandleMovement(deltaTime); //Move Target (WASD)
            HandleRotation(deltaTime); //Rotate around target (Q/E)
            HandlePitch(deltaTime); //Pitch control (Arrow keys)
            HandleZoom(deltaTime); //Zoom
            UpdateCameraPosition(); //Calculate at update camera position
        }

        /// <summary>
        /// HandleMovement function to handle the horizontal movement of the camera
        /// To be more specifiv the function calculates the new horizontal position of the target object wich is followed by the camera
        /// To calculate the new target position the function uses the WASD input and the current yaw of the camera
        /// </summary>
        /// <param name="deltaTime"></param>
        private void HandleMovement(float deltaTime)
        {
            Vector2 moveInput = _moveTargetAction.ReadValue<Vector2>(); // WASD input
            Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y); // X (A/D), Z (W/S)
            Vector3 moveVector = Quaternion.Euler(0, Yaw, 0) * moveDir; // Move relative to current yaw
            TargetPosition += moveVector * MoveSpeed * deltaTime * _moveSpeedZoomSensitivity; // calculate new target position
        }

        /// <summary>
        /// HandleRotation function to handle the rotation of the camera around the target
        /// The function calculates the new yaw of the camera
        /// To calculate the new yaw the function uses the Q/E input
        /// </summary>
        /// <param name="deltaTime"></param>
        private void HandleRotation(float deltaTime)
        {
            float rotateInput = _rotateAction.ReadValue<float>(); // Q/E input
            Yaw = (Yaw + rotateInput * RotationSpeed * deltaTime * _pitchAndRollSpeedZoomSensitivity) % 360f; // Rotate around target
        }

        /// <summary>
        /// HandlePitch function to handle the pitch control of the camera
        /// The function calculates the new pitch of the camera
        /// To calculate the new pitch the function uses the Up/Down arrow keys input
        /// </summary>
        /// <param name="deltaTime"></param>
        private void HandlePitch(float deltaTime)
        {
            float pitchInput = _pitchAction.ReadValue<float>(); // Up/Down arrows
            Pitch += pitchInput * PitchSpeed * deltaTime * _pitchAndRollSpeedZoomSensitivity; // move camera in pitch angle to center
            Pitch = Mathf.Clamp(Pitch, PitchLimits.y, PitchLimits.x); // Limit pitch angle
        }

        /// <summary>
        /// HandleZoom function to handle the zoom in/out of the camera
        /// The function calculates the new distance of the camera from the target
        /// To calculate the new distance the function uses the mouse wheel input and the arrow keys (left/right) input
        /// </summary>
        /// <param name="deltaTime"></param>
        private void HandleZoom(float deltaTime)
        {
            if (UIClickChecker.Instance.IsMouseOnOverlay()) return;
            float zoomInput = _zoomAction.ReadValue<float>(); // Zoom input with mouse wheel (made more smooth with SmoothDamp)
            _targetZoom = Mathf.Clamp(_targetZoom - zoomInput * MouseWheelZoomSpeed, zoomLimits.x, zoomLimits.y); 

            float zoomButtonInput = _zoomButtons.ReadValue<float>(); // Zoom input with the Buttons
            _targetZoom = Mathf.Clamp(_targetZoom - zoomButtonInput * ButtonsZoomSpeed * deltaTime, zoomLimits.x, zoomLimits.y);
            
            Zoom = Mathf.SmoothDamp(Zoom, _targetZoom, ref _zoomVelocity, _zoomSmoothTime); // SmoothDamp for zooming smoothly in/out
            
            // Linear mapping from [min, max Zoom] to [0.4, 3] for WASD movement
            _moveSpeedZoomSensitivity = _moveSensitivityLimits.x + (Zoom - zoomLimits.x) * (_moveSensitivityLimits.y - _moveSensitivityLimits.x) / (zoomLimits.y - zoomLimits.x);
            // Linear mapping from [min, max Zoom] to [0.85, 1.5] for pitch and roll
            _pitchAndRollSpeedZoomSensitivity = _pitchAndRollSensitivityLimits.x + (Zoom - zoomLimits.x) * (_pitchAndRollSensitivityLimits.y - _pitchAndRollSensitivityLimits.x) / (zoomLimits.y - zoomLimits.x);
        }

        /// <summary>
        /// UpdateCameraPosition function to calculate the new position of the camera
        /// The function calculates the new position of the camera based on the target position, the pitch, yaw and distance of the camera
        /// It sets the new position of the camera and makes sure the camera always looks at the centred target
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateCameraPosition()
        {
            Quaternion rotation = Quaternion.Euler(Pitch, Yaw, 0); // Calculate the rotation around the centred object
            Vector3 offset = rotation * new Vector3(0, 0, -Zoom); // Calculate the offset of the camera
            
            TargetPosition.y = GetTerrainHeight(TargetPosition); // get the height of the terrain at the current position
            _targetHeight = Mathf.SmoothDamp(_targetHeight, TargetPosition.y, ref _heightVelocity, _heightSmoothTime);  // SmoothDamp for height
            TargetPosition.y = _targetHeight; 

            transform.position = TargetPosition + offset; // Set the new position of the camera
            transform.LookAt(TargetPosition); // Always look at the center point
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
            if (desiredButtonAction == null) //Test if the desired action / Button is found in the playerInput ActionMap
            {
                Debug.LogError(Action + ": This action or button was not found in ActionMap");
                enabled = false; //Disable this Script if not found 
            }

            return desiredButtonAction;
        }

        /// <summary>
        /// Sets the yaw rotation angle of the camera (in degrees).
        /// </summary>
        public void SetYaw(float newYaw)
        {
            Yaw = newYaw;
        }

        /// <summary>
        /// Sets the pitch rotation angle of the camera (in degrees).
        /// </summary>
        public void SetPitch(float newPitch)
        {
            Pitch = newPitch;
        }

        /// <summary>
        /// Sets the zoom distance from the camera to the target.
        /// </summary>
        public void SetZoom(float newZoom)
        {
            Zoom = newZoom;
        }

        /// <summary>
        /// Sets the position the camera is looking at.
        /// </summary>
        public void SetTargetPosition(Vector3 newTargetPosition)
        {
            TargetPosition = newTargetPosition;
        }

        /// <summary>
        /// Sets the movement speed of the camera.
        /// </summary>
        public void SetMoveSpeed(float newMoveSpeed)
        {
            MoveSpeed = newMoveSpeed;
        }

        /// <summary>
        /// Sets the rotation speed of the camera (yaw).
        /// </summary>
        public void SetRotationSpeed(float newRotationSpeed)
        {
            RotationSpeed = newRotationSpeed;
        }

        /// <summary>
        /// Sets the pitch rotation speed of the camera.
        /// </summary>
        public void SetPitchSpeed(float newPitchSpeed)
        {
            PitchSpeed = newPitchSpeed;
        }

        /// <summary>
        /// Sets the speed of the mpuse Wheel at which the camera zooms in and out.
        /// </summary>
        public void SetMouseWheelZoomSpeed(float newZoomSpeed)
        {
            MouseWheelZoomSpeed = newZoomSpeed;
        }
        
        /// <summary>
        /// Sets the speed of the buttons at which the camera zooms in and out.
        /// </summary>
        public void SetButtonsZoomSpeed(float newZoomSpeed)
        {
            ButtonsZoomSpeed = newZoomSpeed;
        }

        /// <summary>
        /// Sets the pitch angle limits to prevent flipping (X: max, Y: min).
        /// </summary>
        public void SetPitchLimits(Vector2 newPitchLimits)
        {
            PitchLimits = newPitchLimits;
        }

        /// <summary>
        /// Sets the zoom distance limits (X: min, Y: max).
        /// </summary>
        public void SetZoomLimits(Vector2 newZoomLimits)
        {
            zoomLimits = newZoomLimits;
        }

        private float GetTerrainHeight(Vector3 targetPosition)
        {
            return _terrainHeightProvider.SampleHexHeight(targetPosition, Config.TileHeight, Config.TileSize);
        }
        
        /// <summary>
        /// Moves the camera to a specific position.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        public void MoveToLocation(Vector3 position)
        {
            TargetPosition = position;
            _targetHeight = GetTerrainHeight(position);
            Zoom = Mathf.SmoothDamp(Zoom, _buildingSelectionZoom, ref _zoomVelocity, _zoomSmoothTime);
            UpdateCameraPosition();
        }
    }
}