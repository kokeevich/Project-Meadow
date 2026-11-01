using NUnit.Framework;
using ProjectMeadow.Inventory;
using ProjectMeadow.Economy;
using ProjectMeadow.Production;
namespace ProjectMeadow.Tests
{
    public class ProductionTests
    {
        [Test] public void FullStorageRetainsOutputUntilRoomIsAvailable()
        {
            var stock = new InventoryService(2);
            var mill = new ProductionService(stock, new CurrencyService(), "grain", 2, "flour", 1, 6);
            stock.TryAdd("grain", 2);
            Assert.IsTrue(mill.TryQueue());
            stock.TryAdd("egg", 2);
            mill.Tick(6);
            Assert.IsTrue(mill.OutputBlocked);
            Assert.AreEqual(1, mill.Queued);
            stock.TryRemove("egg");
            mill.Tick(0);
            Assert.AreEqual(1, stock.Count("flour"));
            Assert.AreEqual(0, mill.Queued);
            mill.Tick(100);
            Assert.AreEqual(1, stock.Count("flour"));
        }
        [Test] public void UpgradeDoesNotResetOrAccelerateCurrentJob()
        {
            var stock = new InventoryService(4);
            var wallet = new CurrencyService(); wallet.TryCredit(20);
            var mill = new ProductionService(stock, wallet, "grain", 2, "flour", 1, 6);
            stock.TryAdd("grain", 4); mill.TryQueue(); mill.TryQueue();
            mill.Tick(3); Assert.IsTrue(mill.TryUpgrade()); mill.Tick(2);
            Assert.AreEqual(0, stock.Count("flour"));
            mill.Tick(1); Assert.AreEqual(1, stock.Count("flour"));
            mill.Tick(4.8f); Assert.AreEqual(2, stock.Count("flour"));
        }
    }
}
