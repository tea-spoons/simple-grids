
namespace TeaSpoons.SimpleGrids
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A dictionary for holding any kind of cell data for a given (grid, cell position) key.
    /// </summary>
    /// <typeparam name="TCellData">The data type defining the properties of each cell.</typeparam>
    public class SimpleGridCellDictionary<TCellData>
        where TCellData : struct
    {
        private readonly struct Key
        {
            public readonly SimpleGrid Grid;
            public readonly Vector2Int CellPosition;

            public Key(SimpleGrid grid, Vector2Int cellPosition)
            {
                Grid = grid;
                CellPosition = cellPosition;
            }

            public override bool Equals(object obj)
            {
                return obj is Key other &&
                    other.Grid == Grid &&
                    other.CellPosition == CellPosition;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Grid, CellPosition);
            }

            public static implicit operator Key((SimpleGrid grid, Vector2Int cellPosition) tuple)
            {
                return new Key(tuple.grid, tuple.cellPosition);
            }
        }

        private readonly Dictionary<Key, TCellData> dictionary = new();

        /// <summary>
        /// Sets the given <paramref name="cellData"/> for the given cell.
        /// Overrides any previous data.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="grid"/> is null.</exception>
        public void SetCellData(in SimpleGrid grid, in Vector2Int cellPosition, TCellData cellData)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            dictionary[(grid, cellPosition)] = cellData;
        }

        /// <summary>
        /// Removes the (<paramref name="grid"/>, <paramref name="cellPosition"/>) key from the dictionary.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="grid"/> is null.</exception>
        public void RemoveCellData(in SimpleGrid grid, in Vector2Int cellPosition)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            dictionary.Remove((grid, cellPosition));
        }

        /// <summary>
        /// Attempts to get the cell data attached to the (<paramref name="grid"/>, <paramref name="cellPosition"/>) key.
        /// </summary>
        /// <param name="cell"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="grid"/> is null.</exception>
        public bool TryGetCellData(in SimpleGrid grid, in Vector2Int cellPosition, out TCellData cell)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            return dictionary.TryGetValue((grid, cellPosition), out cell);
        }

        /// <summary>
        /// Returns whether there is cell data attached to cell data attached to the (<paramref name="grid"/>, <paramref name="cellPosition"/>) key
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="grid"/> is null.</exception>
        public bool HasCellData(in SimpleGrid grid, in Vector2Int cellPosition)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            return dictionary.ContainsKey((grid, cellPosition));
        }
    }
}
