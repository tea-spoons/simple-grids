
namespace TeaSpoons.SimpleGrids
{
    using UnityEngine;

    /// <summary>
    /// The result of a successful raycast against a <see cref="SimpleGrid"/> or a <see cref="SimpleGridGroup">.
    /// </summary>
    public readonly struct GridRaycastHit
    {
        /// <summary>
        /// A <see cref="GridRaycastHit"/> that represents a non-hit.
        /// </summary>
        internal static GridRaycastHit None => new GridRaycastHit(null, Vector2.zero, Mathf.Infinity);

        /// <summary>
        /// The grid that was hit.
        /// </summary>
        public readonly SimpleGrid Grid;
        /// <summary>
        /// The cell size-sensitive local position the grid was hit on.
        /// </summary>
        public readonly Vector2 GridPosition;
        /// <summary>
        /// The distance that the ray travelled.
        /// </summary>
        public readonly float Distance;

        internal GridRaycastHit(SimpleGrid grid, Vector2 gridPosition, float distance)
        {
            Grid = grid;
            GridPosition = gridPosition;
            Distance = distance;
        }
    }
}
