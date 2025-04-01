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
            PlayerClient player = new PlayerClient();
            Assert.AreEqual("DefaultPlayer", player.GetPlayerName());
            Assert.GreaterOrEqual(player.GetPlayerId(), 0);
            Assert.LessOrEqual(player.GetPlayerId(), 100000);
            Assert.AreEqual(false, player.GetIsHost());
        }
        
        [Test]
        public void TestPlayerClientFull()
        {
            PlayerClient player = new PlayerClient("TestPlayer", 1, true);
            Assert.AreEqual("TestPlayer", player.GetPlayerName());
            Assert.AreEqual(1, player.GetPlayerId());
            Assert.AreEqual(true, player.GetIsHost());
        }
        
        [Test]
        public void TestPlayerClientWithoutHost()
        {
            PlayerClient player = new PlayerClient("TestPlayer", 1);
            Assert.AreEqual("TestPlayer", player.GetPlayerName());
            Assert.AreEqual(1, player.GetPlayerId());
            Assert.AreEqual(false, player.GetIsHost());
        }
        
        [Test]
        public void TestPlayerClientWithoutId()
        {
            PlayerClient player = new PlayerClient("TestPlayer");
            Assert.AreEqual("TestPlayer", player.GetPlayerName());
            Assert.GreaterOrEqual(player.GetPlayerId(), 0);
            Assert.LessOrEqual(player.GetPlayerId(), 100000);
            Assert.AreEqual(false, player.GetIsHost());
        }
        
        [Test]
        public void TestPlayerClientOnlyName()
        {
            PlayerClient player = new PlayerClient("TestPlayer");
            Assert.AreEqual("TestPlayer", player.GetPlayerName());
            Assert.GreaterOrEqual(player.GetPlayerId(), 0);
            Assert.LessOrEqual(player.GetPlayerId(), 100000);
        }
    }
}