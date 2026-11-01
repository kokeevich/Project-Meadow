using System;
using ProjectMeadow.Inventory;
namespace ProjectMeadow.Economy
{
    public sealed class EconomyService
    {
        public const int EggPrice = 5;
        public const int ChickenPrice = 25;
        public const float TripSeconds = 5f;
        private readonly InventoryService inventory;
        public CurrencyService Currency { get; } = new CurrencyService();
        public int Cargo { get; private set; }
        public string CargoId { get; private set; }
        private int cargoPrice;
        public static int Price(string id) => id == "egg" ? EggPrice : id == "grain" ? 2 : id == "flour" ? 14 : 0;
        public int EggsSold { get; private set; }
        public float Remaining { get; private set; }
        public bool Busy => Cargo > 0;
        public EconomyService(InventoryService inventory)
        { this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory)); }
        public bool TryDispatch(string itemId, int quantity)
        {
            int price = Price(itemId);
            if (Busy || price == 0 || quantity <= 0 || quantity > int.MaxValue / price) return false;
            if (quantity * price > int.MaxValue - Currency.Balance || quantity > int.MaxValue - EggsSold) return false;
            if (!inventory.TryRemove(itemId, quantity)) return false;
            CargoId = itemId; cargoPrice = price; Cargo = quantity; Remaining = TripSeconds; return true;
        }
        public void Tick(float seconds)
        {
            if (!Busy || seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return;
            Remaining = Math.Max(0, Remaining - seconds);
            if (Remaining > 0) return;
            if (!Currency.TryCredit(Cargo * cargoPrice)) return;
            if (CargoId == "egg") EggsSold += Cargo;
            Cargo = 0; CargoId = null;
        }
        public bool TryBuyChicken(Func<bool> spawn)
        {
            if (spawn == null || !Currency.TrySpend(ChickenPrice)) return false;
            try
            {
                if (spawn()) return true;
            }
            catch { Currency.TryCredit(ChickenPrice); throw; }
            Currency.TryCredit(ChickenPrice); return false;
        }
    }
}
