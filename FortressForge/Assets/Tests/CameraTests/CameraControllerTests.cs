using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using GameObject = UnityEngine.GameObject;

public class CameraMovementTest
{
    private GameObject cameraObject;
    private Camera _camera;
    private GameObject _cameraController;
    
    private Vector3 initialPosition;
    private float initialRotation;
    private PlayerInput playerInput;

    [SetUp]
    public void Setup()
    {
        // Create a new Camera object for the test
        cameraObject = new GameObject("TestCamera");
        _cameraController = new GameObject("CameraController");
        if (_cameraController == null)
        {
            _cameraController = new GameObject("CameraController");
        }
        _camera = cameraObject.AddComponent<Camera>(); // Add Camera component
        cameraObject.tag = "MainCamera"; // Optional but useful
        

        // Debug check to ensure CameraController is accessible
        var assembly = Assembly.Load("Assembly-CSharp");
        var type = assembly.GetType("CameraController");
        Assert.IsNotNull(type, "Error: CameraController class was not found in Assembly-CSharp!");

        // Add CameraController script dynamically
        //_cameraController = cameraObject.AddComponent<CameraController>();
        Assert.IsNotNull(_cameraController, "Error: Failed to add CameraController to the GameObject.");

        // Add PlayerInput component
        playerInput = cameraObject.AddComponent<PlayerInput>();

        // Load InputActions from Resources (Ensure it's placed in a Resources folder)
        playerInput.actions = Resources.Load<InputActionAsset>("CameraInputActions");
        Assert.IsNotNull(playerInput.actions, "Error: CameraInputActions could not be loaded. Ensure it's in Resources/");

        // Assign the PlayerInput to the CameraController
        //_cameraController.enabled = true;

        // Save initial position & rotation
        initialPosition = cameraObject.transform.position;
        initialRotation = cameraObject.transform.eulerAngles.y;
    }

    [UnityTest]
    public IEnumerator TestCameraMovesForward_WhenPressingW()
    {
        PressKey(Key.W);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.W);

        Assert.Greater(cameraObject.transform.position.z, initialPosition.z, "Camera should move forward when W is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraMovesLeft_WhenPressingA()
    {
        PressKey(Key.A);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.A);

        Assert.Less(cameraObject.transform.position.x, initialPosition.x, "Camera should move left when A is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraMovesRight_WhenPressingD()
    {
        PressKey(Key.D);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.D);

        Assert.Greater(cameraObject.transform.position.x, initialPosition.x, "Camera should move right when D is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraRotatesLeft_WhenPressingQ()
    {
        PressKey(Key.Q);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.Q);

        float newRotation = cameraObject.transform.eulerAngles.y;
        Assert.Less(newRotation, initialRotation, "Camera should rotate left when Q is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraRotatesRight_WhenPressingE()
    {
        PressKey(Key.E);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.E);

        float newRotation = cameraObject.transform.eulerAngles.y;
        Assert.Greater(newRotation, initialRotation, "Camera should rotate right when E is pressed.");
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy the test camera after each test
        Object.Destroy(cameraObject);
    }

    private void PressKey(Key key)
    
    {
        KeyboardState state = new KeyboardState();
        state.Set(key, true);
        InputSystem.QueueStateEvent(Keyboard.current, state);
        InputSystem.Update();
    }

    private void ReleaseKey(Key key)
    {
        KeyboardState state = new KeyboardState();
        state.Set(key, false);
        InputSystem.QueueStateEvent(Keyboard.current, state);
        InputSystem.Update();
    }
}
