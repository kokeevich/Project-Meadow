using System;
namespace ProjectMeadow.Economy
{
    public sealed class CurrencyService
    {
        public int Balance { get; private set; }
        public bool TryCredit(int amount)
        {
            if (amount <= 0 || amount > int.MaxValue - Balance) return false;
            Balance += amount; return true;
        }
        public bool TrySpend(int amount)
        {
            if (amount <= 0 || amount > Balance) return false;
            Balance -= amount; return true;
        }
    }
}
