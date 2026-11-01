using System;
using ProjectMeadow.Animals;
class Program
{
    static void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("PASS: " + name); }
    static void Main()
    {
        var inventory = new ProjectMeadow.Inventory.InventoryService(2);
        int changes = 0;
        inventory.Changed += () => changes++;
        Check(!inventory.TryAdd(null) && !inventory.TryAdd("egg", 0), "Reject invalid inventory additions");
        Check(inventory.TryAdd("egg", 2) && !inventory.TryAdd("egg"), "Capacity enforced");
        Check(inventory.Used == 2 && inventory.Count("egg") == 2 && changes == 1, "Failed addition preserves inventory");
        Check(!inventory.TryRemove("egg", 3) && !inventory.TryRemove("egg", -1), "Invalid removal is atomic");
        Check(inventory.TryRemove("egg") && inventory.TryAdd("milk"), "Removal frees capacity");
        Check(inventory.Count("egg") == 1 && inventory.Count("milk") == 1 && inventory.Used == 2, "Mixed inventory counts");
        Check(!inventory.TryAdd("egg", int.MaxValue), "Large additions cannot overflow capacity");
        var stock = new ProjectMeadow.Inventory.InventoryService(10);
        var economy = new ProjectMeadow.Economy.EconomyService(stock);
        Check(!economy.TryDispatch("egg", 1) && !economy.TryBuyChicken(() => true), "Empty farm cannot sell or purchase");
        stock.TryAdd("egg", 5);
        Check(!economy.TryDispatch("milk", 1) && !economy.TryDispatch("egg", 0) && !economy.TryDispatch("egg", 6), "Invalid cargo rejected");
        Check(economy.TryDispatch("egg", 5) && stock.Used == 0 && economy.Currency.Balance == 0, "Dispatch removes cargo without early payment");
        Check(!economy.TryDispatch("egg", 1), "Truck cannot dispatch twice");
        economy.Tick(float.NaN); economy.Tick(-1); economy.Tick(4);
        Check(economy.Busy && economy.Currency.Balance == 0, "Invalid time ignored and trip takes full duration");
        economy.Tick(1);
        Check(!economy.Busy && economy.Currency.Balance == 25 && economy.EggsSold == 5, "Truck returns with exact payment");
        economy.Tick(100);
        Check(economy.Currency.Balance == 25, "No duplicate payment");
        Check(!economy.TryBuyChicken(() => false) && economy.Currency.Balance == 25, "Failed spawn refunds coins");
        try { economy.TryBuyChicken(() => throw new Exception("spawn failed")); } catch (Exception) {}
        Check(economy.Currency.Balance == 25, "Spawn exception refunds coins");
        int spawned = 0;
        Check(economy.TryBuyChicken(() => { spawned++; return true; }) && economy.Currency.Balance == 0, "Successful purchase deducts price");
        Check(!economy.TryBuyChicken(() => { spawned++; return true; }) && spawned == 1, "Insufficient funds never spawn chicken");
        var wallet = new ProjectMeadow.Economy.CurrencyService();
        Check(!wallet.TryCredit(-1) && wallet.TryCredit(int.MaxValue) && !wallet.TryCredit(1), "Currency rejects negatives and overflow");
        var levelStock = new ProjectMeadow.Inventory.InventoryService(10);
        var levelEconomy = new ProjectMeadow.Economy.EconomyService(levelStock);
        var objective = new ProjectMeadow.Objectives.SellObjective(10);
        Check(!objective.Observe(-5) && objective.Progress == 0, "Negative progress ignored");
        levelStock.TryAdd("egg", 10);
        Check(!objective.Observe(levelEconomy.EggsSold), "Collecting eggs does not complete objective");
        levelEconomy.TryDispatch("egg", 5);
        Check(!objective.Observe(levelEconomy.EggsSold), "Cargo in transit does not count");
        levelEconomy.Tick(5);
        Check(!objective.Observe(levelEconomy.EggsSold) && objective.Progress == 5, "Partial delivered sale updates objective");
        objective.Observe(0);
        Check(objective.Progress == 5, "Objective progress cannot regress");
        levelEconomy.TryDispatch("egg", 5); levelEconomy.Tick(5);
        Check(objective.Observe(levelEconomy.EggsSold) && objective.Complete, "Ten delivered eggs complete level");
        Check(!objective.Observe(10) && !objective.Observe(100) && objective.Progress == 10, "Completion fires once and clamps progress");
        Check(!new ProjectMeadow.Objectives.SellObjective(10).Complete, "New level starts incomplete");
        bool rejectedTarget = false;
        try { new ProjectMeadow.Objectives.SellObjective(0); } catch (ArgumentOutOfRangeException) { rejectedTarget = true; }
        Check(rejectedTarget, "Invalid level target rejected");
        var c = new ChickenCycle(2, 5);
        Check(!c.Tick(100), "No production without food");
        Check(c.BeginMeal() && !c.BeginMeal(), "No overlapping meals");
        Check(!c.Tick(2) && c.State == ChickenCycle.Phase.ProductionCooldown, "Eating transitions to cooldown");
        Check(!c.Tick(4) && c.Tick(1), "Egg only after complete cooldown");
        Check(!c.Tick(100), "Exactly one egg per meal");
        Check(c.BeginMeal() && c.Tick(7), "Next meal produces another egg");
        c.BeginMeal();
        Check(!c.Tick(-10) && c.Tick(20) && !c.Tick(20), "Negative time ignored; large frame produces once");
    }
}
