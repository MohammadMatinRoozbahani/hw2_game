using System;

namespace Moonpop.Core
{
    public struct TrafficRow
    {
        public int SafeLane;
        public int BlockedMask;
        public int MovingMask;
        public int CoinCount;
        public bool HasFuel;
        public bool IsBlocked(int lane) => (BlockedMask & (1 << lane)) != 0;
        public bool IsMoving(int lane) => (MovingMask & (1 << lane)) != 0;
    }

                                                                                        
    public sealed class TrafficPlanner
    {
        private uint random;
        private int lastSafe = 1;
        private int rowIndex;
        public TrafficPlanner(uint seed) { random = seed == 0 ? 0x91E10DA5u : seed; }

        public TrafficRow Next(float difficulty)
        {
            difficulty = Math.Max(0, Math.Min(1, difficulty));
            int safe = lastSafe;
            if (rowIndex > 1 && Sample() < 0.48f)
                safe = Math.Max(0, Math.Min(2, safe + (Sample() < 0.5f ? -1 : 1)));
            int blocked = 0;
            int moving = 0;
            if (rowIndex >= 2)
            {
                int first = (safe + (Sample() < 0.5f ? 1 : 2)) % 3;
                blocked |= 1 << first;
                if (Sample() < 0.10f + 0.78f * difficulty)
                    blocked |= 1 << (3 - safe - first);
                for (int lane = 0; lane < 3; lane++)
                    if ((blocked & (1 << lane)) != 0 && rowIndex >= 4 && Sample() < 0.42f)
                        moving |= 1 << lane;
            }
            var row = new TrafficRow
            {
                SafeLane = safe, BlockedMask = blocked, MovingMask = moving,
                CoinCount = 3 + (int)(Sample() * 3), HasFuel = rowIndex % 7 == 5
            };
            rowIndex++;
            lastSafe = safe;
            return row;
        }

                                                                              
        public static float RowSpacing(float speed, float difficulty) => Math.Max(46f, speed * 0.92f) + Math.Max(0, difficulty) * 2f;

        private float Sample()
        {
            random ^= random << 13;
            random ^= random >> 17;
            random ^= random << 5;
            return (random >> 8) * (1f / 16777216f);
        }
    }
}
