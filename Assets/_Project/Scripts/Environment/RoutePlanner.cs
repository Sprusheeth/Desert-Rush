namespace DesertRush
{
    public static class RoutePlanner
    {
        public const float RowSpacing = 19f;
        public const float SegmentLength = RowSpacing * 3f;
        public const float MaximumSpeed = 34f;

        // The protected lane shifts by at most one lane per row, including across seams.
        public static int NextLane(System.Random random, int previous) =>
            UnityEngine.Mathf.Clamp(previous + random.Next(-1, 2), -1, 1);
    }
}
