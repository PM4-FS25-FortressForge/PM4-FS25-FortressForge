using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
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
        private WeaponBuildingTemplate _testConstants;
        private Keyboard _keyboard;
        private float TestsDelayTime = 0f; // Delay time for the tests to wait for the camera to move (Change this for optical debuging to 0.5f)


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

            _testConstants = GetWeaponConstantsFromPrefabInstance(_weaponInputHandler);
            Assert.IsNotNull(_testConstants, "WeaponBuildingTemplate reference on prefab is null.");

            _towerBase = _weaponInstance.transform.Find("Geschuetzturm");
            Assert.IsNotNull(_towerBase, "'Geschuetzturm' transform not found!");
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

        private IEnumerator WaitUntilTimeout(Func<bool> condition, float timeoutSeconds = 5f)
        {
            float startTime = Time.time;
            while (!condition() && Time.time - startTime < timeoutSeconds)
                yield return null;

            Assert.IsTrue(condition(), $"Condition not met within {timeoutSeconds} seconds");
        }

        private int GetCurrentAmmo(object weapon)
        {
            var currentAmmoField =
                weapon.GetType().GetField("_currentAmmo", BindingFlags.NonPublic | BindingFlags.Instance);
            if (currentAmmoField == null)
                throw new Exception("_currentAmmo field not found");
            return (int)currentAmmoField.GetValue(weapon);
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
            _weaponInputHandler.SendMessage("OnMouseDown"); // Enter fight mode to enable input actions

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
            var cannonShaft = _weaponInstance.transform.Find("Geschuetzturm/Lauf");
            Assert.IsNotNull(cannonShaft, "Cannon shaft transform not found!");
            cannonShaft.localEulerAngles = new Vector3(_testConstants.minCannonAngle, 0, 0);    // Start at maximum pitch angle
            _weaponInputHandler.SendMessage("OnMouseDown"); // Enter fight mode to enable input actions
            yield return null;
            
            //Adjust angle up (to max.)
            Press(_keyboard.iKey); 
            yield return new WaitForSeconds(TestsDelayTime);
            Release(_keyboard.iKey);
            yield return null;

            float pitch = cannonShaft.localRotation.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            Assert.That(pitch, Is.LessThanOrEqualTo(_testConstants.maxCannonAngle + 0.1f),
                $"Pitch angle exceeded max limit: {pitch} > {_testConstants.maxCannonAngle}");
        }

        [UnityTest]
        public IEnumerator AdjustAngle_KKeyPressed_StopsAtMinAngle()
        {
            yield return SetupSceneAndWeapon();
            var cannonShaft = _weaponInstance.transform.Find("Geschuetzturm/Lauf");
            Assert.IsNotNull(cannonShaft, "Cannon shaft transform not found!");
            cannonShaft.localEulerAngles = new Vector3(_testConstants.maxCannonAngle, 0, 0);    // Start at maximum pitch angle
            _weaponInputHandler.SendMessage("OnMouseDown"); // Enter fight mode to enable input actions

            //Adjust angle down (to min.)
            Press(_keyboard.kKey); 
            yield return new WaitForSeconds(TestsDelayTime); 
            Release(_keyboard.kKey);
            yield return null;

            float pitch = cannonShaft.localRotation.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            Assert.That(pitch, Is.GreaterThanOrEqualTo(_testConstants.minCannonAngle - 0.1f),
                $"Pitch angle below min limit: {pitch} < {_testConstants.minCannonAngle}");
        }

        [UnityTest]
        public IEnumerator FireWeapon_SpaceKeyPressed_FiresOnce()
        {
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Exception, new Regex("NullReferenceException.*GameRoomSynchronisation"));

            yield return SetupSceneAndWeapon();

            int initialAmmo = GetCurrentAmmo(_weaponInputHandler);
            _weaponInputHandler.SendMessage("OnMouseDown");

            Press(_keyboard.spaceKey);
            yield return null;
            Release(_keyboard.spaceKey);
            yield return new WaitForSeconds(TestsDelayTime);    

            int ammoAfterFire = GetCurrentAmmo(_weaponInputHandler);

            Assert.AreEqual(initialAmmo - 1, ammoAfterFire,
                $"Expected ammo to decrease by 1 after firing. Actual: {ammoAfterFire}");
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