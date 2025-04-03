using System;
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
using Object = UnityEngine.Object;

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
        Setup();        //Ensure the scene is loaded in each new test and camera is set up
        PressKey(Key.W);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.W);

        Assert.Greater(_mainCamera.transform.position.z, _initialPosition.z, "Camera should move forward when W is pressed.");
        TearDown();     //Ensure the scene is unloaded and the camera is destroyed after each test and all vars are cleared
    }

    [UnityTest]
    public IEnumerator TestCameraMovesLeft_WhenPressingA()
    {
        Setup();
        PressKey(Key.A);
        yield return new WaitForSeconds(0.5f);
        //ReleaseKey(Key.A);

        Assert.Less(_mainCamera.transform.position.x, _initialPosition.x, "Camera should move left when A is pressed.");
        TearDown();
    }
    
    [UnityTest]
    public IEnumerator TestCameraMovesRight_WhenPressingS()
    {
        Setup();
        PressKey(Key.S);
        yield return new WaitForSeconds(0.5f);
        //ReleaseKey(Key.S);

        Assert.Less(_mainCamera.transform.position.z, _initialPosition.z, "Camera should move Down when S is pressed.");
        TearDown();
    }

    [UnityTest]
    public IEnumerator TestCameraMovesRight_WhenPressingD()
    {
        Setup();
        PressKey(Key.D);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.D);

        Assert.Greater(_mainCamera.transform.position.x, _initialPosition.x, "Camera should move right when D is pressed.");
        TearDown();
    }
    
    [UnityTest]
    public IEnumerator TestCameraRotatesLeft_WhenPressingQ()
    {
        Setup();
        PressKey(Key.Q);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.Q);

        float newRotation = _mainCamera.transform.eulerAngles.y;
        Assert.Greater(newRotation, _yInitialRotation, "Camera should rotate left when Q is pressed.");
        TearDown();
    }

    [UnityTest]
    public IEnumerator TestCameraRotatesRight_WhenPressingE()
    {
        Setup();
        PressKey(Key.E);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.E);

        float newRotation = _mainCamera.transform.eulerAngles.y;
        Assert.Greater(newRotation, _yInitialRotation, "Camera should rotate right when E is pressed.");
        TearDown();
    }
    
    [UnityTest]
    public IEnumerator TestCameraPitchUp_WhenPressingArrowUp()
    {
        Setup();
        PressKey(Key.UpArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.UpArrow);

        float newPitch = _mainCamera.transform.eulerAngles.x;
        Assert.Greater(newPitch, _xInitialRotation, "Camera should pitch up when UpArrow is pressed.");
        TearDown();
    }
    
    [UnityTest]
    public IEnumerator TestCameraPitchDown_WhenPressingArrowDown()
    {
        Setup();
        _xInitialRotation = _mainCamera.transform.eulerAngles.x;        //i Know this si technically called 2 times in a row but just leave it this was, so it works
        PressKey(Key.DownArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.DownArrow);

        float newPitch = _mainCamera.transform.eulerAngles.x;
        Assert.Less(newPitch, _xInitialRotation, "Camera should pitch Down when DownArrow is pressed.");
        TearDown();
    }
    
    [UnityTest]
    public IEnumerator TestCameraZoomOut_WhenPressingArrowLeft()
    {
        Setup();
        PressKey(Key.LeftArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.LeftArrow);
    
        Vector3 newPosition = RoundVector(_mainCamera.transform.position);
        Vector3 newCalculatedPosition = new Vector3(0, 5.66f, -5.66f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should zoom Out when LeftArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should zoom Out when LeftArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should zoom Out when LeftArrow is pressed.");
        TearDown();
    }

    [UnityTest]
    public IEnumerator TestCameraZoomsIn_WhenPressingArrowRight()
    {
        Setup();
        PressKey(Key.RightArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.RightArrow);
    
        Vector3 newPosition =  RoundVector(_mainCamera.transform.position);
        Vector3 newCalculatedPosition = new Vector3(0, 2.83f, -2.83f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should zoom In when RightArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should zoom In when RightArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should zoom In when RightArrow is pressed.");
        TearDown();
    }
    
    [UnityTest]
    public IEnumerator TestCameraZoomMaxOutToBorder_WhenPressingArrowLeft()
    {
        Setup();
        PressKey(Key.LeftArrow);
        yield return new WaitForSeconds(5f);
        ReleaseKey(Key.LeftArrow);
    
        Vector3 newPosition =  RoundVector(_mainCamera.transform.position);
        Vector3 newCalculatedPosition = new Vector3(0, 14.14f, -14.14f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should zoom Out till it reaches the boarder when LeftArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should zoom Out till it reaches the boarder when LeftArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should zoom Out till it reaches the boarder when LeftArrow is pressed.");
        TearDown();
    }
    
    [UnityTest]
    public IEnumerator TestCameraZoomsMaxInToBorder_WhenPressingArrowRight()
    {
        Setup();
        PressKey(Key.RightArrow);
        yield return new WaitForSeconds(3f);
        ReleaseKey(Key.RightArrow);
    
        Vector3 newPosition =  RoundVector(_mainCamera.transform.position);
        Vector3 newCalculatedPosition = new Vector3(0, 1.41f, -1.41f);
        Debug.Log(newPosition);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should zoom In till it reaches the boarder when RightArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should zoom In till it reaches the boarder when RightArrow is pressed.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should zoom In till it reaches the boarder when RightArrow is pressed.");
        TearDown();
    }
    
    [UnityTest]
    public IEnumerator TestCameraCombinedMovement()
    {
        Setup();
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

        Vector3 newPosition =  RoundVector(_mainCamera.transform.position);
        Vector3 newCalculatedPosition = new Vector3(-2.74f, 16.0f, 9.88f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should move to new x position.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should move to new y position.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should move to new z position.");
        TearDown();
    }
    [UnityTest]
    public IEnumerator TestCameraCombinedInversedMovement()
    {
        Setup();
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

        Vector3 newPosition =  RoundVector(_mainCamera.transform.position);
        Vector3 newCalculatedPosition = new Vector3(-11.36f, 0f, 4.12f);
        Assert.AreEqual(newCalculatedPosition.x, newPosition.x, "Camera should move to new x position.");
        Assert.AreEqual(newCalculatedPosition.y, newPosition.y, "Camera should move to new y position.");
        Assert.AreEqual(newCalculatedPosition.z, newPosition.z, "Camera should move to new z position.");
        TearDown();
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy the test camera and reset all vars after each test
        Object.Destroy(_mainCamera);
        _mainCamera = null;
        _initialPosition = Vector3.zero;
        _yInitialRotation = 0f;
        _xInitialRotation = 0f;
        _zInitialRotation = 0f;
        _initialZoom = 0f;
        _activeKeys.Clear();
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
    
    private Vector3 RoundVector(Vector3 vector)     // Helper function to round the vector values to 2 decimal places after point
    {
        return new Vector3(
            (float)Math.Round(vector.x, 2),
            (float)Math.Round(vector.y, 2),
            (float)Math.Round(vector.z, 2)
        );
    }
}

