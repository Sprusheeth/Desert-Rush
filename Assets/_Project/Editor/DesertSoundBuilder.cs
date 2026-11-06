using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DesertRush.Editor
{
    public static partial class DesertAssetGenerator
    {
        private static AudioClip[] AudioAssets()
        {
            string[] names = { "Sand footsteps", "Jump", "Slide", "Sun coin", "Relic power",
                "Impact", "Expedition over", "Caravan pulse", "Dune wind", "Sandstorm" };
            float[] durations = { .15f, .25f, .48f, .23f, .51f, .28f, 1.2f, 16f, 8f, 8f };
            for (int i = 0; i < names.Length; i++) WriteWave(i, durations[i], Root + "Audio/" + names[i] + ".wav");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var clips = new AudioClip[names.Length];
            for (int i = 0; i < clips.Length; i++)
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/" + names[i] + ".wav")
                    ?? throw new IOException("Audio did not import: " + names[i]);
            return clips;
        }

        private static void WriteWave(int kind, float duration, string path)
        {
            const int rate = 24000;
            int count = Mathf.RoundToInt(rate * duration);
            var random = new System.Random(223 + kind);
            float[] notes = { 110f, 130.813f, 146.832f, 164.814f, 195.998f, 164.814f, 146.832f, 130.813f };
            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);
            writer.Write(new[] { 'R','I','F','F' });
            writer.Write(36 + count * 2);
            writer.Write(new[] { 'W','A','V','E','f','m','t',' ' });
            writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
            writer.Write(new[] { 'd','a','t','a' });
            writer.Write(count * 2);
            float smoothNoise = 0;
            for (int sample = 0; sample < count; sample++)
            {
                float t = sample / (float)rate;
                float noise = (float)random.NextDouble() * 2f - 1f;
                smoothNoise = Mathf.Lerp(smoothNoise, noise, kind == 9 ? .061f : .025f);
                float envelope = Mathf.Min(1f,t * 230f) * Mathf.Pow(Mathf.Max(0, 1 - t / duration),2.4f);
                float value;
                if (kind == 7)
                {
                    int beat = (int)(t * 2);
                    float a = Mathf.Repeat(t,.5f);
                    float f = notes[(beat / 2 + beat % 2 * 2) % notes.Length];
                    float pluck = (Wave(f * 2,a) + .36f * Wave(f * 4,a)) * Mathf.Exp(-a * 11f) * Mathf.Min(1,a*120f);
                    value = .18f * pluck + .20f * Wave(55,t) * (.71f + .29f * Wave(.25f,t))
                        + .19f * Wave(70,a) * Mathf.Exp(-a*24f) * Mathf.Min(1,a*140f) + .022f * noise;
                }
                else if (kind >= 8)
                    value = smoothNoise * (kind == 9 ? .95f : .60f) * (.66f + .34f * Wave(.25f,t));
                else
                    value = envelope * (kind switch
                    {
                        0 => .42f * noise + .28f * Wave(95,t),
                        1 => .13f * noise + .36f * Wave(180 + 310*t,t),
                        2 => .22f * noise + .75f * smoothNoise,
                        3 => .44f * Wave(1046.5f,t) + .24f * Wave(1567.98f,t),
                        4 => .33f * Wave(392 + 360*t,t) + .29f * Wave(587.33f,t),
                        5 => .51f * Wave(57,t) + .38f * noise,
                        _ => .31f * Wave(174.61f,t) + .21f * Wave(130.81f,t)
                    });
                if (kind >= 7) value *= Mathf.Clamp01(Mathf.Min(t,duration-t) * 21f);
                writer.Write((short)(Mathf.Clamp(value,-1f,1f) * 32760));
            }
        }

        private static float Wave(float hz,float t) => Mathf.Sin(hz * t * 2f * Mathf.PI);
    }
}
