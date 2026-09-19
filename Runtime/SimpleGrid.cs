
namespace TeaSpoons.SimpleGrids
{
    using UnityEngine;
    using TeaSpoons.PackageCore;
    using System.Collections.Generic;
    using System;
    using System.Collections;

    /// <summary>
    /// A simple representation of a 2d grid in 3d space with rectangular cells.
    /// </summary>
    public abstract class SimpleGrid : MonoBehaviour, IEnumerable<Vector2Int>
    {
        private Transform _transform;
        new private Transform transform => _transform == null ? _transform = base.transform : _transform;

        /// <summary>
        /// Invoked when the grid is updated, including initialization.
        /// </summary>
        public event Action OnGridUpdated = delegate { };

        /// <summary>
        /// The size of each cell in the grid.
        /// </summary>
        public float CellSize => cellSize;

        /// <summary>
        /// The bounding rectangle around all cells in grid space.
        /// </summary>
        public Rect Bounds => bounds;

        /// <summary>
        /// The <see cref="SimpleGridGroup"/> this grid is registered to.
        /// </summary>
        public SimpleGridGroup Group => group;

        /// <summary>
        /// <c>true</c> if this grid has one or more cells.
        /// </summary>
        public int CellCount => cells.Count;

        [SerializeField]
        protected internal float cellSize = 1f;

        [SerializeField]
        private PlayModeEditable<SimpleGridGroup> group;

        private readonly HashSet<Vector2Int> cells = new();
        private Rect bounds;


        private void Awake()
        {
            UpdateGrid();

            group.Value?.Add(this);
#if UNITY_EDITOR
            group.OnUpdate = (oldGroup, newGroup) =>
            {
                oldGroup?.Remove(this);
                newGroup?.Add(this);
            };
#endif
        }

        private void OnDestroy()
        {
            group.Value?.Remove(this);

#if UNITY_EDITOR
            group.OnUpdate = null;
#endif
        }

        #region World Position > Grid Position > Cell Position
        /// <summary>
        /// Maps the position of <paramref name="worldPosition"/> into this grid's cell space on its plane.
        /// Any distance to the grid along its normal is ignored.
        /// </summary>
        public Vector2 WorldToGridPosition(Vector3 worldPosition)
        {
            var relativePosition = transform.InverseTransformPoint(worldPosition);
            return new Vector2(relativePosition.x, relativePosition.z) / cellSize;
        }

        /// <summary>
        /// Calculates the cell position of the cell at <paramref name="gridPosition"/>.
        /// </summary>
        /// <remarks>
        /// There isn't necessarily a cell at this position.
        /// </remarks>
        public Vector2Int GridToCellPosition(Vector2 gridPosition)
        {
            return RoundingUtility.NextSmallerInts(gridPosition);
        }

        /// <summary>
        /// Maps the position of <paramref name="worldPosition"/> into this grid's cell space on its plane.
        /// Then returns the cell position at the resulting grid position.
        /// </summary>
        /// <remarks>
        /// There isn't necessarily a cell at this position.
        /// </remarks>
        public Vector2Int WorldToCellPosition(Vector3 worldPosition)
        {
            return GridToCellPosition(WorldToGridPosition(worldPosition));
        }

        /// <summary>
        /// Converts the <paramref name="gridPosition"/> to a <paramref name="cellPosition"/>.
        /// </summary>
        /// <returns><c>true</c> if <paramref name="cellPosition"/> is a cell on the grid, <c>false</c> if there is no cell.</returns>
        public bool TryGetCell(Vector2 gridPosition, out Vector2Int cellPosition)
        {
            cellPosition = GridToCellPosition(gridPosition);

            return HasCellAt(cellPosition);
        }
        #endregion

        #region Cell Position > Grid Position > World Position
        /// <summary>
        /// Calculates the grid position of a given <paramref name="cellPosition"/>.
        /// </summary>
        /// <remarks>
        /// Note: This will return the corner of the cell on the grid.
        /// </remarks>
        public Vector2 CellToGridPosition(Vector2Int cellPosition)
        {
            return new Vector2(cellPosition.x, cellPosition.y);
        }

        /// <summary>
        /// Returns the world position of the given <paramref name="gridPosition"/>.
        /// </summary>
        public Vector3 GridToWorldPosition(Vector2 gridPosition)
        {
            return transform.TransformPoint(new Vector3(gridPosition.x, 0, gridPosition.y) * cellSize);
        }

        /// <summary>
        /// Calculates the world position of the corner of the given <paramref name="cellPosition"/>.
        /// </summary>
        /// <remarks>
        /// Note: This will return the corner of the cell in world space.
        /// </remarks>
        public Vector3 CellToWorldPosition(Vector2Int cellPosition)
        {
            return GridToWorldPosition(CellToGridPosition(cellPosition));
        }
        #endregion

        /// <summary>
        /// Returns whether there is a cell at the given <paramref name="cellPosition"/> on the grid.
        /// </summary>
        public bool HasCellAt(Vector2Int cellPosition)
        {
            return cells.Contains(cellPosition);
        }

        /// <summary>
        /// Casts a <paramref name="ray"/> against the grid's plane.
        /// </summary>
        /// <remarks>
        /// Does NOT check whether a cell was hit - returns true if the ray hit *anywhere* on the plane.
        /// </remarks>
        /// <param name="hit">Information about the hit, if there was any.</param>
        /// <returns><c>true</c> if the <paramref name="ray"/> hits the grid's plane.</returns>
        public bool Raycast(Ray ray, out GridRaycastHit hit)
        {
            var plane = new Plane(transform.up, transform.position);
            if (plane.Raycast(ray, out var enter))
            {
                var gridPosition = WorldToGridPosition(ray.GetPoint(enter));

                hit = new GridRaycastHit(this, gridPosition, enter);
                return true;
            }

            hit = GridRaycastHit.None;
            return false;
        }

        protected abstract IEnumerable<Vector2Int> GetCells();

        /// <summary>
        /// Updates the cells, recalculates the <see cref="bounds"/> and invokes <see cref="OnGridUpdated"/>.
        /// </summary>
        public void UpdateGrid()
        {
            var first = true;
            foreach (var cell in GetCells())
            {
                if (first)
                {
                    bounds = new Rect(cell, Vector2.zero);
                    first = false;
                }
                else
                {
                    bounds = RectUtility.ExtendToContain(bounds, cell);
                }
                cells.Add(cell);
            }

            OnGridUpdated();
        }

        public IEnumerator<Vector2Int> GetEnumerator()
        {
            return cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void OnDrawGizmosSelected()
        {
            var transform = base.transform;

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(cellSize, 1f, cellSize));

            var bounds = new Rect();

            using (GizmosColor.Override(new Color(0.7f, 0.8f, 1f, 0.7f)))
            {
                var first = true;
                foreach (var cell in GetCells())
                {
                    if (first)
                    {
                        bounds = new Rect(cell, Vector2.zero);
                        first = false;
                    }
                    else
                    {
                        bounds = RectUtility.ExtendToContain(bounds, cell);
                    }
                    Gizmos.DrawWireCube(new Vector3(cell.x + 0.5f, 0f, cell.y + 0.5f),
                        new Vector3(1f, 0f, 1f));
                }

                bounds = RectUtility.EnlargeByOne(bounds);
                bounds = RectUtility.AddMargin(bounds);
            }

            using (GizmosColor.Override(new Color(0.3f, 0.4f, 1f, 0.7f)))
            {
                Gizmos.DrawWireCube(new Vector3(bounds.center.x, 0f, bounds.center.y),
                    new Vector3(bounds.size.x, 0f, bounds.size.y));
            }
        }
    }
}
