using NUnit.Framework;
using ProjectMeadow.Objectives;
using ProjectMeadow.Inventory;
using ProjectMeadow.Economy;
namespace ProjectMeadow.Tests
{
    public class ObjectiveTests
    {
        [Test] public void DeliveryCompletesObjectiveOnce()
        {
            var inventory = new InventoryService(10);
            var economy = new EconomyService(inventory);
            var goal = new SellObjective(10);
            inventory.TryAdd("egg", 10);
            Assert.IsFalse(goal.Observe(economy.EggsSold));
            economy.TryDispatch("egg", 10);
            Assert.IsFalse(goal.Observe(economy.EggsSold));
            economy.Tick(5);
            Assert.IsTrue(goal.Observe(economy.EggsSold));
            Assert.IsFalse(goal.Observe(economy.EggsSold));
            Assert.IsTrue(goal.Complete);
            Assert.IsFalse(new SellObjective(10).Complete);
        }
    }
}
