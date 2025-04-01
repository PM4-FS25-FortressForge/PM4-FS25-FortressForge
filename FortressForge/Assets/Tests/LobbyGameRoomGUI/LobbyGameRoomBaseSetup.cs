using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Tests.LobbyGameRoomGUI
{
    public class LobbyGameRoomBaseSetup
    {
        protected VisualElement LobbyMenuRoot;
        private const string TEST_STRING = "TestString";

        [UnitySetUp]
        public virtual IEnumerator SetUp()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
            yield return new WaitForSeconds(1f);


            GameObject lobbyMenuGameObject = GameObject.Find("LobbyMenu");
            Assert.NotNull(lobbyMenuGameObject, "❌ LobbyMenu not found!");

            LobbyMenuRoot = lobbyMenuGameObject.GetComponent<UIDocument>().rootVisualElement;
            Assert.NotNull(LobbyMenuRoot, "❌ LobbyMenuRoot not found!");
        }

        /// <summary>
        /// Gets the root of the GameRoomView
        /// </summary>
        /// <returns>The root of the GameRoomView</returns>
        protected VisualElement GetGameRoomRoot()
        {
            GameObject gameRoomGameObject = GameObject.Find("GameRoomView");
            Assert.NotNull(gameRoomGameObject, "❌ GameRoomView not found!");

            VisualElement gameRoomRoot = gameRoomGameObject.GetComponent<UIDocument>().rootVisualElement;
            Assert.NotNull(gameRoomRoot, "❌ GameRoomRoot not found!");

            return gameRoomRoot;
        }

        /// <summary>
        ///  Checks if a TextField is not null and has a value
        /// </summary>
        /// <param name="textField">The TextField to check</param>
        protected void CheckTextField(TextField textField)
        {
            Assert.NotNull(textField, "TextField not found!");
            textField.value = TEST_STRING;
            Assert.IsTrue(textField.value.Length > 0, "TextField is empty!");
            Assert.AreEqual(TEST_STRING, textField.value, "TextField value is empty!");
        }
    }
}