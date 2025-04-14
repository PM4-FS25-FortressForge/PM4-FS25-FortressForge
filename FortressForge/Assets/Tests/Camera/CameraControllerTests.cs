using System;
using System.Collections;
using FortressForge.CameraControll;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using GameObject = UnityEngine.GameObject;
using Object = UnityEngine.Object;

namespace Tests.Camera
{
    /// <summary>
    /// This class contains unit tests for the CameraController class in Unity.
    /// The tests verify camera movement, rotation, and zoom functionality 
    /// based on keyboard input.
    /// </summary>
    /// <remarks>
    /// To run these tests successfully, ensure that the Unity Camera 
    /// Implementation Test Scene is loaded in the Unity Editor.
    /// </remarks>
    /// <example>
    /// <para>Setup Instructions:</para>
    /// <list type="bullet">
    ///   <item>Manually open the "Unity Camera Implementation Test" scene.</item>
    ///   <item>If the tests fail due to a missing scene, go to Unity Editor:</item>
    ///   <item>File &gt; Build Settings &gt; Add Open Scenes.</item>
    /// </list>
    /// </example>
    /// <para>
    /// Each test simulates key presses using Unity's Input System and verifies 
    /// that the camera reacts correctly. The movement, zoom and rotation limits 
    /// are also validated.
    /// </para>
    /// <author>Hoferlev</author>
    /// <version>1.0</version>
    [TestFixture]
    public class CameraControllerTest : InputTestFixture
    {
        private GameObject _mainCamera;
        private Vector3 _initialPosition;
        private float _yInitialRotation;
        private float _xInitialRotation;
        private float _zInitialRotation;
        private float _initialZoom;
        private CameraController _cameraController;

        private Keyboard _keyboard; // Store the virtual keyboard
        private Mouse _mouse; // Store the virtual mouse
        private InputTestFixture _inputTestFixture;
        private const string TEST_SCENE = "Unity Cammera implementation Test";

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

        private IEnumerator SetupCustom()
        {
            SceneManager.LoadScene(TEST_SCENE);
            yield return null;

            // Ensure the scene is actually loaded
            Scene loadedScene = SceneManager.GetActiveScene();
            Assert.AreEqual(TEST_SCENE, loadedScene.name, $"Error: Scene did not load correctly. Loaded: {loadedScene.name}");

            // Find the main camera in the loaded scene
            _mainCamera = GameObject.Find("Main Camera");
            Assert.IsNotNull(_mainCamera, "Error: Main Camera was not found in the scene.");

            // Save the CameraController component
            _cameraController = _mainCamera.GetComponent<CameraController>();
        }

        private void SetInitialCameraValuesForEachTest()
        {
            // Setting initial values in CameraController
            _cameraController.SetYaw(0.0f);
            _cameraController.SetPitch(45);
            _cameraController.SetZoom(6f);
            _cameraController.SetTargetPosition(Vector3.zero);
            _cameraController.SetMoveSpeed(7.5f);
            _cameraController.SetRotationSpeed(60.0f);
            _cameraController.SetPitchSpeed(45.0f);
            _cameraController.SetZoomSpeed(3.0f);
            _cameraController.SetPitchLimits(new Vector2(89, 0));
            _cameraController.SetZoomLimits(new Vector2(2.0f, 20.0f));

            // Save initial position & rotation
            _initialPosition = _mainCamera.transform.position;
            _yInitialRotation = _mainCamera.transform.eulerAngles.y;
            _xInitialRotation = _mainCamera.transform.eulerAngles.x;
            _zInitialRotation = _mainCamera.transform.eulerAngles.z;
        }

        [UnityTest]
        public IEnumerator TestCameraMovesForward_WhenPressingW()
        {
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.wKey);
            yield return new WaitForSeconds(0.5f); // Let the system react to the input
            Release(_keyboard.wKey);

            Assert.Greater(_mainCamera.transform.position.z, _initialPosition.z, "Camera should move forward when W is pressed.");
        }

        [UnityTest]
        public IEnumerator TestCameraMovesLeft_WhenPressingA()
        {
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test    

            Press(_keyboard.aKey);
            yield return new WaitForSeconds(0.5f); // Let the system react to the input
            Release(_keyboard.aKey);

            Assert.Less(_mainCamera.transform.position.x, _initialPosition.x, "Camera should move left when A is pressed.");
        }

        [UnityTest]
        public IEnumerator TestCameraMovesRight_WhenPressingS()
        {
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.sKey);
            yield return new WaitForSeconds(0.5f); // Let the system react to the input
            Release(_keyboard.sKey);

            Assert.Less(_mainCamera.transform.position.z, _initialPosition.z, "Camera should move Down when S is pressed.");
        }

        [UnityTest]
        public IEnumerator TestCameraMovesRight_WhenPressingD()
        {
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.dKey);
            yield return new WaitForSeconds(0.5f); // Let the system react to the input
            Release(_keyboard.dKey);

            Assert.Greater(_mainCamera.transform.position.x, _initialPosition.x, "Camera should move right when D is pressed.");
        }

        [UnityTest]
        public IEnumerator TestCameraRotatesLeft_WhenPressingQ()
        {
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.qKey);
            yield return new WaitForSeconds(0.5f); // Let the system react to the input
            Release(_keyboard.qKey);

            float newRotation = _mainCamera.transform.eulerAngles.y;
            Assert.Greater(newRotation, _yInitialRotation, "Camera should rotate left when Q is pressed.");
        }

        [UnityTest]
        public IEnumerator TestCameraRotatesRight_WhenPressingE()
        {
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.eKey);
            yield return new WaitForSeconds(0.5f); // Let the system react to the input
            Release(_keyboard.eKey);

            float newRotation = _mainCamera.transform.eulerAngles.y;
            Assert.Greater(newRotation, _yInitialRotation, "Camera should rotate right when E is pressed.");
        }

        [UnityTest]
        public IEnumerator TestCameraPitchUp_WhenPressingArrowUp()
        {
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.upArrowKey);
            yield return new WaitForSeconds(0.5f); // Let the system react to the input
            Release(_keyboard.upArrowKey);

            float newPitch = _mainCamera.transform.eulerAngles.x;
            Debug.Log($"New Pitch: {newPitch}, Initial Pitch: {_xInitialRotation}");
            Assert.Greater(newPitch, _xInitialRotation, "Camera should pitch up when UpArrow is pressed.");
        }

        [UnityTest]
        public IEnumerator TestCameraPitchDown_WhenPressingArrowDown()
        {
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            // _xInitialRotation = _mainCamera.transform.eulerAngles.x; //I Know this is technically called 2 times in a row but just leave it this way, so it works
            Press(_keyboard.downArrowKey);
            yield return new WaitForSeconds(0.5f); // Let the system react to the input
            Release(_keyboard.downArrowKey);

            float newPitch = _mainCamera.transform.eulerAngles.x;
            Assert.Less(newPitch, _xInitialRotation, "Camera should pitch Down when DownArrow is pressed.");
        }

        [UnityTest]
        public IEnumerator TestCameraZoomOut_WhenPressingArrowLeft()
        {
            Vector3 newCalculatedPosition = new(0, 5.66f, -5.66f);
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.leftArrowKey);
            yield return new WaitUntil( //Structure is used to wait until the camera has moved to the new position or terminate after realistic time
                () => CompareCameraPosition(newCalculatedPosition, false),
                new TimeSpan(0, 0, 0, 0, 600),
                () => Assert.True(CompareCameraPosition(newCalculatedPosition, false), "Camera should zoom Out when LeftArrow is pressed." + PrintFailedTestMsg(newCalculatedPosition))
            );
            Release(_keyboard.leftArrowKey);

            Assert.True(CompareCameraPosition(newCalculatedPosition, false), "Camera should zoom Out when LeftArrow is pressed." + PrintFailedTestMsg(newCalculatedPosition));
        }

        [UnityTest]
        public IEnumerator TestCameraZoomsIn_WhenPressingArrowRight()
        {
            Vector3 newCalculatedPosition = new(0, 2.83f, -2.83f);
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.rightArrowKey);
            yield return new WaitUntil( //Structure is used to wait until the camera has moved to the new position or terminate after realistic time
                () => CompareCameraPosition(newCalculatedPosition, true),
                new TimeSpan(0, 0, 0, 0, 600),
                () => Assert.True(CompareCameraPosition(newCalculatedPosition, true), "Camera should zoom In when RightArrow is pressed." + PrintFailedTestMsg(newCalculatedPosition))
            );
            Release(_keyboard.rightArrowKey);

            Assert.True(CompareCameraPosition(newCalculatedPosition, true), "Camera should zoom Out In RightArrow is pressed." + PrintFailedTestMsg(newCalculatedPosition));
        }

        [UnityTest]
        public IEnumerator TestCameraZoomMaxOutToBorder_WhenPressingArrowLeft()
        {
            Vector3 newCalculatedPosition = new(0, 14.1f, -14.1f);
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.leftArrowKey);
            yield return new WaitUntil( //Structure is used to wait until the camera has moved to the new position or terminate after realistic time
                () => CompareCameraPosition(newCalculatedPosition, false),
                new TimeSpan(0, 0, 0, 5, 600),
                () => Assert.True(CompareCameraPosition(newCalculatedPosition, false), "Camera should zoom Out till it reaches the boarder when LeftArrow is pressed" + PrintFailedTestMsg(newCalculatedPosition))
            );
            Release(_keyboard.leftArrowKey);

            Assert.True(CompareCameraPosition(newCalculatedPosition, false), "Camera should zoom Out till it reaches the boarder when LeftArrow is pressed" + PrintFailedTestMsg(newCalculatedPosition));
        }

        [UnityTest]
        public IEnumerator TestCameraZoomsMaxInToBorder_WhenPressingArrowRight()
        {
            Vector3 newCalculatedPosition = new(0, 1.42f, -1.42f);
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            Press(_keyboard.rightArrowKey);
            yield return new WaitUntil( //Structure is used to wait until the camera has moved to the new position or terminate after realistic time
                () => CompareCameraPosition(newCalculatedPosition, true),
                new TimeSpan(0, 0, 0, 3, 0),
                () => Assert.True(CompareCameraPosition(newCalculatedPosition, true),
                    "Camera should zoom In till it reaches the boarder when RightArrow is pressed" + PrintFailedTestMsg(newCalculatedPosition))
            );
            Release(_keyboard.rightArrowKey);

            Assert.True(CompareCameraPosition(newCalculatedPosition, true), "Camera should zoom In till it reaches the boarder when RightArrow is pressed" + PrintFailedTestMsg(newCalculatedPosition));
        }

        [UnityTest]
        public IEnumerator TestCameraCombinedMovement()
        {
            Vector3 newCalculatedPosition = new(-2.74f, 16.0f, 9.88f);
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            //This Test does also test the boarder of the min or max Pitch of the up or down arrow keys
            Press(_keyboard.wKey);
            yield return null; // somehow these are needed in the combined movement tests otherwise they are probably not correctly registered in the update method
            Press(_keyboard.dKey);
            yield return null;
            Press(_keyboard.eKey);
            yield return null;
            Press(_keyboard.upArrowKey);
            yield return null;
            Press(_keyboard.leftArrowKey);
            yield return new WaitUntil( //Structure is used to wait until the camera has moved to the new position or terminate after realistic time
                () => CompareCameraPosition(newCalculatedPosition, false),
                new TimeSpan(0, 0, 0, 2, 500),
                () => Assert.True(CompareCameraPosition(newCalculatedPosition, false), "Camera should move to new position moving with W A S D UpArrow LeftArrow Keys." + PrintFailedTestMsg(newCalculatedPosition))
            );
            Release(_keyboard.wKey);
            Release(_keyboard.dKey);
            Release(_keyboard.eKey);
            Release(_keyboard.upArrowKey);
            Release(_keyboard.leftArrowKey);

            Assert.True(CompareCameraPosition(newCalculatedPosition, false), "Camera should move to new position moving with W D E UpArrow LeftArrow Keys." + PrintFailedTestMsg(newCalculatedPosition));
        }

        [UnityTest]
        public IEnumerator TestCameraCombinedInversedMovement()
        {
            Vector3 newCalculatedPosition = new(-11.36f, 0f, 4.12f);
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            //This Test does also test the boarder of the min or max Pitch of the up or down arrow keys
            Press(_keyboard.sKey);
            yield return null; // somehow these are needed in the combined movement tests otherwise they are probably not correctly registered in the update method
            Press(_keyboard.aKey);
            yield return null;
            Press(_keyboard.qKey);
            yield return null;
            Press(_keyboard.downArrowKey);
            yield return null;
            Press(_keyboard.rightArrowKey);
            yield return new WaitUntil( //Structure is used to wait until the camera has moved to the new position or terminate after realistic time
                () => CompareCameraPosition(newCalculatedPosition, false),
                new TimeSpan(0, 0, 0, 2, 500),
                () => Assert.True(CompareCameraPosition(newCalculatedPosition, false), "Camera should move to new position moving with S A Q DownArrow RightArrow Keys." + PrintFailedTestMsg(newCalculatedPosition))
            );
            Release(_keyboard.sKey);
            Release(_keyboard.aKey);
            Release(_keyboard.qKey);
            Release(_keyboard.downArrowKey);
            Release(_keyboard.rightArrowKey);

            Assert.True(CompareCameraPosition(newCalculatedPosition, false), "Camera should move to new position moving with W A S D UpArrow LeftArrow Keys." + PrintFailedTestMsg(newCalculatedPosition));
        }

        [UnityTest]
        public IEnumerator TestCameraZoomsIn_WhenScrollingMouseWheelUp()
        {
            Vector3 newCalculatedPosition = new(0, 2.83f, -2.83f);
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            for (int i = 0; i < 3; i++) //Simulate mouse wheel scroll
            {
                yield return new WaitForSeconds(0.25f); // Let the system react to the input
                Set(_mouse.scroll.up, 1);
            }

            Assert.True(CompareCameraPosition(newCalculatedPosition, true), "Camera should zoom in when scrolling mouse wheel up" + PrintFailedTestMsg(newCalculatedPosition));
        }

        [UnityTest]
        public IEnumerator TestCameraZoomsOut_WhenScrollingMouseWheelDown()
        {
            Vector3 newCalculatedPosition = new(0, 5.66f, -5.66f);
            yield return SetupCustom(); //Ensure the scene is loaded in each new test and camera is set up
            SetInitialCameraValuesForEachTest(); //Ensure the camera is set to the initial values for each test

            for (int i = 0; i < 5; i++) //Simulate mouse wheel scroll
            {
                yield return new WaitForSeconds(0.25f); // Let the system react to the input
                Set(_mouse.scroll.up, -1);
            }

            Assert.True(CompareCameraPosition(newCalculatedPosition, false), "Camera should zoom out when scrolling mouse wheel down" + PrintFailedTestMsg(newCalculatedPosition));
        }

        /// <summary>
        /// Helper function to compare the new camera position with the expected position
        /// Returns true if the new camera position is greater or less than the expected position
        /// The comparing mode greater or less is set by the boolean parameter compareToGreater
        /// </summary>
        private Boolean CompareCameraPosition(Vector3 newCalculatedPosition, Boolean compareToGreater)
        {
            Boolean result = false;
            if (compareToGreater)
            {
                result = ((Math.Abs(newCalculatedPosition.x) >= Math.Abs(_mainCamera.transform.position.x))) &&
                         (Math.Abs(newCalculatedPosition.y) >= Math.Abs(_mainCamera.transform.position.y)) &&
                         (Math.Abs(newCalculatedPosition.z) >= Math.Abs(_mainCamera.transform.position.z));
            }
            else
            {
                result = ((Math.Abs(newCalculatedPosition.x) <= Math.Abs(_mainCamera.transform.position.x))) &&
                         (Math.Abs(newCalculatedPosition.y) <= Math.Abs(_mainCamera.transform.position.y)) &&
                         (Math.Abs(newCalculatedPosition.z) <= Math.Abs(_mainCamera.transform.position.z));
            }

            return result;
        }

        private string
            PrintFailedTestMsg(Vector3 newCalculatedPosition) // Helper function to print the failed test message including the expected and actual camera position
        {
            return $"\n\nExpected: {newCalculatedPosition} \nbut got: {_mainCamera.transform.position}";
        }

        [TearDown]
        public override void TearDown() // Is called after each test
        {
            // Destroy the test camera and reset all vars after each test
            Object.Destroy(_mainCamera);
            _mainCamera = null;
            _initialPosition = Vector3.zero;
            _yInitialRotation = 0f;
            _xInitialRotation = 0f;
            _zInitialRotation = 0f;
            _initialZoom = 0f;
            base.TearDown();
        }
    }
}