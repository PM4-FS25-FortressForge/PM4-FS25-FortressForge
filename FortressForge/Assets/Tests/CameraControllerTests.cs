using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using GameObject = UnityEngine.GameObject;

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

public class CameraControllerTest
{
    private GameObject _mainCamera;
    private Vector3 _initialPosition;
    private float _yInitialRotation;
    private float _xInitialRotation;
    private float _zInitialRotation;
    private float _initialZoom;
    private HashSet<Key> _activeKeys;
    private PlayerInput _playerInput;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        _activeKeys = new HashSet<Key>(); //Reset the active keys
        string testScene = "Unity Cammera implementation Test";
        UnityEngine.SceneManagement.SceneManager.LoadScene(testScene);
        yield return null; 

        // Ensure the scene is actually loaded
        Scene loadedScene = SceneManager.GetActiveScene();
        Assert.AreEqual(testScene, loadedScene.name, 
            $"Error: Scene did not load correctly. Loaded: {loadedScene.name}");

        // Find the main camera in the loaded scene
        _mainCamera = GameObject.Find("Main Camera");
        Assert.IsNotNull(_mainCamera, "Error: Main Camera was not found in the scene.");

        // Save initial position & rotation
        _initialPosition = _mainCamera.transform.position;
        _yInitialRotation = _mainCamera.transform.eulerAngles.y;
        _xInitialRotation = _mainCamera.transform.eulerAngles.x;
        _zInitialRotation = _mainCamera.transform.eulerAngles.z;
    }



    [UnityTest]
    public IEnumerator TestCameraMovesForward_WhenPressingW()
    {
        PressKey(Key.W);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.W);

        Assert.Greater(_mainCamera.transform.position.z, _initialPosition.z, "Camera should move forward when W is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraMovesLeft_WhenPressingA()
    {
        PressKey(Key.A);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.A);

        Assert.Less(_mainCamera.transform.position.x, _initialPosition.x, "Camera should move left when A is pressed.");
    }
    [UnityTest]
    public IEnumerator TestCameraMovesRight_WhenPressingS()
    {
        PressKey(Key.S);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.S);

        Assert.Less(_mainCamera.transform.position.z, _initialPosition.z, "Camera should move Down when S is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraMovesRight_WhenPressingD()
    {
        PressKey(Key.D);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.D);

        Assert.Greater(_mainCamera.transform.position.x, _initialPosition.x, "Camera should move right when D is pressed.");
    }
    
    [UnityTest]
    public IEnumerator TestCameraRotatesLeft_WhenPressingQ()
    {
        PressKey(Key.Q);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.Q);

        float newRotation = _mainCamera.transform.eulerAngles.y;
        Assert.Greater(newRotation, _yInitialRotation, "Camera should rotate left when Q is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraRotatesRight_WhenPressingE()
    {
        PressKey(Key.E);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.E);

        float newRotation = _mainCamera.transform.eulerAngles.y;
        Assert.Greater(newRotation, _yInitialRotation, "Camera should rotate right when E is pressed.");
    }
    
    [UnityTest]
    public IEnumerator TestCameraPitchUp_WhenPressingArrowUp()
    {
        PressKey(Key.UpArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.UpArrow);

        float newPitch = _mainCamera.transform.eulerAngles.x;
        Assert.Greater(newPitch, _xInitialRotation, "Camera should pitch up when UpArrow is pressed.");
    }
    [UnityTest]
    public IEnumerator TestCameraPitchDown_WhenPressingArrowDown()
    {
        PressKey(Key.DownArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.DownArrow);

        float newPitch = _mainCamera.transform.eulerAngles.x;
        Assert.Less(newPitch, _xInitialRotation, "Camera should pitch Down when DownArrow is pressed.");
    }
    
    [UnityTest]
    public IEnumerator TestCameraZoomOut_WhenPressingArrowLeft()
    {
        PressKey(Key.LeftArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.LeftArrow);
    
        Vector3 newPosition = _mainCamera.transform.position;
        Vector3 newCalculatedPosition = new Vector3(0, 5.6568532f, -5.65685272f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should zoom Out when LeftArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should zoom Out when LeftArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should zoom Out when LeftArrow is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraZoomsIn_WhenPressingArrowRight()
    {
        PressKey(Key.RightArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.RightArrow);
    
        Vector3 newPosition = _mainCamera.transform.position;
        Vector3 newCalculatedPosition = new Vector3(0, 2.82842875f, -2.82842827f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should zoom In when RightArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should zoom In when RightArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should zoom In when RightArrow is pressed.");
    }
    
    [UnityTest]
    public IEnumerator TestCameraZoomMaxOutToBorder_WhenPressingArrowLeft()
    {
        PressKey(Key.LeftArrow);
        yield return new WaitForSeconds(5f);
        ReleaseKey(Key.LeftArrow);
    
        Vector3 newPosition = _mainCamera.transform.position;
        Vector3 newCalculatedPosition = new Vector3(0, 14.1421366f, -14.1421347f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should zoom Out till it reaches the boarder when LeftArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should zoom Out till it reaches the boarder when LeftArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should zoom Out till it reaches the boarder when LeftArrow is pressed.");
    }
    
    [UnityTest]
    public IEnumerator TestCameraZoomsMaxInToBorder_WhenPressingArrowRight()
    {
        PressKey(Key.RightArrow);
        yield return new WaitForSeconds(3f);
        ReleaseKey(Key.RightArrow);
    
        Vector3 newPosition = _mainCamera.transform.position;
        Vector3 newCalculatedPosition = new Vector3(0, 1.41421366f, -1.41421342f);
        Debug.Log(newPosition);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should zoom In till it reaches the boarder when RightArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should zoom In till it reaches the boarder when RightArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should zoom In till it reaches the boarder when RightArrow is pressed.");
    }
    
    [UnityTest]
    public IEnumerator TestCameraCombinedMovement()
    {
        //This Test does also test the boarder of the min or max Pitch of the up or down arrow keys
        PressKey(Key.W);
        PressKey(Key.D);
        PressKey(Key.E);
        PressKey(Key.UpArrow);
        PressKey(Key.LeftArrow);
        yield return new WaitForSeconds(2.5f);
        ReleaseKey(Key.W);
        ReleaseKey(Key.D);
        ReleaseKey(Key.E);
        ReleaseKey(Key.UpArrow);
        ReleaseKey(Key.LeftArrow);

        Vector3 newPosition = _mainCamera.transform.position;
        Vector3 newCalculatedPosition = new Vector3(-2.74307966f, 15.9975538f, 9.88054848f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should move to new x position.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should move to new y position.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should move to new z position.");

    }
    [UnityTest]
    public IEnumerator TestCameraCombinedInversedMovement()
    {
        //This Test does also test the boarder of the min or max Pitch of the up or down arrow keys
        PressKey(Key.S);
        PressKey(Key.A);
        PressKey(Key.Q);
        PressKey(Key.DownArrow);
        PressKey(Key.RightArrow);
        yield return new WaitForSeconds(2.5f);
        ReleaseKey(Key.S);
        ReleaseKey(Key.A);
        ReleaseKey(Key.Q);
        ReleaseKey(Key.DownArrow);
        ReleaseKey(Key.RightArrow);

        Vector3 newPosition = _mainCamera.transform.position;
        Vector3 newCalculatedPosition = new Vector3(-11.3586884f, 0f, 4.11897087f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should move to new x position.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should move to new y position.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should move to new z position.");

    }

    [TearDown]
    public void TearDown()
    {
        // Destroy the test camera after each test
        Object.Destroy(_mainCamera);
    }

    private void PressKey(Key key)      //Can handle multiple keys at the same time (Only presses the key down)
    {
        _activeKeys.Add(key); // Add key to active set

        KeyboardState state = new KeyboardState();
        foreach (Key k in _activeKeys)
        {
            state.Set(k, true);
        }

        InputSystem.QueueStateEvent(Keyboard.current, state);
        InputSystem.Update();
    }

    private void ReleaseKey(Key key)    //Can handle multiple keys at the same time (Only released the key down)
    {
        _activeKeys.Remove(key); // Remove key from active set

        KeyboardState state = new KeyboardState();
        foreach (Key k in _activeKeys)
        {
            state.Set(k, true); // Keep other keys pressed
        }

        InputSystem.QueueStateEvent(Keyboard.current, state);
        InputSystem.Update();
    }
}

