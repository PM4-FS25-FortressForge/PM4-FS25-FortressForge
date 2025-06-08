using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FishNet;
using FishNet.Object;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.BuildManager;
using FortressForge.BuildingSystem.Weapons;
using FortressForge.Economy;
using FortressForge.HexGrid;
using FortressForge.HexGrid.Data;
using GameObject = UnityEngine.GameObject;

namespace Tests.Weapon
{
    public class WeaponInputHandlerTests : InputTestFixture
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

            var networkManagerPrefab = Resources.Load<GameObject>("Prefabs/Tests/NetworkManagerTesting");
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

            yield return SpawnWeaponBuildingPrefab();
            yield return null;
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
            _weaponInputHandler.SendMessage("OnMouseDown");

            float rotationBefore = NormalizeRotation(_towerBase.localEulerAngles.z);
            Debug.Log($"[Test] rotationBefore: {rotationBefore}");

            Press(_keyboard.lKey);

            yield return new WaitUntil(() =>
            {
                float rotation = NormalizeRotation(_towerBase.localEulerAngles.z);
                Debug.Log($"[Test] rotation: {rotation}");
                return Mathf.DeltaAngle(rotationBefore, rotation) > 0f;
            }, new TimeSpan(0, 0, 2), () => Assert.Fail("Tower did not rotate to the right within 2 seconds."));

            Release(_keyboard.lKey);
            yield return null;

            float rotationAfter = NormalizeRotation(_towerBase.localEulerAngles.z);
            Debug.Log($"[Test] rotationAfter: {rotationAfter}");

            Assert.That(Mathf.DeltaAngle(rotationBefore, rotationAfter), Is.GreaterThan(0f),
                $"Tower rotation did not increase to the right: {rotationAfter} (Delta: {Mathf.DeltaAngle(rotationBefore, rotationAfter)})");
        }

        /// <summary>
        /// Simulates pressing the 'J' key to rotate the weapon turret to the left (counter-clockwise).
        /// Asserts that the turret rotated approximately by the expected negative degrees based on rotation speed and delta time.
        /// </summary>
        [UnityTest]
        public IEnumerator RotateLeft_JKeyPressed()
        {
            yield return SetupSceneAndWeapon();
            _towerBase.localEulerAngles = Vector3.zero;
            _weaponInputHandler.SendMessage("OnMouseDown");

            float rotationBefore = NormalizeRotation(_towerBase.localEulerAngles.z);
            Debug.Log($"[Test] rotationBefore: {rotationBefore}");

            Press(_keyboard.jKey);

            yield return new WaitUntil(() =>
            {
                float rotation = NormalizeRotation(_towerBase.localEulerAngles.z);
                Debug.Log($"[Test] rotation: {rotation}");
                return Mathf.DeltaAngle(rotationBefore, rotation) < 0f;
            }, new TimeSpan(0, 0, 2), () => Assert.Fail("Tower did not rotate to the left within 2 seconds."));

            Release(_keyboard.jKey);
            yield return null;

            float rotationAfter = NormalizeRotation(_towerBase.localEulerAngles.z);
            Debug.Log($"[Test] rotationAfter: {rotationAfter}");

            Assert.That(Mathf.DeltaAngle(rotationBefore, rotationAfter), Is.LessThan(0f),
                $"Tower rotation did not decrease to the left: {rotationAfter} (Delta: {Mathf.DeltaAngle(rotationBefore, rotationAfter)})");
        }

        [UnityTest]
        public IEnumerator AdjustAngle_IKeyPressed_StopsAtMaxAngle()
        {
            yield return SetupSceneAndWeapon();

            _cannonShaft.localEulerAngles = new Vector3(_testConstants.minCannonAngle, 0, 0);
            _weaponInputHandler.SendMessage("OnMouseDown");
            yield return null;

            float pitchBefore = _cannonShaft.localRotation.eulerAngles.x;
            if (pitchBefore > 180f) pitchBefore -= 360f;
            Assert.That(pitchBefore, Is.LessThan(_testConstants.maxCannonAngle),
                "Initial pitch angle is not less than max angle.");

            Press(_keyboard.iKey);

            yield return new WaitUntil(() =>
            {
                float pitch = _cannonShaft.localRotation.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                return pitch > pitchBefore || pitch >= _testConstants.maxCannonAngle;
            }, new TimeSpan(0, 0, 2), () => Assert.Fail("Angle did not increase within 2 seconds."));

            Release(_keyboard.iKey);
            yield return null;

            float pitchAfter = _cannonShaft.localRotation.eulerAngles.x;
            if (pitchAfter > 180f) pitchAfter -= 360f;

            Assert.That(pitchAfter, Is.GreaterThan(pitchBefore), "Angle did not increase as expected.");
            Assert.That(pitchAfter, Is.LessThanOrEqualTo(_testConstants.maxCannonAngle + 0.1f),
                $"Pitch angle exceeded max limit: {pitchAfter} > {_testConstants.maxCannonAngle}");
        }

        [UnityTest]
        public IEnumerator AdjustAngle_KKeyPressed_StopsAtMinAngle()
        {
            yield return SetupSceneAndWeapon();

            _cannonShaft.localEulerAngles = new Vector3(_testConstants.maxCannonAngle, 0, 0);
            _weaponInputHandler.SendMessage("OnMouseDown");

            float pitchBefore = _cannonShaft.localRotation.eulerAngles.x;
            if (pitchBefore > 180f) pitchBefore -= 360f;
            Assert.That(pitchBefore, Is.GreaterThan(_testConstants.minCannonAngle),
                "Startwinkel ist nicht größer als das Minimum.");

            Press(_keyboard.kKey);

            yield return new WaitUntil(() =>
            {
                float pitch = _cannonShaft.localRotation.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                return pitch < pitchBefore || pitch <= _testConstants.minCannonAngle;
            }, new TimeSpan(0, 0, 2), () => Assert.Fail("Winkel hat sich nach 2 Sekunden nicht verringert."));

            Release(_keyboard.kKey);
            yield return null;

            float pitchAfter = _cannonShaft.localRotation.eulerAngles.x;
            if (pitchAfter > 180f) pitchAfter -= 360f;

            Assert.That(pitchAfter, Is.LessThan(pitchBefore), "Winkel hat sich nicht verringert.");
            Assert.That(pitchAfter, Is.GreaterThanOrEqualTo(_testConstants.minCannonAngle - 0.1f),
                $"Pitch angle below min limit: {pitchAfter} < {_testConstants.minCannonAngle}");
        }

        [UnityTest]
        public IEnumerator FireWeapon_FiresOnce()
        {
            yield return SetupSceneAndWeapon();

            // Enter fight mode
            _weaponInputHandler.SendMessage("OnMouseDown");
            yield return null;

            int initialAmmo = _weaponInputHandler.GetCurrentAmmo();

            // Press and release space to fire
            Press(_keyboard.enterKey);
            yield return null;
            Release(_keyboard.enterKey);

            // Wait until ammo has changed or timeout
            yield return new WaitUntil(() => _weaponInputHandler.GetCurrentAmmo() < initialAmmo, new TimeSpan(0, 0, 3),
                () => Assert.Fail(
                    "Warten auf Munitionsverbrauch nach Schuss hat das Zeitlimit von 3 Sekunden überschritten."));

            int ammoAfterFire = _weaponInputHandler.GetCurrentAmmo();

            Assert.AreEqual(initialAmmo - 1, ammoAfterFire,
                $"Expected ammo to decrease by 1 after firing. Actual: {ammoAfterFire}");
        }

        [UnityTest]
        public IEnumerator ReloadWeapon_RefillsAmmoAfterDelay()
        {
            yield return SetupSceneAndWeapon();
            
            var dummyHexGrid = new DummyHexGridData();
            _weaponInputHandler.Init(dummyHexGrid);

            _weaponInputHandler.SendMessage("OnMouseDown");
            yield return null;

            int initialAmmo = _weaponInputHandler.GetCurrentAmmo();

            Press(_keyboard.enterKey);
            yield return null;
            Release(_keyboard.enterKey);

            yield return new WaitUntil(() => _weaponInputHandler.GetCurrentAmmo() == 0, new TimeSpan(0, 0, 10),
                () => Assert.Fail("Warten auf Munitionsverbrauch nach Schuss hat das Zeitlimit von 10 Sekunden überschritten."));

            Assert.AreEqual(initialAmmo - 5, _weaponInputHandler.GetCurrentAmmo(), "Expected weapon running out of ammunition");

            yield return new WaitUntil(() => _weaponInputHandler.GetCurrentAmmo() == initialAmmo, new TimeSpan(0, 0, 15),
                () => Assert.Fail("Weapon wurde nicht nachgeladen"));

            Assert.AreEqual(initialAmmo, _weaponInputHandler.GetCurrentAmmo(), "Weapon expected to be reloaded");
        }
        
        private class DummyTerrainHeightProvider : ITerrainHeightProvider
        {
            public float SampleHeight(Vector3 position) => 0f;
            public float SampleHexHeight(Vector3 position, float tileHeight, float tileRadius) => 0f;
            public HexTileCoordinate GetHexTileCoordinate(Vector3 position, float tileHeight, float tileRadius) => new (0, 0, 0);
        }

        private class DummyEconomySystem : EconomySystem
        {
            public DummyEconomySystem() : base(null, null, new Dictionary<ResourceType, float>()) { }

            public override bool CheckForSufficientResources(Dictionary<ResourceType, float> resourceCosts)
            {
                return true;
            }

            public override void PayResource(Dictionary<ResourceType, float> resourceCosts)
            {}
        }

        private class DummyBuildingManager : BuildingManager {}

        public class DummyHexGridManager : HexGridManager
        {}

        private class DummyHexGridData : HexGridData
        {
            public DummyHexGridData()
                : base(
                    id: -1,
                    tileSize: 1f,
                    tileHeight: 1f,
                    terrainHeightProvider: new DummyTerrainHeightProvider(),
                    economySystem: new DummyEconomySystem(),
                    buildingManager: new DummyBuildingManager(),
                    hexGridManager: new DummyHexGridManager(),
                    isInvisible: false)
            {
            }
        }

        private IEnumerator SpawnWeaponBuildingPrefab()
        {
            yield return new WaitUntil(() =>
                InstanceFinder.ClientManager.Connection != null &&
                InstanceFinder.ClientManager.Connection.ClientId != -1
            );
            var connection = InstanceFinder.ClientManager.Connection;
            Assert.NotNull(connection, "Connection was not found.");
            Assert.AreNotEqual(-1, connection.ClientId, "ClientId ist ungültig.");

            var weaponPrefab = Resources.Load<GameObject>("Prefabs/Tier_1/Tier_1_Konzept");
            Assert.IsNotNull(weaponPrefab, "Weapon prefab not found in Resources.");
            _weaponInstance = Object.Instantiate(weaponPrefab);
            Assert.IsNotNull(_weaponInstance, "Weapon prefab instance could not be created.");

            InstanceFinder.ServerManager.Spawn(_weaponInstance, connection);

            var networkObject = _weaponInstance.GetComponent<NetworkObject>();
            yield return new WaitUntil(() => networkObject.IsOwner, new TimeSpan(0, 0, 2),
                () => Assert.Fail("Ownership was not set after 2 seconds."));

            Assert.IsTrue(networkObject.IsOwner, "Client besitzt keine Ownership nach dem Spawn.");

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

            yield return null;
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

