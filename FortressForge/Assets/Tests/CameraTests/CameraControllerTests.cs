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

public class CameraMovementTest
{
    private GameObject _mainCamera;
    private Vector3 initialPosition;
    private float yInitialRotation;
    private float xInitialRotation;
    private float zInitialRotation;
    private HashSet<Key> activeKeys = new HashSet<Key>();

    private PlayerInput playerInput;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Unity Cammera implementation Test");
        yield return null; 

        // Ensure the scene is actually loaded
        Scene loadedScene = SceneManager.GetActiveScene();
        Assert.AreEqual("Unity Cammera implementation Test", loadedScene.name, 
            $"Error: Scene did not load correctly. Loaded: {loadedScene.name}");

        // Find the main camera in the loaded scene
        _mainCamera = GameObject.Find("Main Camera");
        Assert.IsNotNull(_mainCamera, "Error: Main Camera was not found in the scene.");

        // Save initial position & rotation
        initialPosition = _mainCamera.transform.position;
        yInitialRotation = _mainCamera.transform.eulerAngles.y;
        xInitialRotation = _mainCamera.transform.eulerAngles.x;
        zInitialRotation = _mainCamera.transform.eulerAngles.z;
    }



    [UnityTest]
    public IEnumerator TestCameraMovesForward_WhenPressingW()
    {
        PressKey(Key.W);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.W);

        Assert.Greater(_mainCamera.transform.position.z, initialPosition.z, "Camera should move forward when W is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraMovesLeft_WhenPressingA()
    {
        PressKey(Key.A);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.A);

        Assert.Less(_mainCamera.transform.position.x, initialPosition.x, "Camera should move left when A is pressed.");
    }
    [UnityTest]
    public IEnumerator TestCameraMovesRight_WhenPressingS()
    {
        PressKey(Key.S);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.S);

        Assert.Less(_mainCamera.transform.position.z, initialPosition.z, "Camera should move Down when S is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraMovesRight_WhenPressingD()
    {
        PressKey(Key.D);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.D);

        Assert.Greater(_mainCamera.transform.position.x, initialPosition.x, "Camera should move right when D is pressed.");
    }
    

    [UnityTest]
    public IEnumerator TestCameraRotatesLeft_WhenPressingQ()
    {
        PressKey(Key.Q);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.Q);

        float newRotation = _mainCamera.transform.eulerAngles.y;
        Assert.Greater(newRotation, yInitialRotation, "Camera should rotate left when Q is pressed.");
    }

    [UnityTest]
    public IEnumerator TestCameraRotatesRight_WhenPressingE()
    {
        PressKey(Key.E);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.E);

        float newRotation = _mainCamera.transform.eulerAngles.y;
        Assert.Greater(newRotation, yInitialRotation, "Camera should rotate right when E is pressed.");
    }
    
    [UnityTest]
    public IEnumerator TestCameraPitchUp_WhenPressingArrowUp()
    {
        PressKey(Key.UpArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.UpArrow);

        float newPitch = _mainCamera.transform.eulerAngles.x;
        Assert.Greater(newPitch, xInitialRotation, "Camera should pitch up when UpArrow is pressed.");
    }
    [UnityTest]
    public IEnumerator TestCameraPitchDown_WhenPressingArrowDown()
    {
        PressKey(Key.DownArrow);
        yield return new WaitForSeconds(0.5f);
        ReleaseKey(Key.DownArrow);

        float newPitch = _mainCamera.transform.eulerAngles.x;
        Assert.Less(newPitch, xInitialRotation, "Camera should pitch Down when DownArrow is pressed.");
    }

    
    [UnityTest]
    public IEnumerator TestCameraCombinedMovement()
    {
        PressKey(Key.W);
        PressKey(Key.D);
        PressKey(Key.E);
        PressKey(Key.UpArrow);
        yield return new WaitForSeconds(2.5f);
        ReleaseKey(Key.W);
        ReleaseKey(Key.D);
        ReleaseKey(Key.E);
        ReleaseKey(Key.UpArrow);

        Vector3 newPosition = _mainCamera.transform.position;
        Vector3 newCalculatedPosition = new Vector3(-2.88604116f, 5.99908638f, 9.7804451f);
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

    private void PressKey(Key key)
    {
        activeKeys.Add(key); // Add key to active set

        KeyboardState state = new KeyboardState();
        foreach (Key k in activeKeys)
        {
            state.Set(k, true);
        }

        InputSystem.QueueStateEvent(Keyboard.current, state);
        InputSystem.Update();
    }

    private void ReleaseKey(Key key)
    {
        activeKeys.Remove(key); // Remove key from active set

        KeyboardState state = new KeyboardState();
        foreach (Key k in activeKeys)
        {
            state.Set(k, true); // Keep other keys pressed
        }

        InputSystem.QueueStateEvent(Keyboard.current, state);
        InputSystem.Update();
    }
}
