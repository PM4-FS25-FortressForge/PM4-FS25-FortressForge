using System;
using System.Collections;
using FishNet;
using FishNet.Managing;
using FortressForge.Network;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Tests.Network
{
    public class RemoteConnectionHandlerTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("EmptyScene", LoadSceneMode.Single);
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "EmptyScene",
                new TimeSpan(0, 0, 10),
                () => Debug.Log("EmptyScene is not loaded after 10 seconds."));

            GameObject networkManagerPrefab = Resources.Load<GameObject>("Prefabs/NetworkManager");
            Assert.IsNotNull(networkManagerPrefab, "NetworkManager prefab not found!");

            networkManagerPrefab.name = "TestNetworkManager";

            GameObject networkManagerObject = Object.Instantiate(networkManagerPrefab);
            Assert.IsNotNull(networkManagerObject, "NetworkManager object not found!");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.Destroy(obj);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator LeaveServerTest()
        {
            // Start the server
            InstanceFinder.ServerManager.StartConnection();
            yield return new WaitUntil(() => InstanceFinder.ServerManager.Started,
                new TimeSpan(0, 0, 10),
                () => Debug.Log("Server is not started after 10 seconds."));
            Assert.IsTrue(InstanceFinder.ServerManager.Started, "Server did not start!");

            // Start the client
            InstanceFinder.ClientManager.StartConnection();
            yield return new WaitUntil(
                () => InstanceFinder.ClientManager.Started,
                new TimeSpan(0, 0, 10),
                () => Debug.Log("Client is not started after 10 seconds."));
            Assert.IsTrue(InstanceFinder.ClientManager.Started, "Client did not start!");

            RemoteConnectionHandler remoteConnectionHandler = new GameObject().AddComponent<RemoteConnectionHandler>();
            Assert.IsNotNull(remoteConnectionHandler, "RemoteConnectionHandler not created!");

            remoteConnectionHandler.LeaveServer();

            yield return new WaitUntil(
                () => SceneManager.GetActiveScene().name != "EmptyScene",
                new TimeSpan(0, 0, 10),
                () => Debug.Log("New Scene is not loaded after 10 seconds.")
            );

            Assert.AreNotEqual("EmptyScene", SceneManager.GetActiveScene().name, "EmptyScene is still loaded!");

            Assert.IsFalse(InstanceFinder.ServerManager.Started, "Server did not stop!");
            Assert.IsFalse(InstanceFinder.ClientManager.Started, "Client did not stop!");

            NetworkManager[] networkManagerObjects = Object.FindObjectsByType<NetworkManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.IsNotNull(networkManagerObjects, "NetworkManager clone not found!");
            foreach (var obj in networkManagerObjects)
            {
                if (obj.name == "TestNetworkManager")
                {
                    Assert.Fail("NetworkManager clone was not destroyed!");
                }
            }
        }
    }
}