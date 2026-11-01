using System;
namespace ProjectMeadow.Objectives
{
    public sealed class SellObjective
    {
        public int Target { get; }
        public int Progress { get; private set; }
        public bool Complete => Progress >= Target;
        public SellObjective(int target)
        {
            if (target <= 0) throw new ArgumentOutOfRangeException(nameof(target));
            Target = target;
        }
        // Accept cumulative delivered sales, never collected items or cargo in transit.
        public bool Observe(int delivered)
        {
            if (Complete) return false;
            Progress = Math.Min(Target, Math.Max(Progress, delivered));
            return Complete;
        }
    }
}
