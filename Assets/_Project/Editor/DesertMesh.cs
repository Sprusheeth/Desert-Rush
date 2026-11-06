using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesertRush.Editor
{
    internal sealed class DesertMesh
    {
        private readonly List<Vector3> vertices = new();
        private readonly List<Vector2> uv = new();
        private readonly List<int>[] indices;

        public DesertMesh(int slots)
        {
            indices = new List<int>[slots];
            for (int i = 0; i < slots; i++) indices[i] = new List<int>();
        }

        public void Face(int slot, params Vector3[] points)
        {
            int first = vertices.Count;
            var normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]).normalized;
            foreach (var point in points)
            {
                vertices.Add(point);
                if (Mathf.Abs(normal.y) > .6f) uv.Add(new Vector2(point.x, point.z));
                else uv.Add(new Vector2(point.x + point.z, point.y));
            }
            for (int i = 1; i < points.Length - 1; i++)
            {
                indices[slot].Add(first);
                indices[slot].Add(first + i);
                indices[slot].Add(first + i + 1);
            }
        }

        public void Beam(int slot, Vector3 center, Vector3 size, Quaternion rotation, float bevel = .025f)
        {
            var half = size * .5f;
            bevel = Mathf.Min(bevel, Mathf.Min(half.x, Mathf.Min(half.y, half.z)) * .65f);
            Vector2[] outline =
            {
                new(-half.x + bevel, -half.z), new(half.x - bevel, -half.z),
                new(half.x, -half.z + bevel), new(half.x, half.z - bevel),
                new(half.x - bevel, half.z), new(-half.x + bevel, half.z),
                new(-half.x, half.z - bevel), new(-half.x, -half.z + bevel)
            };
            var bottom = new Vector3[8];
            var top = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                bottom[i] = center + rotation * new Vector3(outline[i].x, -half.y, outline[i].y);
                top[i] = center + rotation * new Vector3(outline[i].x, half.y, outline[i].y);
            }
            for (int i = 0; i < 8; i++)
            {
                int next = (i + 1) % 8;
                Face(slot, bottom[i], top[i], top[next], bottom[next]);
            }
            Face(slot, bottom);
            Array.Reverse(top);
            Face(slot, top);
        }

        public void Beam(int slot, Vector3 center, Vector3 size, float bevel = .025f) =>
            Beam(slot, center, size, Quaternion.identity, bevel);

        public void Lathe(int slot, Vector3 origin, Quaternion rotation, int sides, params Vector3[] rings)
        {
            // Each ring stores height, X radius and Z radius.
            Vector3 Point(int ring, int side)
            {
                float angle = side * 2f * Mathf.PI / sides;
                Vector3 r = rings[ring];
                return origin + rotation * new Vector3(Mathf.Cos(angle) * r.y, r.x, Mathf.Sin(angle) * r.z);
            }

            for (int r = 0; r < rings.Length - 1; r++)
            {
                for (int s = 0; s < sides; s++)
                    Face(slot, Point(r, s), Point(r + 1, s), Point(r + 1, s + 1), Point(r, s + 1));
            }

            for (int s = 0; s < sides; s++)
            {
                Face(slot, origin + rotation * Vector3.up * rings[0].x, Point(0, s), Point(0, s + 1));
                int last = rings.Length - 1;
                Face(slot, origin + rotation * Vector3.up * rings[last].x, Point(last, s + 1), Point(last, s));
            }
        }

        public void Surface(int slot, int columns, int rows, Func<float, float, Vector3> point)
        {
            int first = vertices.Count;
            for (int z = 0; z <= rows; z++)
            {
                for (int x = 0; x <= columns; x++)
                {
                    Vector3 p = point(x / (float)columns, z / (float)rows);
                    vertices.Add(p);
                    uv.Add(new Vector2(p.x, p.z));
                }
            }

            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int a = first + z * (columns + 1) + x;
                    int b = a + columns + 1;
                    indices[slot].Add(a); indices[slot].Add(b); indices[slot].Add(a + 1);
                    indices[slot].Add(a + 1); indices[slot].Add(b); indices[slot].Add(b + 1);
                }
            }
        }

        public Mesh Build(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.subMeshCount = indices.Length;
            for (int i = 0; i < indices.Length; i++) mesh.SetTriangles(indices[i], i);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
