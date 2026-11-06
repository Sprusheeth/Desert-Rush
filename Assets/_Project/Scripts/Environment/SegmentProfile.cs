using UnityEngine;

namespace DesertRush
{
    [CreateAssetMenu(menuName = "Desert Rush/Segment Profile")]
    public sealed class SegmentProfile : ScriptableObject
    {
        public GameObject terrain;
        public GameObject[] scenery;
        [Min(8f)] public float sceneryClearance = 8f;
        public bool bridge;
    }
}
