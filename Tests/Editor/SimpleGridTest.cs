
namespace TeaSpoons.SimpleGrids.Editor.Tests
{
    using UnityEngine;
    using NUnit.Framework;
    using System.Collections.Generic;

    public class SimpleGridTest
    {
        private class TestGrid : SimpleGrid
        {
            protected override IEnumerable<Vector2Int> GetCells()
            {
                for (var x = 0; x < 5; x++)
                { 
                    yield return new Vector2Int(x, 0);
                }
            }

            public void SetCellSize(float cellSize)
            {
                this.cellSize = cellSize;
            }
        }

        private TestGrid grid;

        [SetUp]
        public void SetUp()
        {
            grid = new GameObject("SimpleGrid Test").AddComponent<TestGrid>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(grid.gameObject);
        }

        [Test]
        public void WorldToGridPosition()
        {
#line hidden
            void AssertResult(Vector2 expected, Vector3 worldPosition)
            {
                AssertApproximatelyEqual(expected, grid.WorldToGridPosition(worldPosition));
            }
#line default

            AssertResult(Vector2.zero, Vector3.up);
            AssertResult(new Vector2(2f, 0), new Vector3(2f, 1f, 0f));
            AssertResult(new Vector2(-2f, 20f), new Vector3(-2f, -2f, 20f));

            grid.SetCellSize(2f);
            AssertResult(Vector2.zero, Vector3.up);
            AssertResult(new Vector2(1f, 0f), new Vector3(2f, 1f, 0f));
            AssertResult(new Vector2(-1f, 10f), new Vector3(-2f, -2f, 20f));

            // TODO Rotation should be tested, but this doesn't work yet because of unsynced transforms.
            /*
            grid.SetCellSize(Vector2.one);
            grid.transform.rotation = Quaternion.Euler(90f, 0, 0);
            Physics.SyncTransforms();
            AssertResult(Vector2.zero, Vector3.right);
            AssertResult(new Vector2(2f, 0), new Vector3(1f, 2f, 0f));
            AssertResult(new Vector2(-2f, 20f), new Vector3(-1f, -2f, 20f));
            */
        }

        [Test]
        public void GridToCellPosition()
        {
#line hidden
            void AssertResult(Vector2Int expected, Vector2 gridPosition)
            {
                Assert.AreEqual(expected, grid.GridToCellPosition(gridPosition));
            }
#line default

            AssertResult(new Vector2Int(0, 1), new Vector2(0.1f, 1.9f));
            AssertResult(new Vector2Int(-1, 1), new Vector2(-0.1f, 1.9f));
            AssertResult(new Vector2Int(0, 10), new Vector2(0f, 10.9f));
            AssertResult(new Vector2Int(0, -11), new Vector2(0.1f, -10.9f));

            grid.SetCellSize(2f);
            AssertResult(new Vector2Int(0, 1), new Vector2(0.1f, 1.9f));
            AssertResult(new Vector2Int(-1, 1), new Vector2(-0.1f, 1.9f));
            AssertResult(new Vector2Int(0, 10), new Vector2(0f, 10.9f));
            AssertResult(new Vector2Int(0, -11), new Vector2(0.1f, -10.9f));
        }

        [Test]
        public void WorldToCellPosition()
        {
#line hidden
            void AssertResult(Vector2Int expected, Vector3 worldPosition)
            {
                Assert.AreEqual(expected, grid.WorldToCellPosition(worldPosition));
            }
#line default

            AssertResult(new Vector2Int(0, 1), new Vector3(0.1f, 1f, 1.9f));
            AssertResult(new Vector2Int(-1, 1), new Vector3(-0.1f, 1f, 1.9f));
            AssertResult(new Vector2Int(0, 10), new Vector3(0f, -1f, 10.9f));
            AssertResult(new Vector2Int(0, -11), new Vector3(0.1f, -1f, -10.9f));

            grid.SetCellSize(2f);
            AssertResult(new Vector2Int(0, 1), new Vector3(0.1f, 1f, 3.9f));
            AssertResult(new Vector2Int(-1, 1), new Vector3(-0.1f, 1f, 3.9f));
            AssertResult(new Vector2Int(0, 10), new Vector3(0f, -1f, 20.9f));
            AssertResult(new Vector2Int(0, -11), new Vector3(0.1f, -1f, -21.9f));
        }

        [Test]
        public void CellToGridPosition()
        {
#line hidden
            void AssertResult(Vector2 expected, Vector2Int cellPosition)
            {
                AssertApproximatelyEqual(expected, grid.CellToGridPosition(cellPosition));
            }
#line default

            AssertResult(Vector3.zero, Vector2Int.zero);
            AssertResult(new Vector2(2f, 0f), new Vector2Int(2, 0));
            AssertResult(new Vector2(-2f, 20f), new Vector2Int(-2, 20));

            grid.SetCellSize(2f);
            AssertResult(Vector2.zero, Vector2Int.zero);
            AssertResult(new Vector2(1f, 0f), new Vector2Int(1, 0));
            AssertResult(new Vector2(-1f, 10f), new Vector2Int(-1, 10));
        }

        [Test]
        public void GridToWorldPosition()
        {
#line hidden
            void AssertResult(Vector3 expected, Vector2 gridPosition)
            {
                AssertApproximatelyEqual(expected, grid.GridToWorldPosition(gridPosition));
            }
#line default

            AssertResult(Vector3.zero, Vector2.zero);
            AssertResult(new Vector3(2f, 0f, 0f), new Vector2(2f, 0f));
            AssertResult(new Vector3(-2f, 0f, 20f), new Vector2(-2f, 20f));

            grid.SetCellSize(2f);
            AssertResult(Vector3.zero, Vector2Int.zero);
            AssertResult(new Vector3(4f, 0f, 0f), new Vector2(2f, 0f));
            AssertResult(new Vector3(-5f, 0f, 40f), new Vector2(-2.5f, 20f));
        }

        [Test]
        public void CellToWorldPosition()
        {
#line hidden
            void AssertResult(Vector3 expected, Vector2Int cellPosition)
            {
                AssertApproximatelyEqual(expected, grid.CellToWorldPosition(cellPosition));
            }
#line default

            AssertResult(Vector3.zero, Vector2Int.zero);
            AssertResult(new Vector3(2f, 0f, 0f), new Vector2Int(2, 0));
            AssertResult(new Vector3(-2f, 0f, 20f), new Vector2Int(-2, 20));

            grid.SetCellSize(2f);
            AssertResult(Vector2.zero, Vector2Int.zero);
            AssertResult(new Vector3(2f, 0f, 0f), new Vector2Int(1, 0));
            AssertResult(new Vector3(-2f, 0f, 20f), new Vector2Int(-1, 10));
        }

#line hidden
        private static void AssertApproximatelyEqual(Vector2 expected, Vector2 actual)
        {
            Assert.LessOrEqual(Vector2.Distance(expected, actual), Mathf.Epsilon, $"Expected {expected}, but got {actual}.");
        }

        private static void AssertApproximatelyEqual(Vector3 expected, Vector3 actual)
        {
            Assert.LessOrEqual(Vector2.Distance(expected, actual), Mathf.Epsilon, $"Expected {expected}, but got {actual}.");
        }
#line default
    }
}
