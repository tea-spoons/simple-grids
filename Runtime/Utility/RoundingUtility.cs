
using UnityEngine;

namespace TeaSpoons.SimpleGrids
{
    internal static class RoundingUtility
    {
        /// <summary>
        /// Rounds to the next lower whole number.
        /// For negative numbers, that's the number with the next higher absolute value.
        /// </summary>
        /// <example>
        /// Floor(4.5f) // 4
        /// Floor(-4.5f) // -5
        /// </example>
        public static int NextSmallerInt(float value)
        {
            var i = (int)value;
            if (value < 0)
            {
                return i - 1;
            }
            return i;
        }

        /// <summary>
        /// Rounds both x and y of <paramref name="v"/> to the next lower whole number.
        /// See also <seealso cref="NextSmallerInt(float)"/>.
        /// </summary>
        public static Vector2Int NextSmallerInts(Vector2 v)
        {
            return new Vector2Int(NextSmallerInt(v.x), NextSmallerInt(v.y));
        }
    }
}
