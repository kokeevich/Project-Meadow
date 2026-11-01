using NUnit.Framework;
using UnityEngine;
using ProjectMeadow.Farming;

namespace ProjectMeadow.Tests
{
    public class FoodPlotTests
    {
        private GameObject owner;
        private FoodPlot plot;
        [SetUp] public void SetUp()
        {
            owner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plot = owner.AddComponent<FoodPlot>();
        }
        [TearDown] public void TearDown() => Object.DestroyImmediate(owner);
        [Test] public void EmptyPlotCannotBeConsumedOrGrowWithoutPlanting()
        {
            Assert.IsFalse(plot.TryConsume());
            plot.AdvanceGrowth(10f);
            Assert.AreEqual(FoodPlot.GrowthState.Empty, plot.State);
            Assert.AreEqual(0f, plot.Progress);
        }
        [Test] public void OnlyOneAnimalCanReserveReadyFood()
        {
            Assert.IsFalse(plot.TryReserve(owner));
            plot.Interact(); plot.AdvanceGrowth(5);
            Assert.IsTrue(plot.TryReserve(owner));
            Assert.IsFalse(plot.TryReserve(owner.transform));
            plot.Release(owner);
            Assert.IsTrue(plot.TryReserve(owner.transform));
            Assert.IsTrue(plot.TryConsume());
            Assert.IsFalse(plot.IsAvailableFor(owner));
        }
        [Test] public void TwoChickensCannotConsumeTheSameServing()
        {
            plot.Interact();
            plot.AdvanceGrowth(5f);
            Assert.IsTrue(plot.TryConsume());
            Assert.IsFalse(plot.TryConsume());
        }
        [Test] public void PlantGrowConsumeAndReplant()
        {
            plot.Interact();
            plot.AdvanceGrowth(2.5f);
            Assert.AreEqual(FoodPlot.GrowthState.Growing, plot.State);
            Assert.AreEqual(0.5f, plot.Progress, 0.001f);
            Assert.IsFalse(plot.TryConsume());
            plot.Interact(); // A second click must not restart growth.
            plot.AdvanceGrowth(2.5f);
            Assert.AreEqual(FoodPlot.GrowthState.Ready, plot.State);
            plot.Interact(); // Ready food remains for animals to consume.
            Assert.AreEqual(FoodPlot.GrowthState.Ready, plot.State);
            Assert.IsTrue(plot.TryConsume());
            Assert.AreEqual(0f, plot.Progress);
            Assert.IsFalse(plot.TryConsume());
            plot.Interact();
            plot.AdvanceGrowth(100f);
            Assert.AreEqual(1f, plot.Progress);
            Assert.AreEqual(FoodPlot.GrowthState.Ready, plot.State);
        }
    }
}
