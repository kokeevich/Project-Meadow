using System;
using ProjectMeadow.Inventory;
using ProjectMeadow.Economy;
namespace ProjectMeadow.Production
{
    public sealed class ProductionService
    {
        private readonly InventoryService inventory;
        private readonly CurrencyService currency;
        private readonly string input, output;
        private readonly int inputCount, outputCount;
        private readonly float baseSeconds;
        private float duration, remaining;
        public const int QueueCapacity = 3;
        public int Queued { get; private set; }
        public int UpgradeLevel { get; private set; }
        public int UpgradeCost => 20 * (UpgradeLevel + 1);
        public float NextDuration => baseSeconds / (1 + .25f * UpgradeLevel);
        public float Progress => Queued == 0 ? 0 : 1 - remaining / duration;
        public bool OutputBlocked => Queued > 0 && remaining <= 0;
        public ProductionService(InventoryService inventory, CurrencyService currency, string input, int inputCount, string output, int outputCount, float seconds)
        {
            if (inventory == null || currency == null || string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(output) || inputCount <= 0 || outputCount <= 0 || seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) throw new ArgumentException("Invalid production recipe");
            this.inventory = inventory; this.currency = currency; this.input = input; this.output = output;
            this.inputCount = inputCount; this.outputCount = outputCount; baseSeconds = seconds;
        }
        public bool TryQueue()
        {
            if (Queued >= QueueCapacity || !inventory.TryRemove(input, inputCount)) return false;
            if (Queued == 0) { duration = NextDuration; remaining = duration; }
            Queued++; return true;
        }
        public bool TryUpgrade()
        {
            if (UpgradeLevel >= 2 || !currency.TrySpend(UpgradeCost)) return false;
            UpgradeLevel++; return true;
        }
        public void Tick(float seconds)
        {
            if (Queued == 0 || seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return;
            while (Queued > 0)
            {
                float used = Math.Min(seconds, remaining);
                remaining -= used; seconds -= used;
                if (remaining > 0 || !inventory.TryAdd(output, outputCount)) return;
                Queued--;
                if (Queued == 0) { remaining = 0; return; }
                duration = NextDuration; remaining = duration;
                if (seconds <= 0) return;
            }
        }
    }
}
