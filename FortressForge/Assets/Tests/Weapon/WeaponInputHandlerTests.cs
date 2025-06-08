using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using FishNet;
using Object = UnityEngine.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.Weapons;
using GameObject = UnityEngine.GameObject;

namespace Tests.Weapon
{
    public class WeaponInputHandlerRotationTests : InputTestFixture
    {
        private const string WEAPON_TESTS = "WeaponTests";
        private GameObject _weaponInstance;
        private WeaponInputHandler _weaponInputHandler;
        private Transform _towerBase;
        private Transform _cannonShaft;
        private WeaponBuildingTemplate _testConstants;
        private Keyboard _keyboard;
        private Mouse _mouse;

        private float
            TestsDelayTime =
                0f; // Delay time for the tests to wait for the camera to move (Change this for optical debuging to 0.5f)

        /// <summary>
        /// Registers the keyboard input device and prepares the input system for test execution.
        /// This method is called before each test runs.
        /// </summary>
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            InputSystem.RegisterLayout<Keyboard>();
            InputSystem.AddDevice<Keyboard>();
            Assert.That(InputSystem.devices, Has.Exactly(1).TypeOf<Keyboard>());
            _keyboard = InputSystem.GetDevice<Keyboard>();
            Assert.NotNull(_keyboard, "Error: Keyboard device not found.");

            InputSystem.AddDevice<Mouse>();
            _mouse = InputSystem.GetDevice<Mouse>();
            Assert.NotNull(_mouse, "Error: Mouse device not found.");
        }

        /// <summary>
        /// Loads a minimal test scene and sets up the networking context using FishNet.
        /// It also instantiates and spawns the weapon prefab and configures its input handler and rotation parameters.
        /// </summary>
        private IEnumerator SetupSceneAndWeapon()
        {
            // Load test scene
            SceneManager.LoadScene(WEAPON_TESTS);
            yield return new WaitUntil(
                () => SceneManager.GetActiveScene().name == WEAPON_TESTS,
                new TimeSpan(0, 0, 10),
                () => Assert.AreEqual(WEAPON_TESTS, SceneManager.GetActiveScene().name,
                    "Failed to load GameOverlay scene within the timeout period.")
            );

            var networkManagerPrefab = Resources.Load<GameObject>("Prefabs/NetworkManager");
            if (networkManagerPrefab != null)
            {
                var networkManagerInstance = Object.Instantiate(networkManagerPrefab);
                networkManagerInstance.name = "TestNetworkManager";

                yield return null;

                if (!InstanceFinder.ServerManager.Started)
                {
                    InstanceFinder.ServerManager.StartConnection();
                    yield return new WaitUntil(() => InstanceFinder.ServerManager.Started);
                }

                if (!InstanceFinder.ClientManager.Started)
                {
                    InstanceFinder.ClientManager.StartConnection();
                    yield return new WaitUntil(() => InstanceFinder.ClientManager.Started);
                }
            }

            SpawnWeaponBuildingPrefab();
            yield return null;

            _weaponInputHandler.GetComponent<PlayerInput>()?.ActivateInput();
        }
        
        /// <summary>
        /// Simulates pressing the 'L' key to rotate the weapon turret to the right (clockwise).
        /// Asserts that the turret rotated approximately by the expected degrees based on rotation speed and delta time.
        /// </summary>
        [UnityTest]
        public IEnumerator RotateRight_LKeyPressed()
        {
            //Setup
            yield return SetupSceneAndWeapon();
            _towerBase.localEulerAngles = Vector3.zero;
            _weaponInputHandler.SendMessage("OnMouseDown"); // Enter fight mode to enable input actions

            // Rotate right
            Press(_keyboard.lKey);
            yield return new WaitForSeconds(TestsDelayTime);
            Release(_keyboard.lKey);
            yield return null;

            float expectedRotation = _testConstants.rotationSpeed * Time.deltaTime;
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
            //Setup
            yield return SetupSceneAndWeapon();
            _towerBase.localEulerAngles = Vector3.zero;
            _weaponInputHandler.SendMessage("OnMouseDown");

            // rotate left
            Press(_keyboard.jKey);
            yield return new WaitForSeconds(TestsDelayTime);
            Release(_keyboard.jKey);
            yield return null;

            float expectedRotation = -_testConstants.rotationSpeed * Time.deltaTime;
            float actualRotation = NormalizeRotation(_towerBase.localEulerAngles.z);

            Assert.That(actualRotation, Is.EqualTo(expectedRotation).Within(1f),
                $"Expected counter-clockwise rotation ~{expectedRotation}°, but was {actualRotation}°");
        }

        [UnityTest]
        public IEnumerator AdjustAngle_IKeyPressed_StopsAtMaxAngle()
        {
            yield return SetupSceneAndWeapon();

            _cannonShaft.localEulerAngles =
                new Vector3(_testConstants.minCannonAngle, 0, 0); // Start at maximum pitch angle
            _weaponInputHandler.SendMessage("OnMouseDown"); // Enter fight mode to enable input actions
            yield return null;

            //Adjust angle up (to max.)
            Press(_keyboard.iKey);
            yield return new WaitForSeconds(TestsDelayTime);
            Release(_keyboard.iKey);
            yield return null;

            float pitch = _cannonShaft.localRotation.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            Assert.That(pitch, Is.LessThanOrEqualTo(_testConstants.maxCannonAngle + 0.1f),
                $"Pitch angle exceeded max limit: {pitch} > {_testConstants.maxCannonAngle}");
        }

        [UnityTest]
        public IEnumerator AdjustAngle_KKeyPressed_StopsAtMinAngle()
        {
            yield return SetupSceneAndWeapon();

            _cannonShaft.localEulerAngles =
                new Vector3(_testConstants.maxCannonAngle, 0, 0); // Start at maximum pitch angle
            _weaponInputHandler.SendMessage("OnMouseDown"); // Enter fight mode to enable input actions

            //Adjust angle down (to min.)
            Press(_keyboard.kKey);
            yield return new WaitForSeconds(TestsDelayTime);
            Release(_keyboard.kKey);
            yield return null;

            float pitch = _cannonShaft.localRotation.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            Assert.That(pitch, Is.GreaterThanOrEqualTo(_testConstants.minCannonAngle - 0.1f),
                $"Pitch angle below min limit: {pitch} < {_testConstants.minCannonAngle}");
        }

        [UnityTest]
        public IEnumerator FireWeapon_FiresOnce()
        {
            yield return SetupSceneAndWeapon();

            // Enter fight mode
            _weaponInputHandler.SendMessage("OnMouseDown");
            yield return null;

            // Wait until CanFire returns true
            //yield return new WaitUntil(() => _weaponInputHandler.Test_CanFire());

            int initialAmmo = _weaponInputHandler.GetCurrentAmmo();

            // Press and release space to fire
            Press(_keyboard.spaceKey);
            yield return null;
            Release(_keyboard.spaceKey);

            // rotate left to stop the Autofiring
            Press(_keyboard.jKey);
            yield return new WaitForSeconds(TestsDelayTime);
            Release(_keyboard.jKey);
            yield return null;

            // Wait until ammo has changed or timeout
            yield return new WaitUntil(() => _weaponInputHandler.GetCurrentAmmo() < initialAmmo);

            int ammoAfterFire = _weaponInputHandler.GetCurrentAmmo();

            Assert.AreEqual(initialAmmo - 1, ammoAfterFire,
                $"Expected ammo to decrease by 1 after firing. Actual: {ammoAfterFire}");
        }

        private void SpawnWeaponBuildingPrefab()
        {
            // Load and instantiate prefab
            GameObject weaponPrefab = Resources.Load<GameObject>("Prefabs/Tier_1/Tier_1_Konzept");
            Assert.IsNotNull(weaponPrefab, "Weapon prefab not found in Resources.");

            _weaponInstance = Object.Instantiate(weaponPrefab);
            Assert.IsNotNull(_weaponInstance, "Weapon prefab instance could not be created.");

            // Ensure ammunition is present
            GameObject ammoPrefab = Resources.Load<GameObject>("Prefabs/Ammunition/CannonBall");
            Assert.IsNotNull(ammoPrefab,
                "Ammunition prefab could not be loaded. Make sure it exists in a Resources folder.");

            // Get and check WeaponInputHandler
            _weaponInputHandler = _weaponInstance.GetComponent<WeaponInputHandler>();
            Assert.IsNotNull(_weaponInputHandler, "WeaponInputHandler component not found on the weapon prefab.");

            // Optional: If it expects registration, but isn't required for test logic, skip or mock
            WeaponBuildingManager.Instance?.RegisterWeaponBuilding(_weaponInputHandler);

            // Get template constants using reflection
            _testConstants = GetWeaponConstantsFromPrefabInstance(_weaponInputHandler);
            Assert.IsNotNull(_testConstants, "WeaponBuildingTemplate reference on prefab is null.");

            // Get transforms for turret base and cannon shaft
            _towerBase = _weaponInstance.transform.Find("Geschuetzturm");
            Assert.IsNotNull(_towerBase, "'Geschuetzturm' transform not found!");

            _cannonShaft = _weaponInstance.transform.Find("Geschuetzturm/Lauf");
            Assert.IsNotNull(_cannonShaft, "Cannon shaft transform not found!");
        }


        private WeaponBuildingTemplate GetWeaponConstantsFromPrefabInstance(WeaponInputHandler handler)
        {
            // Use reflection to get the private field _constants from WeaponInputHandler
            var constantsField = typeof(WeaponInputHandler)
                .GetField("_constants", BindingFlags.NonPublic | BindingFlags.Instance);

            if (constantsField == null)
                throw new Exception("Field '_constants' not found in WeaponInputHandler.");

            var constants = (WeaponBuildingTemplate)constantsField.GetValue(handler);
            return constants;
        }

        /// <summary>
        /// Normalizes a given angle to be within the range [0, 360).
        /// This is useful for comparing angles regardless of how Unity internally stores them.
        /// </summary>
        private float NormalizeRotation(float angle) => (angle + 360f) % 360f;

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