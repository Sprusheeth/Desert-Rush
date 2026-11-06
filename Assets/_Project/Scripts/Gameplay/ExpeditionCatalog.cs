using UnityEngine;

namespace DesertRush
{
    public sealed class ExpeditionCatalog : ScriptableObject
    {
        public RunnerMotor explorer;
        public GameObject[] obstacles, pickups;
        public SegmentProfile[] profiles;
        public GameObject glider;
        public Material[] outfits;
        public AudioClip[] sounds;
    }
}
