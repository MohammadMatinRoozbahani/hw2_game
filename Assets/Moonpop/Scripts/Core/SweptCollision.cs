using System;

namespace Moonpop.Core
{
    public static class SweptCollision
    {
                                                                                                    
        public static bool Intersects(float fromX, float fromZ, float toX, float toZ, float halfX, float halfZ)
        {
            float enter = 0f, exit = 1f;
            return Slab(fromX, toX - fromX, halfX, ref enter, ref exit) &&
                   Slab(fromZ, toZ - fromZ, halfZ, ref enter, ref exit);
        }

        private static bool Slab(float start, float direction, float extent, ref float enter, ref float exit)
        {
            if (Math.Abs(direction) < 0.000001f) return Math.Abs(start) <= extent;
            float a = (-extent - start) / direction;
            float b = (extent - start) / direction;
            if (a > b) { float temp = a; a = b; b = temp; }
            enter = Math.Max(enter, a);
            exit = Math.Min(exit, b);
            return enter <= exit;
        }
    }
}
