using System;
namespace ProjectMeadow.Economy
{
    public static class TruckRoute
    {
        public static float Offset(float progress)
        {
            progress = Math.Max(0, Math.Min(1, progress));
            if (progress < .3f) return 22 * Smooth(progress / .3f);
            if (progress > .7f) return 22 * (1 - Smooth((progress - .7f) / .3f));
            return 22;
        }
        public static bool Visible(float progress) => progress < .3f || progress > .7f;
        private static float Smooth(float t) => t * t * (3 - 2 * t);
    }
}
