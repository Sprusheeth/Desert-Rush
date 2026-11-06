using UnityEngine;

namespace DesertRush
{
    public sealed class DesertAtmosphere : MonoBehaviour
    {
        public Light sun;
        public Material particles;
        private ParticleSystem sand, dust, sparks;
        private static readonly Color Clear = new(.65f, .49f, .36f);
        private static readonly Color Storm = new(.70f, .40f, .22f);

        public void Bind(Transform player, Transform cameraRig)
        {
            sand = Create("Crosswind", cameraRig, new Vector3(-5, 1, 15), new Vector3(23, 6, 32),
                new Color(1f, .78f, .5f, .52f), .055f, 260);
            dust = Create("Footprints in motion", player, new Vector3(0, .12f, -.33f), new Vector3(.7f, .08f, .4f),
                new Color(.88f, .60f, .35f, .55f), .19f, 110);
            sparks = Create("Pickup glints", transform, Vector3.zero, Vector3.one * .2f,
                new Color(1f, .83f, .37f), .1f, 130);
            var velocity = sand.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = 8f;
            velocity.y = .15f;
            velocity.z = -6f;
        }

        public float Tick(float runTime, float time, bool running, bool grounded, bool boost)
        {
            float cycle = Mathf.Repeat(runTime, 73f);
            float storm = running ? Mathf.SmoothStep(0, 1, Mathf.Clamp01((cycle - 49f) / 5f))
                * Mathf.SmoothStep(0, 1, Mathf.Clamp01((69f - cycle) / 5f)) : 0f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = Mathf.Lerp(.009f, .027f, storm);
            RenderSettings.fogColor = Color.Lerp(Clear, Storm, storm);
            sun.intensity = Mathf.Lerp(1.56f, .68f, storm);
            sun.transform.rotation = Quaternion.Euler(24f + Mathf.Sin(time * .025f) * 4f, -129f, 0f);
            var emission = sand.emission;
            emission.rateOverTime = 22f + storm * 128f;
            emission = dust.emission;
            emission.rateOverTime = running && grounded ? boost ? 88f : 36f : 0f;
            return storm;
        }

        public void Burst(Vector3 position, bool impact)
        {
            sparks.transform.position = position;
            var main = sparks.main;
            main.startColor = impact ? new Color(1f, .36f, .14f) : new Color(1f, .87f, .36f);
            sparks.Emit(impact ? 36 : 20);
        }

        public void ClearTrails()
        {
            sand.Clear();
            dust.Clear();
            sparks.Clear();
        }

        private ParticleSystem Create(string label, Transform parent, Vector3 offset, Vector3 size,
            Color color, float scale, int cap)
        {
            var holder = new GameObject(label);
            holder.transform.SetParent(parent, false);
            holder.transform.localPosition = offset;
            var system = holder.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = color;
            main.startSize = new ParticleSystem.MinMaxCurve(scale * .4f, scale);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.15f, 2f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(.22f, 1.1f);
            main.maxParticles = cap;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            var emission = system.emission;
            emission.rateOverTime = 0f;
            holder.GetComponent<ParticleSystemRenderer>().sharedMaterial = particles;
            system.Play();
            return system;
        }
    }
}
