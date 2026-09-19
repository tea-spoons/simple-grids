
namespace TeaSpoons.SimpleGrids
{
    using static TeaSpoons.SimpleGrids.SimpleGridResident;
    using UnityEngine;

    /// <summary>
    /// Provides sophisticated code for placing and moving <see cref="SimpleGridResident"/>s.
    /// </summary>
    public static class AdvancedGridResidentPlacement
    {
        /// <summary>
        /// Attempts to place the <paramref name="resident"/> at the position calculated by a raycast onto the <paramref name="gridGroup"/>.
        /// If this doesn't succeed, an attempt is made to "slide" the <paramref name="resident"/> closer to the target position on its current grid.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        ///   <term><see cref="CellCheckResult.Valid"/></term>
        ///   <description>if all blocks can be placed on the cell under them.</description>
        /// </item>
        /// <item>
        ///   <term><see cref="CellCheckResult.BlockedOrOffGrid"/></term>
        ///   <description>
        ///     if one or more blocks didn't have cells under them, or a cell check returned this value
        ///     (see <see cref="CellCheckFunction"/>).
        ///   </description>
        /// </item>
        /// <item>
        ///   <term><see cref="CellCheckResult.Invalid"/></term>
        ///   <description>
        ///     if all blocks had cells under them
        ///     and the cell check never returned <see cref="CellCheckResult.BlockedOrOffGrid"/>,
        ///     but the cell check (see <see cref="CellCheckFunction"/>) returned <see cref="CellCheckResult.Invalid"/> 
        ///     for one or more block.
        ///   </description>
        /// </item>
        /// </list>
        /// </returns>
        public static CellCheckResult PlaceOnGridGroup(SimpleGridResident resident, SimpleGridGroup gridGroup, Ray ray)
        {
            if (resident == null) throw new System.ArgumentNullException(nameof(resident));
            if (gridGroup == null) throw new System.ArgumentNullException(nameof(gridGroup));

            var placementCheckResult = CellCheckResult.BlockedOrOffGrid;
            var originCellPosition = Vector2Int.zero;
            GridRaycastHit hit;

            if (gridGroup.Raycast(ray, out hit))
            {
                var originGridPosition = resident.CenterToOriginGridPosition(hit.GridPosition);
                originCellPosition = hit.Grid.GridToCellPosition(originGridPosition);

                placementCheckResult = resident.CheckGridPlacement(hit.Grid, originCellPosition);
            }

            // If no position was found, perform a second raycast on the resident's current grid
            // and see if we can slide towards the hit point.
            if (placementCheckResult == CellCheckResult.BlockedOrOffGrid &&
                resident.CurrentGrid != null &&
                resident.CurrentGrid.Group == gridGroup &&
                resident.CurrentGrid.Raycast(ray, out hit))
            {
                var targetGridPosition = resident.CenterToOriginGridPosition(hit.GridPosition);
                var targetCellPosition = resident.CurrentGrid.GridToCellPosition(targetGridPosition);

                if (TrySliding(resident, targetCellPosition, out originCellPosition))
                {
                    placementCheckResult = CellCheckResult.Valid;
                }
            }

            if (placementCheckResult != CellCheckResult.BlockedOrOffGrid)
            {
                resident.PlaceOnGrid(hit.Grid, originCellPosition);
            }

            return placementCheckResult;
        }

        /// <summary>
        /// Attempts to place the <paramref name="resident"/> at the position calculated by a raycast onto the <paramref name="grid"/>.
        /// If this doesn't succeed, an attempt is made to "slide" the <paramref name="resident"/> closer to the target position
        /// on the <paramref name="grid"/>, if it is the <paramref name="resident"/>'s current grid.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        ///   <term><see cref="CellCheckResult.Valid"/></term>
        ///   <description>if all blocks can be placed on the cell under them.</description>
        /// </item>
        /// <item>
        ///   <term><see cref="CellCheckResult.BlockedOrOffGrid"/></term>
        ///   <description>
        ///     if one or more blocks didn't have cells under them, or a cell check returned this value
        ///     (see <see cref="CellCheckFunction"/>).
        ///   </description>
        /// </item>
        /// <item>
        ///   <term><see cref="CellCheckResult.Invalid"/></term>
        ///   <description>
        ///     if all blocks had cells under them
        ///     and the cell check never returned <see cref="CellCheckResult.BlockedOrOffGrid"/>,
        ///     but the cell check (see <see cref="CellCheckFunction"/>) returned <see cref="CellCheckResult.Invalid"/> 
        ///     for one or more block.
        ///   </description>
        /// </item>
        /// </list>
        /// </returns>
        public static CellCheckResult PlaceOnGrid(SimpleGridResident resident, SimpleGrid grid, Ray ray)
        {
            if (resident == null) throw new System.ArgumentNullException(nameof(resident));
            if (grid == null) throw new System.ArgumentNullException(nameof(grid));

            var placementCheckResult = CellCheckResult.BlockedOrOffGrid;
            var originCellPosition = Vector2Int.zero;

            if (grid.Raycast(ray, out var hit))
            {
                var originGridPosition = resident.CenterToOriginGridPosition(hit.GridPosition);
                originCellPosition = hit.Grid.GridToCellPosition(originGridPosition);

                placementCheckResult = resident.CheckGridPlacement(hit.Grid, originCellPosition);
            }

            if (placementCheckResult == CellCheckResult.BlockedOrOffGrid &&
                resident.CurrentGrid == grid &&
                TrySliding(resident, originCellPosition, out originCellPosition))
            {
                placementCheckResult = CellCheckResult.Valid;
            }

            if (placementCheckResult != CellCheckResult.BlockedOrOffGrid)
            {
                resident.PlaceOnGrid(hit.Grid, originCellPosition);
            }

            return placementCheckResult;
        }

        /// <summary>
        /// Attempts to "slide" the <paramref name="resident"/> closer to the <paramref name="targetCellPosition"/>
        /// by checking three neighboring cells of the <paramref name="resident"/>'s current cell position on the <paramref name="grid"/>.
        /// </summary>
        /// <returns><c>true</c> if one of the three cells works as a valid origin cell.</returns>
        private static bool TrySliding(SimpleGridResident resident, Vector2Int targetCellPosition, out Vector2Int originCellPosition)
        {
            var currentCellPosition = resident.CurrentOriginCellPosition;
            var delta = targetCellPosition - currentCellPosition;
            var distance = delta.magnitude;

            var firstOffset = new Vector2Int(Sign(delta.x), Sign(delta.y));
            Vector2Int secondOffset;
            Vector2Int thirdOffset;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                secondOffset = new Vector2Int(Sign(delta.x), 0);
                thirdOffset = new Vector2Int(0, Sign(delta.y));
            }
            else
            {
                secondOffset = new Vector2Int(0, Sign(delta.y));
                thirdOffset = new Vector2Int(Sign(delta.x), 0);
            }

            bool IsOffsetCloserToTargetAndValid(Vector2Int offset)
            {
                var newDistance = Vector2.Distance(currentCellPosition + offset, targetCellPosition);
                if (newDistance >= distance)
                {
                    return false;
                }

                var placementCheckResult = resident.CheckGridPlacement(resident.CurrentGrid,
                            currentCellPosition + offset);

                return placementCheckResult == CellCheckResult.Valid;
            }

            if (IsOffsetCloserToTargetAndValid(firstOffset))
            {
                originCellPosition = currentCellPosition + firstOffset;
                return true;
            }
            if (IsOffsetCloserToTargetAndValid(secondOffset))
            {
                originCellPosition = currentCellPosition + secondOffset;
                return true;
            }
            if (IsOffsetCloserToTargetAndValid(thirdOffset))
            {
                originCellPosition = currentCellPosition + thirdOffset;
                return true;
            }

            originCellPosition = currentCellPosition;
            return false;
        }

        /// <summary>
        /// Like <see cref="Mathf.Sign(float)"/>, but for <see cref="int"/>s.
        /// </summary>
        /// <returns><c>1</c> if <paramref name="i"/> is 0 or positive, <c>-1</c> otherwise.</returns>
        private static int Sign(int i)
        {
            return i >= 0 ? 1 : -1;
        }
    }
}
