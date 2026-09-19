
namespace TeaSpoons.SimpleGrids
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// A <see cref="SimpleGrid"/> that has an unconstrained list of cells.
    /// </summary>
    [AddComponentMenu(PackageCore.Menus.RootItem + "Simple Grids/Freeform Grid")]
    public class SimpleFreeformGrid : SimpleGrid
    {
        [SerializeField]
        private List<Vector2Int> cells;

        protected override IEnumerable<Vector2Int> GetCells()
        {
            if (cells == null) yield break;

            foreach (var cell in cells)
            {
                yield return cell;
            }
        }
    }
}
