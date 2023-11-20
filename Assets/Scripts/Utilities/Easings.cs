using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BucketBrawl
{
    public static class Easings
    {
        // Sine
        public static float EaseInSine(float x) { return 1 - Mathf.Cos(x * Mathf.PI / 2); }

        public static float EaseOutSine(float x) { return Mathf.Sin(x * Mathf.PI / 2); }

        public static float EaseInOutSine(float x) { return -.5f * (Mathf.Cos(Mathf.PI * x) - 1); }


        // Quad
        public static float EaseInQuad(float x) { return x * x; }

        public static float EaseOutQuad(float x) { return 1 - (1 - x) * (1 - x); }

        public static float EaseInOutQuad(float x) { return x < 0.5f ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2; }


        // Cubic
        public static float EaseInCubic(float x) { return Mathf.Pow(x, 3); }

        public static float EaseOutCubic(float x) { return 1 - Mathf.Pow(1 - x, 3); }

        public static float EaseInOutCubic(float x) { return x < 0.5f ? 4 * Mathf.Pow(x, 3) : 1 - Mathf.Pow(-2 * x + 2, 3) / 2; }


        // Back
        public static float EaseInBack(float x) { return x * x * ((1.70158f + 1) * x - 1.70158f); }

        public static float EaseOutBack(float x) { return 1 + ((1.70158f + 1) * Mathf.Pow(x - 1, 3) + 1.70158f); }

        public static float EaseInOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            return x < 0.5f ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2 : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        }


        // Bounce
        public static float EaseInBounce(float x) { return 1 - EaseOutBounce(1 - x); }

        public static float EaseOutBounce(float x)
        {
            if (x < 1 / 2.75f)
                return 7.5625f * x * x;
            else if (x < 2 / 2.75f)
                return 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f;
            else if (x < 2.5 / 2.75f)
                return 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f;
            else
                return 7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f;
        }

        public static float EaseInOutBounce(float x) { return x < 0.5f ? (1 - EaseOutBounce(1 - 2 * x)) / 2 : (1 + EaseOutBounce(2 * x - 1)) / 2; }
    }
}
