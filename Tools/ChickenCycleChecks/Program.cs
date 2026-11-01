using System;
using ProjectMeadow.Animals;
class Program
{
    static void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("PASS: " + name); }
    static void Main()
    {
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
