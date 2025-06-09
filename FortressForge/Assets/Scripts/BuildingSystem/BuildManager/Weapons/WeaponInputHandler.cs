using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.CameraControll;
using FortressForge.Economy;
using FortressForge.Enums;
using FortressForge.HexGrid.Data;
using FortressForge.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FortressForge.BuildingSystem.Weapons
{
    /// <summary>
    /// Handles input, aiming, and firing logic for a networked deployable weapon.
    /// This script is attached to each deployed weapon instance, managing local input and syncing actions across the network.
    /// </summary>
    public class WeaponInputHandler : NetworkBehaviour, WeaponInputAction.IWeaponInputActionsActions
    {
        [SerializeField] private WeaponBuildingTemplate _constants;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private WeaponClasses weaponClass;
        public WeaponClasses WeaponClass => weaponClass;
        private Transform _towerBase;
        private Transform _cannonShaft;

        private WeaponInputAction _weaponInputAction;
        private HexGridData _hexGridData;
        private Material _buildingMaterial;
        private Color _originalColor;

        private Coroutine _reloadCoroutine = null;
        private Coroutine _autoFireCoroutine = null;

        private GameViewStateMachine _gameViewStateMachine;
        private bool _isInFightMode = false;
        private bool _isAutoFiring = false;
        private bool _isReloading = false;
        private bool _isOnCooldown = false;

        private float _rotateInput;
        private float _angleInput;
        private int _currentAmmo;

        private Coroutine _rotateCoroutine;

        private FightSystemOverlayGenerator _overlay;

        private CameraController _cameraController;

        /// <summary>
        /// Initializes the input system and finds the required child transforms.
        /// Throws an exception if any of them are missing.
        /// </summary>
        void Awake()
        {
            _weaponInputAction = new WeaponInputAction();
            _weaponInputAction.WeaponInputActions.SetCallbacks(this);

            _firePoint = transform.Find("Geschuetzturm/Lauf/FirePoint");
            _towerBase = transform.Find("Geschuetzturm");
            _cannonShaft = transform.Find("Geschuetzturm/Lauf");
            if (_firePoint == null || _towerBase == null || _cannonShaft == null)
            {
                throw new Exception("Could not find required transforms!");
            }

            _buildingMaterial = GetComponentInChildren<MeshRenderer>().material;
            _originalColor = _buildingMaterial.color;
            _currentAmmo = _constants.maxAmmo;

            _gameViewStateMachine = FindAnyObjectByType<GameViewStateMachine>();
            if (_gameViewStateMachine == null)
            {
                throw new Exception("Could not find GameViewStateMachine!");
            }

            _overlay = FindAnyObjectByType<FightSystemOverlayGenerator>();
            if (_overlay == null)
            {
                throw new Exception("Could not find FightSystemOverlayGenerator!");
            }

            _cameraController = FindAnyObjectByType<CameraController>();
            if (_cameraController == null)
            {
                throw new Exception("Could not find CameraController!");
            }

            if (!Enum.IsDefined(typeof(WeaponClasses), WeaponClass) || WeaponClass == default)
            {
                WeaponClasses[] values = Enum.GetValues(typeof(WeaponClasses)).Cast<WeaponClasses>().ToArray();
                weaponClass = values[UnityEngine.Random.Range(0, values.Length)];
            }
        }

        /// <summary>
        /// Initializes weapon with external hex grid data used for economic checks.
        /// </summary>
        public void Init(HexGridData hexGridData)
        {
            _hexGridData = hexGridData;
            WeaponBuildingManager.Instance?.RegisterWeaponBuilding(this);
        }

        /// <summary>
        /// Ensures input actions are disabled when the component is turned off.
        /// </summary>
        void OnDisable()
        {
            _weaponInputAction.WeaponInputActions.Disable();
        }

        private void OnDestroy()
        {
            WeaponBuildingManager.Instance?.UnregisterWeaponBuilding(this);
        }

        /// <summary>
        /// Processes input each frame to rotate the weapon base and adjust weapon angle.
        /// Syncs these changes with the server via RPC.
        /// </summary>
        void Update()
        {
            if (Mathf.Abs(_rotateInput) > 0.01f)
            {
                RotateTowerBase(_rotateInput, Time.deltaTime);
            }

            if (Mathf.Abs(_angleInput) > 0.01f)
            {
                AdjustCannonAngle(_angleInput, Time.deltaTime);
            }
        }

        /// <summary>
        /// Called when the weapon prefab is clicked with the mouse.
        /// Enters fight mode and enables input handling.
        /// </summary>
        void OnMouseDown()
        {
            WeaponBuildingManager.Instance.InvokeSelectedWeaponBuilding(this);
            SelectWeapon();
        }

        /// <summary>
        /// Selects the weapon and enables fight mode.
        /// </summary>
        public void SelectWeapon()
        {
            if (!IsOwner)
                return;

            _isInFightMode = true;
            SwitchOverlay();
            _weaponInputAction.WeaponInputActions.Enable();
            _cameraController.MoveToLocation(transform.position);
        }

        /// <summary>
        /// Handles input to exit fight mode. Disables weapon input when the action is performed.
        /// </summary>
        public void OnExitFightMode(InputAction.CallbackContext context)
        {
            if (!IsOwner || !context.performed)
                return;

            ExitFightMode();
        }

        /// <summary>
        /// Exits fight mode, disables weapon input, and switches the overlay back to building mode.
        /// </summary>
        public void ExitFightMode()
        {
            if (!IsOwner || !_isInFightMode)
                return;

            _isInFightMode = false;
            SwitchOverlay();
            _weaponInputAction.WeaponInputActions.Disable();
        }

        /// <summary>
        /// Handles rotation input from the input system and stops auto-firing if active.
        /// </summary>
        public void OnRotateWeapon(InputAction.CallbackContext context)
        {
            if (context.performed && IsOwner)
            {
                _rotateInput = context.ReadValue<float>();
                stopAutoFire();
                StartCoroutine(FireCooldown());
            }
        }

        /// <summary>
        /// Handles cannon pitch (angle) input from the input system and stops auto-firing if active.
        /// </summary>
        public void OnAdjustWeaponAngle(InputAction.CallbackContext context)
        {
            if (context.performed && IsOwner)
            {
                _angleInput = context.ReadValue<float>();
                stopAutoFire();
                StartCoroutine(FireCooldown());
            }
        }

        /// <summary>
        /// Rotation logic
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void RotateTowerBase(float rotationInput, float deltaTime)
        {
            _towerBase.Rotate(Vector3.forward, rotationInput * _constants.rotationSpeed * deltaTime);
        }

        /// <summary>
        /// Angle logic
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void AdjustCannonAngle(float angleInput, float deltaTime)
        {
            // current rotation
            Vector3 currentRotation = _cannonShaft.localEulerAngles;

            // Convert to signed angle (-180 to 180)
            float currentPitch = currentRotation.x;

            if (currentPitch > 180f) currentPitch -= 360f;

            // Calculate new pitch
            float newPitch = currentPitch + angleInput * _constants.pitchSpeed * deltaTime;

            // Clamp angle
            newPitch = Mathf.Clamp(newPitch, _constants.minCannonAngle, _constants.maxCannonAngle);

            // Apply adjusted angle
            _cannonShaft.localEulerAngles = new Vector3(newPitch, currentRotation.x, currentRotation.z);
        }

        /// <summary>
        /// Starts auto-firing when the fire button is pressed.
        /// Handles all firing states and transitions, including reloading.
        /// </summary>
        public void OnFireWeapon(InputAction.CallbackContext context)
        {
            if (!IsOwner || !context.performed) return;
            FireWeaponInternal();
        }

        /// <summary>
        /// Called from the overlay to fire the weapon.
        /// </summary>
        public void FireWeapon()
        {
            if (!IsOwner) return;
            FireWeaponInternal();
        }

        /// <summary>
        /// Internal method to handle weapon firing logic.
        /// </summary>
        private void FireWeaponInternal()
        {
            switch (true)
            {
                case true when !_isAutoFiring && !_isReloading && !_isOnCooldown && _currentAmmo > 0 && _autoFireCoroutine == null: // Fire if not already firing and reloaded
                    _isAutoFiring = true;
                    Debug.Log("Firing");
                    _autoFireCoroutine = StartCoroutine(AutoFire());
                    break;

                case true
                    when _isAutoFiring && _currentAmmo > 0 && _autoFireCoroutine != null: // Denies the player to fire while already firing
                    Debug.Log("Already firing");
                    break;

                case true
                    when !_isReloading && _currentAmmo <= 0 && _reloadCoroutine == null: // Reload if not already reloading and out of ammo
                    ReloadWeaponServerRpc();
                    break;

                case true
                    when _isReloading && _currentAmmo <= 0 && _reloadCoroutine != null: // Denies the player to reload while already reloading
                    Debug.Log("Already reloading");
                    break;
            }
        }

        /// <summary>
        /// Coroutine to fire ammunition at a constant rate until ammo runs out or firing is stopped.
        /// </summary>
        private IEnumerator AutoFire()
        {
            while (_isAutoFiring && _currentAmmo > 0)
            {
                FireCannonServerRpc();
                _currentAmmo--;
                yield return StartCoroutine(FireCooldown());
            }

            _isAutoFiring = false;
            _autoFireCoroutine = null;
            _buildingMaterial.color = Color.blue;

            ReloadWeaponServerRpc();
        }

        /// <summary>
        /// Handles firing delay between shots.
        /// </summary>
        private IEnumerator FireCooldown()
        {
            _isOnCooldown = true;
            yield return new WaitForSeconds(_constants.automaticReloadSpeed);
            _isOnCooldown = false;
        }

        /// <summary>
        /// Handles time-based reloading and ammunition refill.
        /// </summary>
        private IEnumerator ReloadTime()
        {
            yield return new WaitForSeconds(_constants.weaponReload);
            _currentAmmo = _constants.maxAmmo;
            _buildingMaterial.color = _originalColor;
            _isReloading = false;
            _reloadCoroutine = null;
            Debug.Log("Reloading Finished");
        }

        /// <summary>
        /// Stops auto-firing and ends the firing coroutine.
        /// Triggered by aim adjustment or exit condition.
        /// </summary>
        private void stopAutoFire()
        {
            if (_isAutoFiring)
            {
                _isAutoFiring = false;

                if (_autoFireCoroutine != null)
                {
                    StopCoroutine(_autoFireCoroutine);
                    _autoFireCoroutine = null;
                    StartCoroutine(FireCooldown());
                }
            }
        }

        /// <summary>
        /// Spawns the ammunition object and assigns its velocity. Called on the server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void FireCannonServerRpc()
        {
            GameObject ammunition = Instantiate(_constants.ammunitionPrefab, _firePoint.position, _firePoint.rotation);
            NetworkObject netObj = ammunition.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                base.Spawn(ammunition);
            }

            Vector3 velocity = _firePoint.rotation * -Vector3.right * _constants.cannonForce;

            Ammunition ammoScript = ammunition.GetComponent<Ammunition>();
            ammoScript.SetInitialVelocity(velocity);
        }

        /// <summary>
        /// Checks for available resources and initiates reloading on the server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void ReloadWeaponServerRpc()
        {
            var cost = new Dictionary<ResourceType, float>
            {
                { ResourceType.Amunition, _constants.reloadCost }
            };

            bool success = false;

            if (_hexGridData != null && _hexGridData.EconomySystem.CheckForSufficientResources(cost))
            {
                _isReloading = true;
                _hexGridData.EconomySystem.PayResource(cost);
                success = true;
                Debug.Log("Reloading");
            }

            TargetReloadResult(Owner, success);
        }

        /// <summary>
        /// Client-side handler for reload result. Starts reload time if successful.
        /// </summary>
        [TargetRpc]
        private void TargetReloadResult(NetworkConnection conn, bool success)
        {
            if (success)
            {
                _reloadCoroutine = StartCoroutine(ReloadTime());
            }
            else
            {
                Debug.Log("Reloading Failed - Not enough resources");
            }
        }

        /// <summary>
        /// Toggles the overlay state based on the current mode (fight or build).
        /// </summary>
        private void SwitchOverlay()
        {
            _gameViewStateMachine?.SetOverlayState(_isInFightMode
                ? GameOverlayState.FightingSystemOverlay
                : GameOverlayState.BuildingOverlay);
        }

        /// <summary>
        /// Called when the weapon is rotated from the overlay.
        /// </summary>
        /// <param name="direction">The direction to rotate the weapon.</param>
        public void OnRotateWeaponFromOverlayStart(AxisDirection direction)
        {
            if (!IsOwner) return;
            _rotateInput = (int)direction * 0.1f;
        }

        /// <summary>
        /// Stops the rotation input when the overlay action is stopped.
        /// </summary>
        public void OnRotateWeaponFromOverlayStop()
        {
            _rotateInput = 0f;
        }

        /// <summary>
        /// Called when the weapon angle is adjusted from the overlay.
        /// </summary>
        /// <param name="direction">The direction to adjust the weapon angle.</param>
        public void OnAdjustWeaponAngleFromOverlayStart(AxisDirection direction)
        {
            if (!IsOwner) return;
            _angleInput = (int)direction * 0.1f;
        }

        /// <summary>
        /// Stops the angle adjustment input when the overlay action is stopped.
        /// </summary>
        public void OnAdjustWeaponAngleFromOverlayStop()
        {
            _angleInput = 0f;
        }

        public int GetCurrentAmmo()
        {
            return _currentAmmo;
        }
    }
}