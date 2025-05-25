using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using FishNet;
using FishNet.Object;
using Object = UnityEngine.Object;
using FortressForge.BuildingSystem.BuildingData;
using GameObject = UnityEngine.GameObject;

namespace Tests.Weapon
{
    public class WeaponInputHandlerRotationTests : InputTestFixture
    {
        private GameObject _weaponInstance;
        private WeaponInputHandler _weaponInputHandler;
        private Transform _towerBase;
        private Keyboard _keyboard;

        private const float _rotationSpeed = 90f;
        private const float _pitchSpeed = 90f;

        private const int _maxWeaponAngle = 60;
        private const int _minWeaponAngle = 10;

        private const float _cannonForce = 50f;
        private const int _maxAmmo = 5;
        private const int _reloadCost = 20;
        private const float _automaticReloadSpeed = 3f;
        private const int _weaponReload = 5;
        private const int _baseDamage = 100;

        /// <summary>
        /// Registers the keyboard input device and prepares the input system for test execution.
        /// This method is called before each test runs.
        /// </summary>
        [SetUp]
        public override void Setup()
        {
            base.Setup();

            // Register and add keyboard device for input simulation
            InputSystem.RegisterLayout<Keyboard>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            Assert.NotNull(_keyboard, "Keyboard device not found.");
        }

        /// <summary>
        /// Loads a minimal test scene and sets up the networking context using FishNet.
        /// It also instantiates and spawns the weapon prefab and configures its input handler and rotation parameters.
        /// </summary>
        private IEnumerator SetupSceneAndWeapon()
        {
            // Load empty test scene
            SceneManager.LoadScene("EmptyScene", LoadSceneMode.Single);
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "EmptyScene");

            // Instantiate NetworkManager prefab for FishNet networking setup
            var networkManagerPrefab = Resources.Load<GameObject>("Prefabs/NetworkManager");
            Assert.IsNotNull(networkManagerPrefab, "NetworkManager prefab not found!");
            var networkManagerInstance = Object.Instantiate(networkManagerPrefab);
            networkManagerInstance.name = "TestNetworkManager";
            yield return null;

            // Start server and client connections
            InstanceFinder.ServerManager.StartConnection();
            yield return new WaitUntil(() => InstanceFinder.ServerManager.Started);

            InstanceFinder.ClientManager.StartConnection();
            yield return new WaitUntil(() => InstanceFinder.ClientManager.Started);

            // Instantiate weapon prefab and spawn it on server
            var weaponPrefab = Resources.Load<GameObject>("Prefabs/Tier_1/Tier_1_Konzept");
            Assert.IsNotNull(weaponPrefab, "Weapon prefab not found!");
            _weaponInstance = Object.Instantiate(weaponPrefab);
            yield return null;

            var networkObject = _weaponInstance.GetComponent<NetworkObject>();
            Assert.IsNotNull(networkObject, "Weapon prefab missing NetworkObject!");
            InstanceFinder.ServerManager.Spawn(networkObject);
            yield return null;

            _weaponInputHandler = _weaponInstance.GetComponentInChildren<WeaponInputHandler>();
            Assert.IsNotNull(_weaponInputHandler, "WeaponInputHandler not found!");


            // Set weapon rotation and ammo constants
            var constants = ScriptableObject.CreateInstance<WeaponBuildingTemplate>();
            constants.rotationSpeed = _rotationSpeed;
            constants.pitchSpeed = _pitchSpeed;
            constants.minCannonAngle = _minWeaponAngle;
            constants.minCannonAngle = _maxWeaponAngle;
            constants.cannonForce = _cannonForce;
            constants.maxAmmo = _maxAmmo;
            constants.reloadCost = _reloadCost;
            constants.automaticReloadSpeed = _automaticReloadSpeed;
            constants.weaponReload = _weaponReload;
            constants.baseDamage = _baseDamage;

            var constantsField = typeof(WeaponInputHandler).GetField("_constants",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            constantsField.SetValue(_weaponInputHandler, constants);

            _towerBase = _weaponInstance.transform.Find("Geschuetzturm");
            Assert.IsNotNull(_towerBase, "'Geschuetzturm' transform not found!");
        }

        /// <summary>
        /// Normalizes a given angle to be within the range [0, 360).
        /// This is useful for comparing angles regardless of how Unity internally stores them.
        /// </summary>
        private float NormalizeRotation(float angle)
        {
            return (angle + 360f) % 360f;
        }

        private float NormalizePitchAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }

        /// <summary>
        /// Simulates pressing the 'L' key to rotate the weapon turret to the right (clockwise).
        /// Asserts that the turret rotated approximately by the expected degrees based on rotation speed and delta time.
        /// </summary>
        [UnityTest]
        public IEnumerator RotateRight_LKeyPressed()
        {
            yield return SetupSceneAndWeapon();

            _towerBase.localEulerAngles = Vector3.zero;

            // Enter fight mode to enable input actions
            _weaponInputHandler.SendMessage("OnMouseDown");

            // Press L key to rotate right (clockwise)
            Press(_keyboard.lKey);

            yield return new WaitForSeconds(2.5f);

            Release(_keyboard.lKey);
            yield return null;

            // Calculate expected rotation based on rotationSpeed and deltaTime
            float expectedRotation = _rotationSpeed * Time.deltaTime;
            float actualRotation = NormalizeRotation(_towerBase.localEulerAngles.z);

            Assert.That(actualRotation, Is.EqualTo(expectedRotation).Within(1f),
                $"Expected clockwise rotation ~{expectedRotation}°, but was {actualRotation}°");
        }

        /// <summary>
        /// Simulates pressing the 'J' key to rotate the weapon turret to the left (counter-clockwise).
        /// Asserts that the turret rotated approximately by the expected negative degrees based on rotation speed and delta time.
        /// </summary>
        [UnityTest]
        public IEnumerator RotateLeft_JKeyPressed()
        {
            yield return SetupSceneAndWeapon();

            // Disable problematic GameRoomSynchronisation to avoid NullReferenceException during tests
            var gameRoomSync = Object.FindObjectOfType<FortressForge.Network.GameRoomSynchronisation>();
            if (gameRoomSync != null)
                Object.Destroy(gameRoomSync);

            _towerBase.localEulerAngles = Vector3.zero;

            // Enter fight mode to enable input actions
            _weaponInputHandler.SendMessage("OnMouseDown");

            // Press J key to rotate left (counter-clockwise)
            Press(_keyboard.jKey);

            yield return new WaitForSeconds(2.5f);

            Release(_keyboard.jKey);
            yield return null;

            // Expected rotation based on rotation speed and deltaTime
            float expectedRotation = -_rotationSpeed * Time.deltaTime;
            float actualRotation = NormalizeRotation(_towerBase.localEulerAngles.z);

            Assert.That(actualRotation, Is.EqualTo(expectedRotation).Within(1f),
                $"Expected counter-clockwise rotation ~{expectedRotation}°, but was {actualRotation}°");
        }

        /// <summary>
        /// Simulates no key being pressed and ensures that the turret does not rotate.
        /// Asserts that the rotation angle remains effectively unchanged.
        /// </summary>
        [UnityTest]
        public IEnumerator NoRotation_NoKeyPressed()
        {
            yield return SetupSceneAndWeapon();
            _towerBase.localEulerAngles = Vector3.zero;
            _weaponInputHandler.SendMessage("OnMouseDown");

            yield return new WaitForSeconds(0.1f);

            float actualRotation = NormalizeRotation(_towerBase.localEulerAngles.z);
            Assert.That(actualRotation, Is.EqualTo(0f).Within(0.1f), "Expected no rotation when no keys are pressed.");
        }

        [UnityTest]
        public IEnumerator AdjustAngle_IKeyPressed_StopsAtMaxAngle()
        {
            yield return SetupSceneAndWeapon();

            var cannonShaft = _weaponInstance.transform.Find("Geschuetzturm/Lauf");
            Assert.IsNotNull(cannonShaft, "Cannon shaft transform not found!");

            // Start at maximum pitch angle
            cannonShaft.localEulerAngles = new Vector3(_minWeaponAngle, 0, 0);

            _weaponInputHandler.SendMessage("OnMouseDown"); // enter fight mode

            Press(_keyboard.iKey); // Press K key to decrease angle

            yield return new WaitForSeconds(2.0f);

            Release(_keyboard.iKey);
            yield return null;

            float pitch = cannonShaft.localRotation.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            Assert.That(pitch, Is.LessThanOrEqualTo(_maxWeaponAngle + 0.1f),
                $"Pitch angle exceeded max limit: {pitch} > {_maxWeaponAngle}");
        }

        [UnityTest]
        public IEnumerator AdjustAngle_KKeyPressed_StopsAtMinAngle()
        {
            yield return SetupSceneAndWeapon();

            var cannonShaft = _weaponInstance.transform.Find("Geschuetzturm/Lauf");
            Assert.IsNotNull(cannonShaft, "Cannon shaft transform not found!");

            // Start at maximum pitch angle
            cannonShaft.localEulerAngles = new Vector3(_maxWeaponAngle, 0, 0);

            _weaponInputHandler.SendMessage("OnMouseDown"); // enter fight mode

            Press(_keyboard.kKey); // Press K key to decrease angle

            yield return new WaitForSeconds(2.0f); // wait enough time to try go beyond min

            Release(_keyboard.kKey);
            yield return null;

            float pitch = cannonShaft.localRotation.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            Assert.That(pitch, Is.GreaterThanOrEqualTo(_minWeaponAngle - 0.1f),
                $"Pitch angle exceeded min limit: {pitch} < {_minWeaponAngle}");
        }

        /// <summary>
        /// Cleans up the weapon instance after each test to avoid memory leaks or leftover state.
        /// This method is called after each test completes.
        /// </summary>
        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            if (_weaponInstance != null)
                Object.Destroy(_weaponInstance);
        }
    }
}