using System;
using UnityEngine;

namespace DesertRush.Editor
{
    public static partial class DesertAssetGenerator
    {
        private static GameObject Scenery(string name, string modelCategory, Action<DesertMesh> build, params Material[] materials)
        {
            var mesh = new DesertMesh(materials.Length);
            build(mesh);
            return SavePrefab(Model(name, modelCategory, mesh, materials), "Environment");
        }

        private static SegmentProfile[] Profiles()
        {
            var pillar = Scenery("Carved pillar", "Architecture", m => DesertGeometry.Pillar(m, Vector3.zero, 6.3f), stone, shade, turquoise);
            var arch = Scenery("Sunstone arch", "Architecture", DesertGeometry.Arch, stone, shade, turquoise);
            var wall = Scenery("Broken enclosure", "Architecture", m => DesertGeometry.Wall(m, 7, 7, 1.2f, .48f), stone, shade);
            var temple = Scenery("Buried temple", "Architecture", Temple, stone, shade, turquoise);
            var settlement = Scenery("Caravan shelter", "Architecture", Shelter, stone, shade, darkWood, trim);
            var palm = Scenery("Date palm", "Environment", m => DesertGeometry.Palm(m, 12), darkWood, wood, leaf, leafDark);
            var rock = Scenery("Sandstone outcrop", "Environment", m => DesertGeometry.Rock(m, 2.5f, 2.45f, 45, 11), stone, shade);
            var cliff = Scenery("Canyon escarpment", "Environment", m => DesertGeometry.Rock(m, 5f, 8.5f, 93, 10), shade, stone);
            var fossils = Scenery("Fossil ribs", "Environment", Fossils, Surface("Sun-bleached bone", 0xE4CC9F));
            return new[] {
                Profile("Sunstone road", 0, 9f, new[] { pillar, arch, wall, temple, palm, rock, settlement }),
                Profile("Buried caravanserai", 1, 9f, new[] { palm, palm, settlement, wall, rock, fossils, pillar }),
                Profile("Ember canyon", 2, 10f, new[] { cliff, cliff, fossils, rock, pillar, arch })
            };
        }

        private static SegmentProfile Profile(string name, int variant, float clearance, GameObject[] scenery)
        {
            const float width = 110f;
            float length = RoutePlanner.SegmentLength;
            var mesh = new DesertMesh(4);
            mesh.Surface(0, 58, 44, (u, v) =>
            {
                float x = (u - .5f) * width;
                float fade = Mathf.SmoothStep(0, 1, Mathf.Clamp01((Mathf.Abs(x) - 5.8f) / 17f));
                float end = Mathf.Sin(v * Mathf.PI);
                float ripple = Mathf.Sin(u * 26f + v * 12f + variant) * .5f + .5f;
                float rise = Mathf.PerlinNoise(u * 4f + variant * 3f, v * 2.4f);
                return V(x, -.13f + fade * end * (ripple * 2.7f + rise * 5.4f), v * length);
            });
            mesh.Surface(1, 8, 22, (u, v) => V((u - .5f) * 11.2f, -.045f, v * length));
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 23; i++)
                    mesh.Beam(2, V(s * (5.44f + Mathf.Sin(i * 3f) * .16f), .015f, 1.22f + 2.48f * i),
                        V(.36f, .18f, 1.63f + .28f * Mathf.Sin(i)), Quaternion.Euler(0, i * 17f % 14f, 0), .074f);
            }
            if (variant == 2)
            {
                for (int i = 0; i < 37; i++)
                    mesh.Beam(i % 3 == 0 ? 3 : 2, V(0, -.11f, 14f + .78f * i), V(10.1f, .26f, .74f));
            }
            var segment = Model(name, "Environment", mesh, sand, road, variant == 2 ? wood : stone, darkWood);
            Box(segment, V(0, -.22f, length * .5f), V(11f, .40f, length), false);
            var profile = ScriptableObject.CreateInstance<SegmentProfile>();
            profile.terrain = SavePrefab(segment, "Environment");
            profile.scenery = scenery;
            profile.sceneryClearance = clearance;
            profile.bridge = variant == 2;
            return Persist(profile, "ScriptableObjects/" + name + ".asset");
        }

        private static void Temple(DesertMesh m)
        {
            for (int i = 0; i < 5; i++)
                m.Beam(i % 2, V(0, .19f + i * .35f, 0), V(12f - i * .45f, .37f, 7.8f - i * .59f), .16f);
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 4; i++)
                    DesertGeometry.Pillar(m, V(s * (1.35f + i * 1.13f), 1.8f, -2.72f), 4.7f);
                m.Beam(0, V(s * 4.2f, 3.87f, .66f), V(2.6f, 4.3f, 6.4f), .27f);
            }
            for (int i = 0; i < 4; i++)
                m.Beam(i == 1 ? 2 : i % 2, V(0, 6.54f + i * .35f, -.23f), V(11.05f - i * .49f, .34f, 6.25f - i * .46f), .15f);
            for (int i = -9; i <= 9; i++)
                m.Beam(0, V(i * .49f, 6.87f, -3.15f), V(.18f, .21f, .075f), Quaternion.Euler(0, 0, 45));
        }

        private static void Shelter(DesertMesh m)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    m.Beam(i % 2, V(s * 2.4f, .25f + i * .5f, 0), V(.8f, .48f, 5.3f), .09f);
                    m.Beam(i % 2, V(0, .25f + i * .5f, 2.47f), V(4.7f, .48f, .7f), .09f);
                }
                m.Beam(2, V(s * 2.16f, 1.34f, -2.7f), V(.19f, 2.68f, .21f));
            }
            for (int i = 0; i < 12; i++)
                m.Beam(i % 2 + 2, V(-2.42f + i * .44f, 2.66f, -.15f), V(.19f, .18f, 5.72f));
        }

        private static void Fossils(DesertMesh m)
        {
            for (int rib = 0; rib < 7; rib++)
            {
                float z = (rib - 3) * .43f;
                for (int i = 0; i < 10; i++)
                {
                    float a = Mathf.PI * i / 10;
                    float b = Mathf.PI * (i + 1) / 10;
                    var p = V(Mathf.Cos(a) * 1.21f, Mathf.Sin(a) * .95f - .11f, z);
                    var q = V(Mathf.Cos(b) * 1.21f, Mathf.Sin(b) * .95f - .11f, z);
                    m.Beam(0, (p + q) / 2, V(.13f, .15f, Vector3.Distance(p, q) + .04f),
                        Quaternion.LookRotation(q - p), .045f);
                }
            }
        }

        private static GameObject Hazard(string name, Action<DesertMesh> build, Vector3 center, Vector3 size, params Material[] materials)
        {
            var mesh = new DesertMesh(materials.Length);
            build(mesh);
            var instance = Model(name, "Obstacles", mesh, materials);
            Box(instance, center, size, true);
            return SavePrefab(instance, "Obstacles");
        }

        private static GameObject[] Obstacles()
        {
            var result = new GameObject[14];
            result[0] = Hazard("Freight crate", m => DesertGeometry.Crate(m, 1.78f, 1.74f, 1.55f), V(0,.91f,0), V(2,1.82f,1.78f), wood,darkWood,trim,metal);
            result[1] = Hazard("Supply crate", m => DesertGeometry.Crate(m, 1.4f, .71f, .9f), V(0,.38f,0), V(1.62f,.76f,1.12f), wood,darkWood,trim,metal);
            result[2] = Hazard("Cut stone blocks", m => DesertGeometry.Wall(m, 2, 2, .78f, .47f), V(0,.47f,0), V(1.87f,.94f,.78f), stone,shade);
            result[3] = Hazard("Falling rubble", m => DesertGeometry.Rock(m,.86f,1.45f,21,8), V(0,.74f,0), V(1.66f,1.54f,1.53f), stone,shade);
            result[4] = Hazard("Rolling boulder", m => m.Lathe(0, V(0,.83f,0),Quaternion.identity,11,
                Ring(-.83f,.16f,.16f),Ring(-.51f,.66f,.67f),Ring(-.12f,.85f,.79f),Ring(.41f,.72f,.63f),Ring(.84f,.16f,.17f)),
                V(0,.83f,0), V(1.63f,1.66f,1.55f), shade);
            result[5] = Hazard("Abandoned ore cart", Cart, V(0,.65f,0), V(1.86f,1.3f,1.6f), wood,darkWood,metal);
            result[6] = Hazard("Splintered barrier", m => Barrier(m,false), V(0,.43f,0), V(1.91f,.86f,.75f), wood,darkWood,trim);
            result[7] = Hazard("Hanging timber", m => Barrier(m,true), V(0,1.29f,0), V(1.93f,.72f,.85f), wood,darkWood,trim);
            result[8] = Hazard("Low parapet", m => DesertGeometry.Wall(m, 2, 3, .53f, .35f), V(0,.35f,0), V(1.90f,.7f,.76f), stone,shade);
            result[9] = Hazard("Bronze spike trap", Spikes, V(0,.41f,0), V(1.85f,.82f,1.36f), stone,metal,gold);
            result[10] = Hazard("Sand sink", Pit, V(0,.23f,0), V(1.75f,.46f,1.9f), shade,darkWood);
            result[11] = Hazard("Broken bridge deck", m => Barrier(m,false), V(0,.4f,0), V(1.9f,.8f,.75f), darkWood,wood,trim);
            result[12] = Hazard("Swinging gate", m => Barrier(m,true), V(0,1.3f,0), V(1.83f,.70f,.85f), darkWood,wood,metal);
            result[13] = Hazard("Temple drop gate", Gate, V(0,1.48f,0), V(1.88f,1.12f,.84f), stone,shade,turquoise);
            return result;
        }

        private static void Cart(DesertMesh m)
        {
            m.Beam(1,V(0,.53f,0),V(1.56f,.19f,1.41f));
            for (int s=-1;s<=1;s+=2)
            {
                m.Beam(0,V(s*.63f,.91f,0),V(.18f,.66f,1.52f), Quaternion.Euler(0,0,s*-12f));
                m.Beam(0,V(0,.90f,s*.68f),V(1.43f,.58f,.18f));
                for (int z=-1;z<=1;z+=2)
                    m.Lathe(2,V(s*.81f,.39f,z*.47f),Quaternion.Euler(0,0,90),12,
                        Ring(-.09f,.31f,.31f),Ring(0,.38f,.38f),Ring(.09f,.31f,.31f));
            }
        }

        private static void Barrier(DesertMesh m, bool hanging)
        {
            float y = hanging ? 1.25f : .49f;
            for (int i = 0; i < 4; i++)
                m.Beam(i % 2, V(0,y+(i-1.5f)*.14f,0),V(1.84f,.127f,.40f),Quaternion.Euler(0,0,(i-1.5f)*4f));
            for (int s = -1; s <= 1; s += 2)
            {
                m.Beam(1, V(s * 1.06f, y * .58f, 0), V(.16f, y * 1.16f, .19f), Quaternion.Euler(0,0,s*-9f));
                m.Beam(2, V(s * .57f, y, -.225f), V(.15f,.68f,.10f));
                if (hanging) m.Beam(2, V(s*.64f, 1.89f, 0), V(.058f,.87f,.058f));
            }
        }

        private static void Spikes(DesertMesh m)
        {
            m.Beam(0,V(0,.075f,0),V(1.88f,.15f,1.38f));
            for (int x=-2;x<=2;x++)
            for (int z=-1;z<=1;z++)
                m.Lathe((x+z)%2==0?1:2,V(x*.35f,0,z*.42f),Quaternion.identity,5,
                    Ring(.13f,.15f,.15f),Ring(.37f,.14f,.14f),Ring(.76f,.014f,.014f));
        }

        private static void Pit(DesertMesh m)
        {
            m.Lathe(0,Vector3.zero,Quaternion.identity,20,Ring(-.024f,.98f,1.07f),Ring(.044f,.93f,1.04f));
            m.Lathe(1,Vector3.zero,Quaternion.identity,16,Ring(.044f,.63f,.72f),Ring(.05f,.57f,.65f));
            for (int i=0;i<9;i++)
                m.Beam(0,V(Mathf.Sin(i*2f)*.80f,.075f,Mathf.Cos(i*2f)*.91f),V(.19f,.12f,.13f),Quaternion.Euler(0,i*39f,0));
        }

        private static void Gate(DesertMesh m)
        {
            for (int s=-1;s<=1;s+=2)
                m.Beam(1,V(s*1.05f,1.05f,0),V(.21f,2.1f,.62f));
            for(int i=0;i<3;i++)
                m.Beam(i==1?2:0,V(0,1.05f+i*.35f,0),V(1.95f,.32f,.83f),.085f);
        }

        private static GameObject[] Pickups()
        {
            var result = new GameObject[8];
            var colors = new[] { gold, gold, Surface("Lapis crystal", 0x27B7D9, null, .34f, .55f),
                Surface("Magnetic red", 0xE75443), Surface("Overdrive amber", 0xFF832C),
                Surface("Ward teal", 0x49DBB1), Surface("Multiplier violet", 0xC178ED), Surface("Glider pearl", 0xE9DD9A) };
            var names = new[] { "Sun coin", "Royal idol", "Lapis gemstone", "Magnet", "Overdrive", "Ward shield", "Double gold", "Glider beacon" };
            for (int i=0;i<result.Length;i++)
            {
                var m = new DesertMesh(2);
                if (i == 0 || i == 6)
                {
                    var rotation = Quaternion.Euler(90,0,0);
                    m.Lathe(0,Vector3.zero,rotation,24,Ring(-.065f,.26f,.26f),Ring(-.04f,.30f,.30f),
                        Ring(.04f,.30f,.30f),Ring(.065f,.26f,.26f));
                    for(int s=-1;s<=1;s+=2)
                        m.Beam(1,V(0,0,s*.067f),V(.26f,.26f,.028f),Quaternion.Euler(0,0,45));
                }
                else if (i == 3)
                {
                    for(int s=-1;s<=1;s+=2)
                    {
                        m.Beam(0,V(s*.22f,.10f,0),V(.21f,.58f,.20f),.053f);
                        m.Beam(1,V(s*.22f,.34f,0),V(.22f,.16f,.21f));
                    }
                    m.Beam(0,V(0,-.21f,0),V(.64f,.27f,.20f),.086f);
                }
                else if (i == 1)
                {
                    m.Lathe(0,Vector3.zero,Quaternion.identity,9,Ring(-.39f,.28f,.21f),Ring(-.24f,.27f,.21f),
                        Ring(.09f,.15f,.13f),Ring(.28f,.21f,.20f),Ring(.43f,.11f,.10f));
                    m.Beam(1,V(0,.18f,-.177f),V(.21f,.13f,.10f));
                }
                else
                {
                    m.Lathe(0,Vector3.zero,Quaternion.identity, i == 5 ? 6 : 5,Ring(-.43f,.025f,.025f),
                        Ring(-.11f,.29f,.22f),Ring(.19f,.32f,.22f),Ring(.42f,.13f,.13f));
                    m.Lathe(1,Vector3.zero,Quaternion.identity,10,Ring(-.055f,.34f,.26f),Ring(.055f,.34f,.26f));
                }
                var instance = Model(names[i],"Collectibles",m,colors[i],i==0?trim:gold);
                var trigger = instance.AddComponent<SphereCollider>();
                trigger.radius = .52f;
                trigger.isTrigger = true;
                result[i] = SavePrefab(instance, i<3 ? "Collectibles" : "PowerUps");
            }
            return result;
        }

        private static GameObject Glider()
        {
            var m = new DesertMesh(3);
            for (int s=-1;s<=1;s+=2)
            {
                m.Beam(0,V(s*.41f,0,0),V(.72f,.10f,1.6f),Quaternion.Euler(0,s*16f,0),.16f);
                m.Beam(1,V(s*.68f,.055f,-.04f),V(.14f,.11f,1.53f),Quaternion.Euler(0,s*16f,0),.05f);
                m.Beam(2,V(s*.44f,-.09f,-.73f),V(.27f,.14f,.48f),.045f);
            }
            return SavePrefab(Model("Dune skimmer","Collectibles",m,turquoise,gold,metal),"PowerUps");
        }
    }
}
