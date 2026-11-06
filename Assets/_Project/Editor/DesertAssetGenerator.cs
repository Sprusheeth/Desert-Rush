using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DesertRush.Editor
{
    public static partial class DesertAssetGenerator
    {
        private const string Root = "Assets/_Project/";
        private const string MenuScene = Root + "Scenes/MainMenu.unity";
        private const string GameScene = Root + "Scenes/Gameplay.unity";
        private static Material sand, road, stone, shade, turquoise, wood, darkWood, trim, metal, gold, leaf, leafDark;

        [MenuItem("Tools/Desert Rush/Asset Generator")]
        public static void GenerateFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Generate();
        }

        public static void Generate()
        {
            foreach (string folder in new[] { "Scenes", "Models/Characters", "Models/Environment", "Models/Architecture",
                "Models/Collectibles", "Models/Obstacles", "Materials", "Textures", "Audio", "Settings", "ScriptableObjects",
                "Prefabs/Player", "Prefabs/Environment", "Prefabs/Obstacles", "Prefabs/Collectibles", "Prefabs/PowerUps", "Prefabs/UI" })
                Directory.CreateDirectory(Root + folder);
            AssetDatabase.Refresh();

            var texture = Grain("Sand grain", false);
            var woodGrain = Grain("Acacia grain", true);
            sand = Surface("Dune sand", 0xD89B5F, texture);
            road = Surface("Packed trail", 0xB57D52, texture);
            stone = Surface("Sunstone", 0xD6A073, texture);
            shade = Surface("Weathered stone", 0xA36445, texture);
            turquoise = Surface("Copper patina", 0x397978, texture, .35f);
            wood = Surface("Acacia", 0xA06238, woodGrain);
            darkWood = Surface("Smoked acacia", 0x71442E, woodGrain);
            trim = Surface("Carved hardwood", 0xCE8F4C, woodGrain);
            metal = Surface("Iron", 0x383D40, null, .6f);
            gold = Surface("Hammered gold", 0xFFAE27, null, .72f, .42f);
            leaf = Surface("Palm green", 0x577749);
            leafDark = Surface("Palm shadow", 0x344F3F);

            var catalog = ScriptableObject.CreateInstance<ExpeditionCatalog>();
            catalog.obstacles = Obstacles();
            catalog.pickups = Pickups();
            catalog.profiles = Profiles();
            catalog.explorer = Explorer(out catalog.outfits);
            catalog.glider = Glider();
            catalog.sounds = AudioAssets();
            catalog = Persist(catalog, "ScriptableObjects/Expedition Catalog.asset");
            var uiObject = new GameObject("Expedition interface", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(ExpeditionUI));
            var ui = SavePrefab(uiObject, "UI").GetComponent<ExpeditionUI>();
            var particles = new Material(RequiredShader("Universal Render Pipeline/Particles/Unlit"));
            particles.SetFloat("_Surface", 1);
            particles.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            particles.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            particles.SetFloat("_ZWrite", 0);
            particles.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            particles.renderQueue = (int)RenderQueue.Transparent;
            particles = Persist(particles, "Materials/Wind and glints.mat");
            var sky = new Material(RequiredShader("Skybox/Procedural"));
            sky.SetColor("_SkyTint", ColorHex(0xD49B73));
            sky.SetColor("_GroundColor", ColorHex(0xB78158));
            sky.SetFloat("_AtmosphereThickness", .89f);
            sky = Persist(sky, "Materials/Desert sky.mat");

            SceneFor(MenuScene, false, catalog, ui, particles, sky);
            SceneFor(GameScene, true, catalog, ui, particles, sky);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MenuScene, true),
                new EditorBuildSettingsScene(GameScene, true) };
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MenuScene);
            Debug.Log("Desert Rush generated both scenes and the expedition asset library.");
        }

        private static void SceneFor(string path, bool gameplay, ExpeditionCatalog catalog, ExpeditionUI ui, Material particles, Material sky)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Expedition camera", typeof(Camera), typeof(AudioListener), typeof(DuneCamera));
            camera.tag = "MainCamera";
            camera.transform.SetPositionAndRotation(new Vector3(0, 3.3f, -7), Quaternion.Euler(5f, 0, 0));
            camera.GetComponent<Camera>().farClipPlane = 230f;
            var sun = new GameObject("Late afternoon sun", typeof(Light)).GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = ColorHex(0xFFD7AC);
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(25, -129, 0);
            RenderSettings.skybox = sky;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ColorHex(0xACAAAC);
            RenderSettings.ambientEquatorColor = ColorHex(0x99827F);
            RenderSettings.ambientGroundColor = ColorHex(0x60504C);
            var root = new GameObject("Expedition systems");
            var atmosphere = root.AddComponent<DesertAtmosphere>();
            atmosphere.sun = sun;
            atmosphere.particles = particles;
            var game = root.AddComponent<Expedition>();
            game.atmosphere = atmosphere;
            game.catalog = catalog;
            game.chase = camera.GetComponent<DuneCamera>();
            game.interfacePrefab = ui;
            game.startImmediately = gameplay;
            if (!EditorSceneManager.SaveScene(scene, path))
                throw new IOException("Unable to save " + path);
        }

        private static Color ColorHex(int hex) =>
            new Color32((byte)(hex >> 16), (byte)(hex >> 8), (byte)hex, 255);

        private static Shader RequiredShader(string name) => Shader.Find(name)
            ?? throw new InvalidOperationException("Missing shader: " + name);

        private static Material Surface(string name, int color, Texture2D texture = null, float metallic = 0, float smoothness = .19f)
        {
            var material = new Material(RequiredShader("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", ColorHex(color));
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (texture != null) material.SetTexture("_BaseMap", texture);
            return Persist(material, "Materials/" + name + ".mat");
        }

        private static Texture2D Grain(string name, bool woodgrain)
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float a = x * Mathf.PI * 2 / size;
                    float b = y * Mathf.PI * 2 / size;
                    float bands = woodgrain ? Mathf.Sin(a * 15 + Mathf.Sin(b * 2 + a) * 1.8f) : Mathf.Sin(a * 3 + b * 9 + Mathf.Cos(a * 3));
                    uint noise = unchecked((uint)(x + y * size) * 2654435761u);
                    byte shade = (byte)(221 + bands * (woodgrain ? 21 : 9) + (noise >> 28));
                    pixels[y * size + x] = new Color32(shade, shade, shade, 255);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(true);
            return Persist(texture, "Textures/" + name + ".asset");
        }

        private static T Persist<T>(T value, string relative) where T : UnityEngine.Object
        {
            string path = Root + relative;
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(value, path);
                return value;
            }
            EditorUtility.CopySerialized(value, existing);
            EditorUtility.SetDirty(existing);
            UnityEngine.Object.DestroyImmediate(value);
            return existing;
        }

        private static GameObject SavePrefab(GameObject instance, string category)
        {
            var asset = PrefabUtility.SaveAsPrefabAsset(instance, Root + "Prefabs/" + category + "/" + instance.name + ".prefab");
            UnityEngine.Object.DestroyImmediate(instance);
            return asset ?? throw new InvalidOperationException("Prefab could not be saved.");
        }

        private static GameObject Model(string name, string category, DesertMesh geometry, params Material[] materials)
        {
            var asset = Persist(geometry.Build(name), "Models/" + category + "/" + name + ".asset");
            var instance = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            instance.GetComponent<MeshFilter>().sharedMesh = asset;
            instance.GetComponent<MeshRenderer>().sharedMaterials = materials;
            return instance;
        }

        private static void Box(GameObject instance, Vector3 center, Vector3 size, bool trigger)
        {
            var collider = instance.AddComponent<BoxCollider>();
            collider.center = center;
            collider.size = size;
            collider.isTrigger = trigger;
        }

        private static Vector3 V(float x, float y, float z) => new(x, y, z);
        private static Vector3 Ring(float height, float x, float z) => V(height, x, z);
    }
}
