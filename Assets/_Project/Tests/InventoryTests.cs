using NUnit.Framework;
using ProjectMeadow.Inventory;
namespace ProjectMeadow.Tests
{
    public class InventoryTests
    {
        [Test] public void FullStorageRejectsItemsWithoutChangingCounts()
        {
            var inventory = new InventoryService(2);
            Assert.IsTrue(inventory.TryAdd("egg", 2));
            Assert.IsFalse(inventory.TryAdd("egg"));
            Assert.AreEqual(2, inventory.Count("egg"));
            Assert.AreEqual(2, inventory.Used);
        }
        [Test] public void RemovalFreesSpaceAndInvalidOperationsAreAtomic()
        {
            var inventory = new InventoryService(2);
            Assert.IsFalse(inventory.TryAdd("egg", 0));
            Assert.IsFalse(inventory.TryAdd(null));
            Assert.IsTrue(inventory.TryAdd("egg", 2));
            Assert.IsFalse(inventory.TryRemove("egg", 3));
            Assert.IsTrue(inventory.TryRemove("egg"));
            Assert.IsTrue(inventory.TryAdd("milk"));
            Assert.AreEqual(1, inventory.Count("egg"));
            Assert.AreEqual(1, inventory.Count("milk"));
            Assert.AreEqual(2, inventory.Used);
        }
    }
}
