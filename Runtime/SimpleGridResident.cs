
namespace TeaSpoons.SimpleGrids
{
    using UnityEngine;
    using System.Collections.Generic;
    using TeaSpoons.PackageCore;

    /// <summary>
    /// Base class for objects that have a shape that is to be placed on a <see cref="SimpleGrid"/>.
    /// Allows for positioning, rotating and checking grid cells on which we might place the object.
    /// </summary>
    public abstract class SimpleGridResident : MonoBehaviour
    {
        /// <summary>
        /// The result of a cell check, or a series of cell checks.
        /// </summary>
        /// <remarks>
        /// See the individual values for more info.
        /// </remarks>
        public enum CellCheckResult
        {
            /// <summary>
            /// Returned if all blocks are over a cell
            /// and no <see cref="CellCheckFunction"/> returned anything else than this value.
            /// </summary>
            Valid,
            /// <summary>
            /// Returned if a block is over a cell,
            /// but the <see cref="CellCheckFunction"/> returned this value.
            /// </summary>
            /// <remarks>
            /// If this is returned by a placement check, the resident will move to the target position,
            /// but refuses to be placed there.
            /// This should usually be accompanied by a visual indicator that shows that no placement is possible.
            /// </remarks>
            Invalid,
            /// <summary>
            /// Returned if a block is above a position with no cell,
            /// or the <see cref="CellCheckFunction"/> returned this value.
            /// </summary>
            /// <remarks>
            /// The resident will not move to this position if this is returned by a placement check.
            /// </remarks>
            BlockedOrOffGrid
        }

        /// <summary>
        /// The delegate that is used to check whether a specific block can be placed over a specific cell.
        /// </summary>
        /// <param name="grid">The grid that the resident is supposed to be placed on.</param>
        /// <param name="cellPosition">The position of the cell that the block is over.</param>
        /// <param name="blockPosition">The resident-local position of the block.</param>
        /// <returns>See <see cref="CellCheckResult"/> values.</returns>
        public delegate CellCheckResult CellCheckFunction(SimpleGrid grid, Vector2Int cellPosition, Vector2Int blockPosition);

        public static float MovementSpeed { get; set; } = 30f;
        public static float RotationSpeed { get; set; } = 30f;
        public static float ScaleSpeed { get; set; } = 30f;

        private new Transform transform;

        /// <summary>
        /// The amount of possible orientations.
        /// </summary>
        /// <remarks>
        /// Currently constantly 4, assuming rectangular cells.
        /// </remarks>
        private const byte orientationCount = 4;

        public SimpleGrid CurrentGrid { get; private set; }
        public Vector2Int CurrentOriginCellPosition { get; private set; }
        public bool CanBePlaced { get; private set; } = false;
        public RectInt Bounds { get; private set; }

        /// <summary>
        /// A <see cref="CellCheckFunction"> that is called for every cell position during <see cref="CheckGridPlacement(SimpleGrid, Vector2Int)"/>.
        /// </summary>
        protected CellCheckFunction cellCheck;

        private byte orientation = 0;
        private Vector3 targetPosition;
        private Quaternion targetRotation;


        protected virtual void Awake()
        {
            transform = base.transform;

            RecalculateBounds();

            targetPosition = transform.position;
            targetRotation = transform.rotation;
        }

        protected virtual void FixedUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, MovementSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

            var targetScale = Vector3.one * (CurrentGrid ? CurrentGrid.CellSize : 1f);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, ScaleSpeed * Time.deltaTime);
        }

        #region Center-Origin Position Conversions
        /// <summary>
        /// Returns the origin grid position of this object, assuming the given <paramref name="centerGridPosition"/>.
        /// </summary>
        /// <remarks>
        /// The origin position is the (min, min) corner of the object.
        /// </remarks>
        public Vector2 CenterToOriginGridPosition(Vector2 centerGridPosition)
        {
            var size = GetOrientedBoundsSize();

            var cornerGridPosition = centerGridPosition - size / 2;
            if (size.x % 2 == 0)
            {
                cornerGridPosition.x += 0.5f;
            }
            if (size.y % 2 == 0)
            {
                cornerGridPosition.y += 0.5f;
            }
            return cornerGridPosition;
        }

        /// <summary>
        /// Returns the center grid position of this object, assuming the given <paramref name="originGridPosition"/>.
        /// </summary>
        public Vector2 OriginToCenterGridPosition(Vector2 originGridPosition)
        {
            var size = GetOrientedBoundsSize();

            return originGridPosition + new Vector2(size.x, size.y) * 0.5f;
        }
        #endregion

        #region Placing on Grids
        /// <summary>
        /// Officially moves the object onto a specific cell by updating <see cref="CurrentGrid"/> and <see cref="CurrentOriginCellPosition"/>.
        /// Starts translating towards the resulting world position and rotation.
        /// </summary>
        /// <remarks>
        /// Interpolates towards the resulting position and rotation.
        /// Use <see cref="FinishMovement"/> to instantly finish the movement
        /// or <see cref="PlaceOnGridImmediately(SimpleGrid, Vector2Int)"/> to not interpolate at all.
        /// </remarks>
        public void PlaceOnGrid(SimpleGrid grid, Vector2Int originCellPosition)
        {
            CurrentGrid = grid;
            CurrentOriginCellPosition = originCellPosition;

            var cornerGridPosition = grid.CellToGridPosition(originCellPosition);
            var size = GetOrientedBoundsSize();
            var centerGridPosition = cornerGridPosition + size / 2; 
            if (size.x % 2 == 1)
            {
                centerGridPosition.x += 0.5f;
            }
            if (size.y % 2 == 1)
            {
                centerGridPosition.y += 0.5f;
            }
            var worldPosition = grid.GridToWorldPosition(centerGridPosition);

            targetPosition = worldPosition;
            targetRotation = grid.transform.rotation * Quaternion.Euler(0, orientation * 90f, 0);
        }

        /// <summary>
        /// Places the object at the given <paramref name="cellPosition"/> on the given <paramref name="grid"/>.
        /// </summary>
        /// <remarks>
        /// To smoothly interpolate towards the resulting position and rotation, use <see cref="PlaceOnGrid(SimpleGrid, Vector2Int)"/>.
        /// </remarks>
        public void PlaceOnGridImmediately(SimpleGrid grid, Vector2Int cellPosition)
        {
            PlaceOnGrid(grid, cellPosition);
            FinishMovement();
        }

        /// <summary>
        /// Stops movement/rotation tweening and immediately applies target position and rotation.
        /// </summary>
        public void FinishMovement()
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
            transform.localScale = Vector3.one * CurrentGrid.CellSize;
        }

        /// <summary>
        /// Checks whether all blocks fit on the <paramref name="grid"/> when using <paramref name="originCellPosition"/> as the origin cell position.
        /// </summary>
        /// <param name="grid">The grid to check the cells on.</param>
        /// <param name="originCellPosition">The origin cell, on which the (min, min) corner is to be placed.</param>
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
        public CellCheckResult CheckGridPlacement(SimpleGrid grid, Vector2Int originCellPosition)
        {
            var invalidCellFound = false;

            foreach (var (cellPosition, localBlockPosition) in GetBlocksAndMatchingCells(originCellPosition))
            {
                if (!grid.HasCellAt(cellPosition))
                {
                    return CellCheckResult.BlockedOrOffGrid;
                }

                if (cellCheck != null)
                {
                    var cellCheckResult = cellCheck(grid, cellPosition, localBlockPosition);

                    switch (cellCheckResult)
                    {
                        case CellCheckResult.BlockedOrOffGrid:
                            return CellCheckResult.BlockedOrOffGrid;
                        case CellCheckResult.Invalid:
                            invalidCellFound = true;
                            break;
                    }
                }
            }

            CanBePlaced = !invalidCellFound;
            return invalidCellFound ? CellCheckResult.Invalid : CellCheckResult.Valid;
        }
        #endregion

        #region Rotate Methods
        /// <summary>
        /// Updates the orientation by rotating clockwise once.
        /// </summary>
        public void RotateClockwise()
        {
            orientation++;
            if (orientation == orientationCount)
            {
                orientation = 0;
            }
        }

        /// <summary>
        /// Updates the orientation by rotating counterclockwise once.
        /// </summary>
        public void RotateCounterclockwise()
        {
            if (orientation == 0)
            {
                orientation = orientationCount;
            }
            orientation--;
        }
        #endregion

        /// <summary>
        /// Returns an <see cref="IEnumerable{Vector2Int}"/> with all of this object's oriented blocks.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vector2Int> GetOrientedBlocks()
        {
            foreach (var block in GetBlocks())
            {
                yield return OrientBlock(block);
            }
        }

        /// <summary>
        /// Returns one tuple for each block of this building - the tuple contains the block position (not oriented) and the position of the cell it is currently placed on.
        /// </summary>
        /// <exception cref="System.Exception">Thrown if the resident is not currently on a grid. Call <see cref="PlaceOnGrid(SimpleGrid, Vector2Int)"/> first to avoid this.</exception>
        public IEnumerable<(Vector2Int cellPosition, Vector2Int localBlockPosition)> GetBlocksAndMatchingCells()
        {
            if (!CurrentGrid)
            {
                throw new System.Exception($"Calling {nameof(GetBlocksAndMatchingCells)} on a grid resident that is not currently on a grid ({CurrentGrid} == null).");
            }

            return GetBlocksAndMatchingCells(CurrentOriginCellPosition);
        }

        protected abstract IEnumerable<Vector2Int> GetBlocks();

        private IEnumerable<(Vector2Int cellPosition, Vector2Int localBlockPosition)> GetBlocksAndMatchingCells(Vector2Int originCellPosition)
        {
            foreach (var block in GetBlocks())
            {
                var orientedBlock = OrientBlock(block);

                var cellPosition = originCellPosition + orientedBlock;
                yield return (cellPosition, block);
            }
        }

        private void RecalculateBounds()
        {
            var bounds = new RectInt();

            var first = true;
            foreach (var cell in GetBlocks())
            {
                if (first)
                {
                    bounds = new RectInt(cell, Vector2Int.zero);
                    first = false;
                }
                else
                {
                    bounds = RectUtility.ExtendToContain(bounds, cell);
                }
            }

            bounds = RectUtility.EnlargeByOne(bounds);

            Bounds = bounds;
        }

        /// <summary>
        /// Orients a block depending on the current <see cref="orientation"/>.
        /// </summary>
        private Vector2Int OrientBlock(Vector2Int block)
        {
            switch (orientation)
            {
                case 1:
                    return new Vector2Int(block.y, Bounds.width - 1 - block.x);
                case 2:
                    return new Vector2Int(Bounds.width - 1 - block.x, Bounds.height - 1 - block.y);
                case 3:
                    return new Vector2Int(Bounds.height - 1 - block.y, block.x);
                default:
                    return block;
            }
        }

        /// <summary>
        /// Returns <see cref="Bounds.size"/>, but with X and Y swapped if the object is rotated by 90° or 270°.
        /// </summary>
        private Vector2Int GetOrientedBoundsSize()
        {
            if (orientation % 2 == 0)
            {
                return new Vector2Int(Bounds.width, Bounds.height);
            }
            else
            {
                return new Vector2Int(Bounds.height, Bounds.width);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var transform = base.transform;

            RecalculateBounds();
            var boundsSize = GetOrientedBoundsSize();
            var halfBoundsGroundSize = GetGroundVector(boundsSize) * 0.5f;

            Gizmos.matrix = Matrix4x4.TRS(transform.position,
                transform.rotation * Quaternion.Euler(0f, orientation * -90f, 0f),
                transform.localScale);

            using (GizmosColor.Override(new Color(1f, 0.5f, 0.3f, 0.8f)))
            {
                foreach (var block in GetBlocks())
                {
                    var blockPosition = -halfBoundsGroundSize + GetGroundVector(OrientBlock(block)) + new Vector3(0.5f, 0f, 0.5f);
                    Gizmos.DrawCube(blockPosition,
                        new Vector3(0.8f, 0.2f, 0.8f));
                }
            }

            using (GizmosColor.Override(Color.yellow))
            {
                Gizmos.DrawWireCube(Vector3.zero,
                    new Vector3(boundsSize.x + 0.1f, 0f, boundsSize.y + 0.1f));
            }
        }

        private static Vector3 GetGroundVector(Vector2 v)
        {
            return new Vector3(v.x, 0f, v.y);
        }
    }
}
