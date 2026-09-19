
namespace TeaSpoons.SimpleGrids
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// A <see cref="SimpleGrid"/> that has a rectangle full of cells.
    /// </summary>
    [AddComponentMenu(PackageCore.Menus.RootItem + "Simple Grids/Rect Grid")]
    public class SimpleRectGrid : SimpleGrid
    {
        [SerializeField]
        private Vector2Int size = new Vector2Int(8, 8);

        protected override IEnumerable<Vector2Int> GetCells()
        {
            for (var y = 0; y < size.y; y++)
            {
                for (var x = 0; x < size.x; x++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }

        private void OnValidate()
        {
            if (size.x < 1)
            {
                size.x = 1;
            }
            if (size.y < 1)
            {
                size.y = 1;
            }
        }
    }
}
