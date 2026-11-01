using System;
namespace ProjectMeadow.Animals
{
    // Feeding succeeds once; only a completed meal can produce an egg.
    public sealed class ChickenCycle
    {
        public enum Phase { Idle, Eat, ProductionCooldown }
        public Phase State { get; private set; }
        private float remaining;
        private readonly float eating;
        private readonly float production;
        public ChickenCycle(float eatSeconds, float productionSeconds)
        {
            eating = Math.Max(0.1f, eatSeconds);
            production = Math.Max(0.1f, productionSeconds);
        }
        public bool BeginMeal()
        {
            if (State != Phase.Idle) return false;
            State = Phase.Eat;
            remaining = eating;
            return true;
        }
        public bool Tick(float seconds)
        {
            if (seconds <= 0 || State == Phase.Idle) return false;
            remaining -= seconds;
            if (remaining > 0) return false;
            if (State == Phase.Eat)
            {
                State = Phase.ProductionCooldown;
                remaining += production;
                if (remaining > 0) return false;
            }
            State = Phase.Idle;
            return true;
        }
    }
}
