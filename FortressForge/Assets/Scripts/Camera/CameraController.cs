using FortressForge.HexGrid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FortressForge.CameraControll
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] public float Yaw = 0.0f;
        [SerializeField] public float Pitch = 45;
        [SerializeField] public float Zoom = 6f;
        [SerializeField] public Vector3 TargetPosition = Vector3.zero;

        [SerializeField] public float MoveSpeed = 5.0f;
        [SerializeField] public float RotationSpeed = 50.0f;
        [SerializeField] public float PitchSpeed = 40.0f;
        [SerializeField] public float ZoomSpeed = 2.0f;

        [Tooltip("To avoid the camera to flip or bug do not use value higher than 89\u00b0 or lower than 0\u00b0")]
        [SerializeField] public Vector2 PitchLimits = new Vector2(89, 0);

        [SerializeField] public Vector2 zoomLimits = new Vector2(2.0f, 20.0f);

        private PlayerInput _playerInput;
        private InputAction _moveTargetAction;
        private InputAction _rotateAction;
        private InputAction _zoomAction;
        private InputAction _zoomButtons;
        private InputAction _pitchAction;

        private ITerrainHeightProvider _terrainHeightProvider = new TerrainHeightProvider();
        private float _moveSpeedZoomSensitivity = 1f;
        private float _pitchAndRollSpeedZoomSensitivity = 1f;
        private Vector2 _moveSensitivityLimits = new Vector2(0.4f, 3f);
        private Vector2 _pitchAndRollSensitivityLimits = new Vector2(0.85f, 1.5f);

        void Start()
        {
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput == null)
            {
                Debug.LogError("OrbitCamera: No PlayerInput component found!");
                enabled = false;
                return;
            }

            _moveTargetAction = InitializeActionsButtons("Move");
            _rotateAction = InitializeActionsButtons("Rotate");
            _pitchAction = InitializeActionsButtons("Pitch");
            _zoomAction = InitializeActionsButtons("Zoom");
            _zoomButtons = InitializeActionsButtons("ZoomButtons");
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            HandleMovement(deltaTime);
            HandleRotation(deltaTime);
            HandlePitch(deltaTime);
            HandleZoom(deltaTime);
            UpdateCameraPosition(deltaTime);
        }

        private void HandleMovement(float deltaTime)
        {
            if (_moveTargetAction == null) return;
            Vector2 moveInput = _moveTargetAction.ReadValue<Vector2>();
            Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);
            Vector3 moveVector = Quaternion.Euler(0, Yaw, 0) * moveDir;
            TargetPosition += moveVector * MoveSpeed * deltaTime * _moveSpeedZoomSensitivity;
        }

        private void HandleRotation(float deltaTime)
        {
            if (_rotateAction == null) return;
            float rotateInput = _rotateAction.ReadValue<float>();
            Yaw = (Yaw + rotateInput * RotationSpeed * deltaTime * _pitchAndRollSpeedZoomSensitivity) % 360f;
        }

        private void HandlePitch(float deltaTime)
        {
            if (_pitchAction == null) return;
            float pitchInput = _pitchAction.ReadValue<float>();
            Pitch += pitchInput * PitchSpeed * deltaTime * _pitchAndRollSpeedZoomSensitivity;
            Pitch = Mathf.Clamp(Pitch, PitchLimits.y, PitchLimits.x);
        }

        private void HandleZoom(float deltaTime)
        {
            if (_zoomAction == null || _zoomButtons == null) return;

            float zoomInput = _zoomAction.ReadValue<float>();
            Zoom = Mathf.Clamp(Zoom - zoomInput * ZoomSpeed, zoomLimits.x, zoomLimits.y);

            float zoomButtonInput = _zoomButtons.ReadValue<float>();
            Zoom = Mathf.Clamp(Zoom - zoomButtonInput * ZoomSpeed * deltaTime * 2, zoomLimits.x, zoomLimits.y);

            _moveSpeedZoomSensitivity = Mathf.Lerp(_moveSensitivityLimits.x, _moveSensitivityLimits.y, (Zoom - zoomLimits.x) / (zoomLimits.y - zoomLimits.x));
            _pitchAndRollSpeedZoomSensitivity = Mathf.Lerp(_pitchAndRollSensitivityLimits.x, _pitchAndRollSensitivityLimits.y, (Zoom - zoomLimits.x) / (zoomLimits.y - zoomLimits.x));
        }

        private void UpdateCameraPosition(float deltaTime)
        {
            Quaternion rotation = Quaternion.Euler(Pitch, Yaw, 0);
            Vector3 offset = rotation * new Vector3(0, 0, -Zoom);
            TargetPosition.y = _terrainHeightProvider.SampleHeight(TargetPosition);
            transform.position = TargetPosition + offset;
            transform.LookAt(TargetPosition);
        }

        private InputAction InitializeActionsButtons(string Action)
        {
            if (_playerInput == null || _playerInput.actions == null)
            {
                Debug.LogError("PlayerInput or Actions map is missing.");
                enabled = false;
                return null;
            }

            InputAction desiredButtonAction = _playerInput.actions[Action];
            if (desiredButtonAction == null)
            {
                Debug.LogError(Action + ": This action or button was not found in ActionMap");
                enabled = false;
                return null;
            }

            return desiredButtonAction;
        }

        public void SetYaw(float newYaw) => Yaw = newYaw;
        public void SetPitch(float newPitch) => Pitch = newPitch;
        public void SetZoom(float newZoom) => Zoom = newZoom;
        public void SetTargetPosition(Vector3 newTargetPosition) => TargetPosition = newTargetPosition;
        public void SetMoveSpeed(float newMoveSpeed) => MoveSpeed = newMoveSpeed;
        public void SetRotationSpeed(float newRotationSpeed) => RotationSpeed = newRotationSpeed;
        public void SetPitchSpeed(float newPitchSpeed) => PitchSpeed = newPitchSpeed;
        public void SetZoomSpeed(float newZoomSpeed) => ZoomSpeed = newZoomSpeed;
        public void SetPitchLimits(Vector2 newPitchLimits) => PitchLimits = newPitchLimits;
        public void SetZoomLimits(Vector2 newZoomLimits) => zoomLimits = newZoomLimits;
    }
}
