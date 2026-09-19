
namespace TeaSpoons.SimpleGrids
{
    using UnityEngine;

    /// <summary>
    /// Base component class that creates a mesh that visually represents a parent <see cref="SimpleGrid"/> at runtime.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public abstract class SimpleGridMeshGenerator : MonoBehaviour
    {
        protected SimpleGrid grid { get; private set; }
        private MeshFilter meshFilter;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();

            grid = GetComponentInParent<SimpleGrid>();

            // TODO Consider adding 13Pixels.Validation package instead
            if (grid == null)
            {
                throw new MissingComponentException();
            }

            if (grid.CellCount > 0)
            {
                RebuildMesh();
            }

            grid.OnGridUpdated += RebuildMesh;
        }

        private void OnDestroy()
        {
            grid.OnGridUpdated -= RebuildMesh;
            DestroyExistingMesh();
        }

        protected abstract void BuildMesh(Mesh mesh);

        private void RebuildMesh()
        {
            DestroyExistingMesh();

            var mesh = new Mesh();

            BuildMesh(mesh);

            meshFilter.sharedMesh = mesh;
        }

        private void DestroyExistingMesh()
        {
            if (meshFilter.sharedMesh != null)
            {
                DestroyImmediate(meshFilter.sharedMesh);
            }
        }
    }
}
