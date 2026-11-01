using System;
using System.Collections.Generic;
namespace ProjectMeadow.Inventory
{
    public sealed class InventoryService
    {
        private readonly Dictionary<string, int> items = new Dictionary<string, int>();
        public int Capacity { get; }
        public int Used { get; private set; }
        public event Action Changed;
        public InventoryService(int capacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity;
        }
        public int Count(string id) => id != null && items.TryGetValue(id, out var count) ? count : 0;
        public bool TryAdd(string id, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(id) || amount < 1 || amount > Capacity - Used) return false;
            items[id] = Count(id) + amount;
            Used += amount;
            Changed?.Invoke();
            return true;
        }
        public bool TryRemove(string id, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(id) || amount < 1 || Count(id) < amount) return false;
            int remaining = Count(id) - amount;
            if (remaining == 0) items.Remove(id); else items[id] = remaining;
            Used -= amount;
            Changed?.Invoke();
            return true;
        }
    }
}
