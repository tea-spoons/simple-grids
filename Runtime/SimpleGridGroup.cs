
namespace TeaSpoons.SimpleGrids
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A group of <see cref="SimpleGrid"/>s that can be raycasted against.
    /// </summary>
    [CreateAssetMenu(menuName = PackageCore.Menus.RootItem + "Simple Grids/SimpleGrid Group")]
    public class SimpleGridGroup : ScriptableObject
    {
        private readonly HashSet<SimpleGrid> grids = new();

        /// <summary>
        /// Raycasts against all grids in this group.
        /// </summary>
        /// <remarks>
        /// Only returns true when an actual cell was hit.
        /// </remarks>
        /// <param name="hit">The <see cref="GridRaycastHit"/> containing information about what grid was hit and where.</param>
        /// <returns><c>true</c> if any of the grids was hit, <c>false</c> otherwise.</returns>
        public bool Raycast(Ray ray, out GridRaycastHit hit)
        {
            hit = GridRaycastHit.None;
            var result = false;

            foreach (var grid in grids)
            {
                if (grid.Raycast(ray, out var currentHit) &&
                    currentHit.Distance <= hit.Distance &&
                    grid.HasCellAt(grid.GridToCellPosition(currentHit.GridPosition)))
                {
                    hit = currentHit;
                    result = true;
                }
            }

            return result;
        }

        internal void Add(SimpleGrid group)
        {
            grids.Add(group);
        }

        internal void Remove(SimpleGrid group)
        {
            grids.Remove(group);
        }
    }
}
