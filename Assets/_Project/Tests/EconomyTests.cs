using NUnit.Framework;
using ProjectMeadow.Inventory;
using ProjectMeadow.Economy;
namespace ProjectMeadow.Tests
{
    public class EconomyTests
    {
        [Test] public void DeliveryPaysOnceAndPurchaseRefundsOnFailedSpawn()
        {
            var inventory = new InventoryService(10);
            var economy = new EconomyService(inventory);
            inventory.TryAdd("egg", 5);
            Assert.IsTrue(economy.TryDispatch("egg", 5));
            Assert.AreEqual(0, inventory.Used);
            Assert.AreEqual(0, economy.Currency.Balance);
            Assert.IsFalse(economy.TryDispatch("egg", 1));
            economy.Tick(5);
            economy.Tick(5);
            Assert.AreEqual(25, economy.Currency.Balance);
            Assert.AreEqual(5, economy.EggsSold);
            Assert.IsFalse(economy.TryBuyChicken(() => false));
            Assert.AreEqual(25, economy.Currency.Balance);
            Assert.IsTrue(economy.TryBuyChicken(() => true));
            Assert.AreEqual(0, economy.Currency.Balance);
        }
    }
}
