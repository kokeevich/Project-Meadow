using NUnit.Framework;
using ProjectMeadow.Animals;
namespace ProjectMeadow.Tests
{
    public class ChickenCycleTests
    {
        [Test] public void IdleNeverProducesEggs()
        {
            var cycle = new ChickenCycle(2, 5);
            Assert.IsFalse(cycle.Tick(100));
            Assert.AreEqual(ChickenCycle.Phase.Idle, cycle.State);
        }
        [Test] public void MealProducesExactlyOneEggAfterEatingAndCooldown()
        {
            var cycle = new ChickenCycle(2, 5);
            Assert.IsTrue(cycle.BeginMeal());
            Assert.IsFalse(cycle.BeginMeal());
            Assert.IsFalse(cycle.Tick(2));
            Assert.AreEqual(ChickenCycle.Phase.ProductionCooldown, cycle.State);
            Assert.IsFalse(cycle.Tick(4));
            Assert.IsTrue(cycle.Tick(1));
            Assert.IsFalse(cycle.Tick(100));
            Assert.IsTrue(cycle.BeginMeal());
            Assert.IsTrue(cycle.Tick(7));
        }
        [Test] public void LargeFramePreservesElapsedTime()
        {
            var cycle = new ChickenCycle(2, 5);
            cycle.BeginMeal();
            Assert.IsFalse(cycle.Tick(-1));
            Assert.IsTrue(cycle.Tick(20));
            Assert.IsFalse(cycle.Tick(20));
        }
    }
}
