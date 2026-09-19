
namespace TeaSpoons.SimpleGrids
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    /// <summary>
    /// Creates a simple one-quad-per-cell mesh to visualize a grid at runtime.
    /// </summary>
    public class PlainGridMeshGenerator : SimpleGridMeshGenerator
    {
        protected override void BuildMesh(Mesh mesh)
        {
            var vertices = new Vector3[grid.CellCount * 4];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[grid.CellCount * 6];

            var cellIndex = 0;
            foreach (var cell in grid)
            {
                var baseIndex = cellIndex * 4;
                var corner = new Vector3(cell.x, 0, cell.y);
                vertices[baseIndex] = corner * grid.CellSize;
                vertices[baseIndex + 1] = (corner + new Vector3(0, 0, 1)) * grid.CellSize;
                vertices[baseIndex + 2] = (corner + new Vector3(1, 0, 1)) * grid.CellSize;
                vertices[baseIndex + 3] = (corner + new Vector3(1, 0, 0)) * grid.CellSize;

                uvs[baseIndex] = new Vector2(0, 0);
                uvs[baseIndex + 1] = new Vector2(0, 1);
                uvs[baseIndex + 2] = new Vector2(1, 1);
                uvs[baseIndex + 3] = new Vector2(1, 0);

                var triBaseIndex = cellIndex * 6;
                triangles[triBaseIndex] = baseIndex;
                triangles[triBaseIndex + 1] = baseIndex + 1;
                triangles[triBaseIndex + 2] = baseIndex + 2;

                triangles[triBaseIndex + 3] = baseIndex;
                triangles[triBaseIndex + 4] = baseIndex + 2;
                triangles[triBaseIndex + 5] = baseIndex + 3;

                cellIndex++;
            }

            for (var i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
        }
    }
}
