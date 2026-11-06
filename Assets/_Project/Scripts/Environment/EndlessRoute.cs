using System.Collections.Generic;
using UnityEngine;

namespace DesertRush
{
    public sealed class EndlessRoute
    {
        private struct Prop
        {
            public GameObject Prefab, Instance;
            public Vector3 Home;
            public int Kind;
            public float Phase;
            public bool Taken;
        }

        private sealed class Segment
        {
            public Transform Root;
            public readonly List<Prop> Props = new(80);
        }

        private readonly Queue<Segment> active = new();
        private readonly Stack<Segment> available = new();
        private readonly PrefabPool pool;
        private readonly Transform root;
        private readonly ExpeditionCatalog catalog;
        private System.Random random;
        private int index, safeLane;
        private float nextZ;

        public EndlessRoute(Transform root, ExpeditionCatalog catalog)
        {
            this.root = root;
            this.catalog = catalog;
            pool = new PrefabPool(root);
        }

        public void Reset(int seed)
        {
            while (active.Count > 0) Retire();
            random = new System.Random(seed);
            index = safeLane = 0;
            nextZ = -RoutePlanner.RowSpacing;
            Stream(0f);
        }

        public void Stream(float playerZ)
        {
            while (nextZ < playerZ + 178f) Spawn();
            while (active.Peek().Root.localPosition.z + RoutePlanner.SegmentLength < playerZ - 29f)
                Retire();
        }

        public void Rebase(float offset)
        {
            nextZ -= offset;
            foreach (var segment in active)
                segment.Root.localPosition -= Vector3.forward * offset;
        }

        public void Interact(Expedition run, Vector3 from, Vector3 to, float dt)
        {
            foreach (var segment in active)
            {
                float localZ = to.z - segment.Root.localPosition.z;
                if (localZ < -20f || localZ > RoutePlanner.SegmentLength + 20f) continue;
                var props = segment.Props;
                for (int i = 0; i < props.Count; i++)
                {
                    var prop = props[i];
                    if (prop.Kind == 0 || prop.Taken) continue;
                    var t = prop.Instance.transform;
                    float delta = t.position.z - to.z;
                    if (delta < -11f || delta > 64f) continue;
                    if (prop.Kind > 0)
                    {
                        var kind = (PickupKind)(prop.Kind - 1);
                        Vector3 target = to + Vector3.up * .86f;
                        if (kind <= PickupKind.Gem && run.Effects.Active(PickupKind.Magnet)
                            && (t.position - target).sqrMagnitude < 90f)
                            t.position = Vector3.MoveTowards(t.position, target, dt * 39f);
                        else
                            t.localPosition = prop.Home + Vector3.up * (.13f * Mathf.Sin(run.Clock * 4f + prop.Phase));
                        t.localRotation = Quaternion.Euler(0f, run.Clock * 108f + prop.Phase * 70f, 0f);
                        Vector3 center = t.position;
                        float cross = Crossing(from, to, center.z);
                        Vector3 runner = Vector3.Lerp(from, to, cross);
                        float y = run.Motor.Sliding ? .42f : .92f;
                        if (run.Effects.Active(PickupKind.Glider)) y = center.y - runner.y;
                        if ((runner + Vector3.up * y - center).sqrMagnitude > .93f) continue;
                        run.Collect(kind, center);
                    }
                    else
                    {
                        int hazard = -prop.Kind - 1;
                        var box = prop.Instance.GetComponent<BoxCollider>();
                        if (hazard == 3 || hazard == 4 || hazard == 12)
                        {
                            float wave = Mathf.Sin(run.Clock * 3f + prop.Phase);
                            t.localPosition = prop.Home + (hazard == 3
                                ? Vector3.up * (1.5f + wave * 1.5f)
                                : Vector3.right * wave * .42f);
                            if (hazard == 4)
                                t.localRotation = Quaternion.Euler(run.Clock * 175f, 0f, 0f);
                        }

                        Vector3 center = t.position + box.center;
                        Vector3 half = box.size * .5f;
                        float reach = half.z + .36f;
                        if (center.z + reach < from.z || center.z - reach > to.z) continue;
                        Vector3 runner = Vector3.Lerp(from, to, Crossing(from, to, center.z));
                        if (Mathf.Abs(runner.x - center.x) >= half.x + .32f) continue;
                        float height = run.Motor.Sliding ? RunnerMotor.SlideHeight : RunnerMotor.NormalHeight;
                        if (runner.y >= center.y + half.y || runner.y + height <= center.y - half.y) continue;
                        if (!run.Hit(center)) return;
                    }

                    prop.Taken = true;
                    props[i] = prop;
                    prop.Instance.SetActive(false);
                }
            }
        }

        private static float Crossing(Vector3 from, Vector3 to, float z) =>
            Mathf.InverseLerp(from.z, to.z, z);

        private void Spawn()
        {
            Segment segment;
            if (available.Count == 0)
            {
                segment = new Segment { Root = new GameObject("Route segment").transform };
                segment.Root.SetParent(root, false);
            }
            else segment = available.Pop();

            segment.Root.localPosition = Vector3.forward * nextZ;
            segment.Root.gameObject.SetActive(true);
            var profile = catalog.profiles[index == 0 ? 0 : random.Next(catalog.profiles.Length)];
            Add(segment, profile.terrain, Vector3.zero, Quaternion.identity);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 9; i++)
                {
                    float z = RoutePlanner.SegmentLength * (i + Range(.1f, .95f)) / 9f;
                    float x = side * (profile.sceneryClearance + Range(0f, 32f));
                    Add(segment, profile.scenery[random.Next(profile.scenery.Length)], new Vector3(x, 0, z),
                        Quaternion.Euler(0, Range(0, 360), 0));
                }
            }

            for (int row = 0; row < 3; row++)
            {
                float z = RoutePlanner.RowSpacing * (row + .62f);
                int oldLane = safeLane;
                safeLane = index == 0 ? 0 : RoutePlanner.NextLane(random, safeLane);
                if (index > 0 || row == 2)
                {
                    int hazard = random.Next(index < 3 ? 5 : catalog.obstacles.Length);
                    int alternate = safeLane == 0 ? (random.Next(2) == 0 ? -1 : 1) : 0;
                    Add(segment, catalog.obstacles[hazard], Lane(alternate, 0, z),
                        Quaternion.identity, -1 - hazard);
                    if (index >= 3 && random.NextDouble() < System.Math.Min(.82, index * .035))
                    {
                        int other = -safeLane - alternate;
                        hazard = random.Next(catalog.obstacles.Length);
                        Add(segment, catalog.obstacles[hazard], Lane(other, 0, z), Quaternion.identity, -1 - hazard);
                    }
                }

                for (int c = 0; c < 5; c++)
                {
                    float lane = Mathf.Lerp(oldLane, safeLane, Mathf.Clamp01(c / 3f));
                    Add(segment, catalog.pickups[0], Lane(lane, 1.0f, z - 11f + c * 1.9f),
                        Quaternion.identity, 1);
                }

                if ((index * 3 + row) % 4 == 3)
                {
                    int kind = 1 + random.Next(catalog.pickups.Length - 1);
                    Add(segment, catalog.pickups[kind], Lane(safeLane, 1.15f, z + 1.8f),
                        Quaternion.identity, kind + 1);
                }
            }
            active.Enqueue(segment);
            index++;
            nextZ += RoutePlanner.SegmentLength;
        }

        private static Vector3 Lane(float lane, float y, float z) => new(lane * RunnerMotor.LaneWidth, y, z);
        private float Range(float low, float high) => Mathf.Lerp(low, high, (float)random.NextDouble());

        private void Add(Segment segment, GameObject prefab, Vector3 position, Quaternion rotation, int kind = 0)
        {
            var instance = pool.Rent(prefab, segment.Root, position, rotation);
            segment.Props.Add(new Prop
            {
                Prefab = prefab, Instance = instance, Home = position, Kind = kind, Phase = Range(0f, 6.28f)
            });
        }

        private void Retire()
        {
            var segment = active.Dequeue();
            foreach (var prop in segment.Props) pool.Return(prop.Prefab, prop.Instance);
            segment.Props.Clear();
            segment.Root.gameObject.SetActive(false);
            available.Push(segment);
        }
    }
}
