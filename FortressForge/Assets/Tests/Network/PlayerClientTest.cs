using FortressForge.Network;
using NUnit.Framework;

namespace Tests.Network
{
    [TestFixture]
    public class PlayerClientTest
    {
        [Test]
        public void TestEmptyPlayerClient()
        {
            PlayerClient player = new ();
            Assert.AreEqual("DefaultPlayer", player.PlayerName);
            Assert.GreaterOrEqual(player.PlayerID, 0);
            Assert.LessOrEqual(player.PlayerID, 100000);
            Assert.AreEqual(false, player.IsHost);
        }
        
        [Test]
        public void TestPlayerClientFull()
        {
            PlayerClient player = new ("TestPlayer", 1, true);
            Assert.AreEqual("TestPlayer", player.PlayerName);
            Assert.AreEqual(1, player.PlayerID);
            Assert.AreEqual(true, player.IsHost);
        }
        
        [Test]
        public void TestPlayerClientWithoutHost()
        {
            PlayerClient player = new ("TestPlayer", 1);
            Assert.AreEqual("TestPlayer", player.PlayerName);
            Assert.AreEqual(1, player.PlayerID);
            Assert.AreEqual(false, player.IsHost);
        }
        
        [Test]
        public void TestPlayerClientWithoutId()
        {
            PlayerClient player = new ("TestPlayer");
            Assert.AreEqual("TestPlayer", player.PlayerName);
            Assert.GreaterOrEqual(player.PlayerID, 0);
            Assert.LessOrEqual(player.PlayerID, 100000);
            Assert.AreEqual(false, player.IsHost);
        }
        
        [Test]
        public void TestPlayerClientOnlyName()
        {
            PlayerClient player = new ("TestPlayer");
            Assert.AreEqual("TestPlayer", player.PlayerName);
            Assert.GreaterOrEqual(player.PlayerID, 0);
            Assert.LessOrEqual(player.PlayerID, 100000);
        }
    }
}