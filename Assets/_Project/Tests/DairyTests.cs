using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectMeadow.Animals;
using ProjectMeadow.Production;
using ProjectMeadow.Inventory;
using ProjectMeadow.Economy;
namespace ProjectMeadow.Tests
{
    public class DairyTests
    {
        [Test] public void CowPrefabUsesSharedControllerAndMilkData()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Animals/Cow.prefab");
            Assert.IsNotNull(prefab);
            var controller = prefab.GetComponent<FarmAnimalController>();
            Assert.IsNotNull(controller);
            var serialized = new SerializedObject(controller);
            Assert.IsTrue(serialized.FindProperty("isCow").boolValue);
            var data = (ProjectMeadow.Items.ItemData)serialized.FindProperty("productData").objectReferenceValue;
            Assert.AreEqual("milk", data.id);
        }
        [Test] public void DairyProducesAndSellsCheeseWithoutEggProgress()
        {
            var stock = new InventoryService(10);
            var economy = new EconomyService(stock);
            var dairy = new ProductionService(stock, economy.Currency, "milk", 2, "cheese", 1, 8);
            stock.TryAdd("milk", 2);
            Assert.IsTrue(dairy.TryQueue());
            dairy.Tick(7); Assert.AreEqual(0, stock.Count("cheese"));
            dairy.Tick(1); Assert.AreEqual(1, stock.Count("cheese"));
            Assert.IsTrue(economy.TryDispatch("cheese", 1)); economy.Tick(5);
            Assert.AreEqual(24, economy.Currency.Balance);
            Assert.AreEqual(0, economy.EggsSold);
        }
    }
}
