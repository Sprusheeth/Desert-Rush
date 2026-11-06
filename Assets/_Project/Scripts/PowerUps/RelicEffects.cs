using UnityEngine;

namespace DesertRush
{
    public enum PickupKind { Coin, Idol, Gem, Magnet, Boost, Shield, Double, Glider }

    public sealed class RelicEffects
    {
        private readonly float[] timers = new float[8];

        public float Remaining(PickupKind kind) => timers[(int)kind];
        public bool Active(PickupKind kind) => Remaining(kind) > 0f;
        public void Clear() => System.Array.Clear(timers, 0, timers.Length);

        public void Tick(float dt)
        {
            for (int i = 3; i < timers.Length; i++)
                timers[i] = Mathf.Max(0f, timers[i] - dt);
        }

        public int Collect(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Coin: return Active(PickupKind.Double) ? 2 : 1;
                case PickupKind.Idol: return 15;
                case PickupKind.Gem: return 5;
                case PickupKind.Magnet: timers[(int)kind] = 11f; break;
                case PickupKind.Boost: timers[(int)kind] = 6f; break;
                case PickupKind.Shield: timers[(int)kind] = 20f; break;
                case PickupKind.Double: timers[(int)kind] = 12f; break;
                case PickupKind.Glider: timers[(int)kind] = 9f; break;
            }
            return 0;
        }

        public bool AbsorbImpact()
        {
            if (Active(PickupKind.Boost) || Active(PickupKind.Glider)) return true;
            if (!Active(PickupKind.Shield)) return false;
            timers[(int)PickupKind.Shield] = 0f;
            return true;
        }
    }
}
