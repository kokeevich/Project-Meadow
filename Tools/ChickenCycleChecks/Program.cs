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
