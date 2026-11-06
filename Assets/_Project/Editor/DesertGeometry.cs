using UnityEngine;

namespace DesertRush.Editor
{
    internal static class DesertGeometry
    {
        private static Vector3 V(float x, float y, float z) => new(x, y, z);
        private static Vector3 Ring(float height, float radius) => V(height, radius, radius);

        public static void Crate(DesertMesh mesh, float width, float height, float depth)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    mesh.Beam(i % 2, V((i - 2) * width / 5f, height / 2, s * depth / 2),
                        V(width / 5f - .024f, height, .09f));
                    mesh.Beam((i + 1) % 2, V(s * width / 2, height / 2, (i - 2) * depth / 5f),
                        V(.09f, height, depth / 5f - .024f));
                    if (s > 0) mesh.Beam(i % 2, V((i - 2) * width / 5f, height, 0),
                        V(width / 5f - .024f, .09f, depth));
                }
                foreach (float y in new[] { .12f, height - .12f })
                {
                    mesh.Beam(2, V(0, y, s * (depth / 2 + .055f)), V(width + .18f, .16f, .11f));
                    mesh.Beam(2, V(s * (width / 2 + .055f), y, 0), V(.11f, .16f, depth + .18f));
                }
                mesh.Beam(2, V(0, height / 2, s * (depth / 2 + .055f)), V(.17f, height * 1.21f, .09f),
                    Quaternion.Euler(0, 0, s * 37f));
                for (int x = -1; x <= 1; x += 2)
                    mesh.Beam(3, V(x * width * .41f, height - .12f, s * (depth * .5f + .118f)),
                        V(.075f, .075f, .035f));
            }
        }

        public static void Pillar(DesertMesh mesh, Vector3 pivot, float h)
        {
            mesh.Beam(1, pivot + V(0, .21f, 0), V(1.54f, .42f, 1.54f), .15f);
            mesh.Beam(0, pivot + V(0, .52f, 0), V(1.22f, .27f, 1.22f), .12f);
            for (int i = 0; i < 7; i++)
            {
                float y = .64f + (h - 1.39f) * i / 7f;
                float top = y + (h - 1.39f) / 7f;
                float radius = .57f - i * .016f;
                mesh.Lathe(i == 1 || i == 5 ? 1 : 0, pivot, Quaternion.identity, 10,
                    Ring(y, radius - .04f), Ring(y + .045f, radius),
                    Ring(top - .055f, radius - .016f), Ring(top - .012f, radius - .05f));
            }
            mesh.Lathe(2, pivot, Quaternion.identity, 10, Ring(h - .79f, .52f), Ring(h - .54f, .77f));
            mesh.Beam(0, pivot + V(0, h - .39f, 0), V(1.55f, .32f, 1.55f), .15f);
            mesh.Beam(1, pivot + V(0, h - .12f, 0), V(1.72f, .24f, 1.72f), .13f);
        }

        public static void Arch(DesertMesh mesh)
        {
            Pillar(mesh, V(-5.9f, 0, 0), 3.5f);
            Pillar(mesh, V(5.9f, 0, 0), 3.5f);
            Vector3 W(float radius, float a, float z) => V(radius * Mathf.Cos(a), 3.35f + radius * Mathf.Sin(a), z);
            for (int i = 0; i < 19; i++)
            {
                float a = Mathf.PI * i / 19 + .008f;
                float b = Mathf.PI * (i + 1) / 19 - .008f;
                int slot = i == 9 ? 2 : i % 3 == 0 ? 1 : 0;
                var p = W(5.16f, a, - .74f); var q = W(6.61f, a, - .74f);
                var r = W(6.61f, b, - .74f); var s = W(5.16f, b, - .74f);
                var depth = Vector3.forward * 1.48f;
                mesh.Face(slot, s, r, q, p);
                mesh.Face(slot, p + depth, q + depth, r + depth, s + depth);
                mesh.Face(1, p, q, q + depth, p + depth);
                mesh.Face(1, r, s, s + depth, r + depth);
                mesh.Face(slot, q, r, r + depth, q + depth);
                mesh.Face(1, s, p, p + depth, s + depth);
            }
        }

        public static void Wall(DesertMesh mesh, int rows, int columns, float brick, float height)
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (rows > 3 && y > 2 && (x + y) % 4 == 0) continue;
                    float stagger = y % 2 == 0 ? -.15f : .15f;
                    mesh.Beam((x + 2 * y) % 2, V((x - (columns - 1) / 2f) * brick + stagger,
                        (y + .5f) * height, Mathf.Sin(x + y * 8f) * .055f),
                        V(brick - .029f, height - .025f, .76f), .065f);
                }
            }
        }

        public static void Rock(DesertMesh mesh, float width, float height, int seed, int sides)
        {
            var random = new System.Random(seed);
            Vector3[,] rings = new Vector3[4, sides];
            for (int y = 0; y < 4; y++)
            {
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2f / sides;
                    float r = width * (1f - y * .2f) * (.72f + .37f * (float)random.NextDouble());
                    rings[y, i] = V(Mathf.Cos(a) * r + y * width * .07f,
                        (y / 3f + .07f * (float)random.NextDouble()) * height - .04f, Mathf.Sin(a) * r * .84f);
                }
            }

            for (int y = 0; y < 3; y++)
            {
                for (int i = 0; i < sides; i++)
                {
                    int j = (i + 1) % sides;
                    mesh.Face((i + y) % 3 == 0 ? 1 : 0, rings[y, i], rings[y + 1, i], rings[y + 1, j]);
                    mesh.Face(i % 3 == 0 ? 1 : 0, rings[y, i], rings[y + 1, j], rings[y, j]);
                    if (y == 2) mesh.Face(0, rings[3, j], V(.2f * width, height * 1.11f, 0), rings[3, i]);
                }
            }
        }

        public static void Palm(DesertMesh mesh, int count)
        {
            for (int i = 0; i < 13; i++)
            {
                float y = i * .37f;
                float r = .26f - i * .008f;
                mesh.Lathe(i % 3 == 0 ? 1 : 0, V(i * i * .0035f, y, 0), Quaternion.Euler(0, 0, -i),
                    8, Ring(0, r), Ring(.30f, r * .79f), Ring(.42f, r * .72f));
            }

            var crown = V(.53f, 4.72f, 0);
            for (int i = 0; i < count; i++)
            {
                float a = i * 2.39996f;
                var d = V(Mathf.Cos(a), 0, Mathf.Sin(a));
                var cross = V(-d.z, 0, d.x);
                float length = 1.8f + i % 4 * .27f;
                Vector3 Point(float t) => crown + d * t * length + V(0, Mathf.Sin(t * 2.5f) * .94f - t * t * .87f, 0);
                for (int j = 0; j < 8; j++)
                {
                    float t = j / 8f;
                    float w = Mathf.Sin(Mathf.PI * (t + .09f)) * .39f;
                    var p = Point(t);
                    var q = Point(t + .125f);
                    mesh.Face(2 + j % 2, p, p + cross * w - Vector3.up * .11f, q);
                    mesh.Face(3, q, p - cross * w - Vector3.up * .11f, p);
                    mesh.Face(3, q, p + cross * w - Vector3.up * .11f, p);
                    mesh.Face(2, p, p - cross * w - Vector3.up * .11f, q);
                }
            }
        }
    }
}
