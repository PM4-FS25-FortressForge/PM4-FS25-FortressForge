using System.Collections;
using FortressForge.UI.Manager;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Tests.LobbyGameRoomGUI
{
    public class LobbyGameRoomTestBaseSetup
    {
        protected VisualElement LobbyMenuRoot;
        private const string TEST_STRING = "TestString";
        private const string EMPTY_SCENE = "EmptyScene";

        [UnitySetUp]
        public virtual IEnumerator SetUp()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
            yield return new WaitForSeconds(1f);


            GameObject lobbyMenuGameObject = GameObject.Find("LobbyMenu");
            Assert.NotNull(lobbyMenuGameObject, "❌ LobbyMenu not found!");

            LobbyMenuRoot = lobbyMenuGameObject.GetComponent<UIDocument>().rootVisualElement;
            Assert.NotNull(LobbyMenuRoot, "❌ LobbyMenuRoot not found!");
            
            GameObject viewManager = GameObject.Find("ViewManager");
            Assert.NotNull(viewManager, "❌ ViewManager not found!");
            ViewManager viewManagerComponent = viewManager.GetComponent<ViewManager>();
            Assert.NotNull(viewManager, "❌ ViewManagerComponent not found!");
            viewManagerComponent.NextScene = EMPTY_SCENE;
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
            
            CheckIfVisualElementExistsDisplayed(textField);
        }

        /// <summary>
        /// Checks if a Visual Element exists and is visible
        /// </summary>
        /// <param name="element">The Visual Element to check</param>
        protected void CheckIfVisualElementExistsDisplayed(VisualElement element)
        {
            Assert.NotNull(element, "❌ VisualElement not found!");
            
            Assert.IsTrue(element.resolvedStyle.display != DisplayStyle.None, "❌ VisualElement is not visible!");
            Assert.IsTrue(element.enabledInHierarchy, "❌ VisualElement is not enabled!");
            Assert.NotZero((int) element.worldBound.height, "❌ VisualElement has no world bound height!");
            Assert.NotZero((int) element.worldBound.width, "❌ VisualElement has no world bound width!");
        }
    }
}