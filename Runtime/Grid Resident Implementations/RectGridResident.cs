
namespace TeaSpoons.SimpleGrids
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// A <see cref="SimpleGridResident"/> with a rectangular shape.
    /// </summary>
    public class RectGridResident : SimpleGridResident
    {
        [SerializeField]
        private Vector2Int size = new Vector2Int(2, 2);

        public CellCheckFunction CellCheck
        {
            get => cellCheck;
            set => cellCheck = value;
        }

        protected override IEnumerable<Vector2Int> GetBlocks()
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
