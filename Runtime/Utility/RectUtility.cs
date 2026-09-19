
namespace TeaSpoons.SimpleGrids
{
    using UnityEngine;

    internal static class RectUtility
    {
        /// <summary>
        /// Updates <paramref name="rect"/> to contain <paramref name="v"/>.
        /// </summary>
        public static Rect ExtendToContain(Rect rect, Vector2 v)
        {
            if (rect.xMax < v.x) rect.xMax = v.x;
            if (rect.xMin > v.x) rect.xMin = v.x;
            if (rect.yMax < v.y) rect.yMax = v.y;
            if (rect.yMin > v.y) rect.yMin = v.y;

            return rect;
        }

        /// <summary>
        /// Updates <paramref name="rect"/> to contain <paramref name="v"/>.
        /// </summary>
        public static RectInt ExtendToContain(RectInt rect, Vector2Int v)
        {
            if (rect.xMax < v.x) rect.xMax = v.x;
            if (rect.xMin > v.x) rect.xMin = v.x;
            if (rect.yMax < v.y) rect.yMax = v.y;
            if (rect.yMin > v.y) rect.yMin = v.y;

            return rect;
        }

        /// <summary>
        /// Enlarges the given <paramref name="rect"/>'s xMax and yMax by 1 each.
        /// Used to update the bounds from containing the cell positions to containing the cells,
        /// which have a relative width of 1.
        /// </summary>
        public static Rect EnlargeByOne(Rect rect)
        {
            return new Rect(rect.position, rect.size + Vector2.one);
        }

        /// <summary>
        /// Enlarges the given <paramref name="rect"/>'s xMax and yMax by 1 each.
        /// Used to update the bounds from containing the cell positions to containing the cells,
        /// which have a relative width of 1.
        /// </summary>
        public static RectInt EnlargeByOne(RectInt rect)
        {
            return new RectInt(rect.position, rect.size + Vector2Int.one);
        }

        /// <summary>
        /// Adds a margin of <c>0.5f</c> to all sides of the <paramref name="rect"/>.
        /// </summary>
        public static Rect AddMargin(Rect rect)
        {
            const float margin = 0.5f;
            return Rect.MinMaxRect(rect.xMin - margin, rect.yMin - margin, rect.xMax + margin, rect.yMax + margin);
        }
    }
}
