using UnityEngine;

namespace DesertRush
{
    public sealed class ExpeditionAudio : MonoBehaviour
    {
        private AudioClip[] clips;
        private AudioSource effects, music, wind, storm;

        public void Bind(AudioClip[] sounds)
        {
            clips = sounds;
            effects = Source(null, false, .65f);
            music = Source(clips[7], true, .33f);
            wind = Source(clips[8], true, .18f);
            storm = Source(clips[9], true, 0f);
        }

        public void Play(int sound, float pitch = 1f)
        {
            effects.pitch = pitch;
            effects.PlayOneShot(clips[sound]);
        }

        public void Mix(float intensity, bool paused)
        {
            music.volume = paused ? .15f : .33f;
            wind.volume = .14f + intensity * .1f;
            storm.volume = paused ? 0f : intensity * .48f;
        }

        private AudioSource Source(AudioClip clip, bool loop, float volume)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.clip = clip;
            source.volume = volume;
            if (loop) source.Play();
            return source;
        }
    }
}
