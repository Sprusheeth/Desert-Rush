using UnityEngine;

namespace DesertRush
{
    public static class ExpeditionSave
    {
        private const string Prefix = "DesertRush.";
        public static int Best => PlayerPrefs.GetInt(Prefix + "Best");
        public static int Coins => PlayerPrefs.GetInt(Prefix + "Coins");
        public static int TotalCoins => PlayerPrefs.GetInt(Prefix + "Lifetime");
        public static int Longest => PlayerPrefs.GetInt(Prefix + "Distance");
        public static int Outfit => PlayerPrefs.GetInt(Prefix + "Outfit");
        public static float Volume => PlayerPrefs.GetFloat(Prefix + "Volume", .7f);
        public static int Quality => Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "Quality", QualitySettings.names.Length - 1),
            0, QualitySettings.names.Length - 1);
        public static bool Owns(int outfit) => outfit == 0 || PlayerPrefs.GetInt(Prefix + "Owned" + outfit) == 1;
        public static bool Claimed(int mission) => PlayerPrefs.GetInt(Prefix + "Mission" + mission) == 1;

        public static void Record(int score, int distance, int coins)
        {
            PlayerPrefs.SetInt(Prefix + "Best", Mathf.Max(Best, score));
            PlayerPrefs.SetInt(Prefix + "Distance", Mathf.Max(Longest, distance));
            PlayerPrefs.SetInt(Prefix + "Coins", Coins + coins);
            PlayerPrefs.SetInt(Prefix + "Lifetime", TotalCoins + coins);
            PlayerPrefs.Save();
        }

        public static bool Equip(int outfit, int price)
        {
            if (!Owns(outfit))
            {
                if (Coins < price) return false;
                PlayerPrefs.SetInt(Prefix + "Coins", Coins - price);
                PlayerPrefs.SetInt(Prefix + "Owned" + outfit, 1);
            }

            PlayerPrefs.SetInt(Prefix + "Outfit", outfit);
            PlayerPrefs.Save();
            return true;
        }

        public static void Claim(int mission, int progress, int target, int reward)
        {
            if (progress < target || Claimed(mission)) return;
            PlayerPrefs.SetInt(Prefix + "Coins", Coins + reward);
            PlayerPrefs.SetInt(Prefix + "Mission" + mission, 1);
            PlayerPrefs.Save();
        }

        public static void ApplySettings()
        {
            AudioListener.volume = Volume;
            QualitySettings.SetQualityLevel(Quality, true);
        }

        public static void SetVolume(float value)
        {
            PlayerPrefs.SetFloat(Prefix + "Volume", value);
            AudioListener.volume = value;
            PlayerPrefs.Save();
        }

        public static void SetQuality(int value)
        {
            PlayerPrefs.SetInt(Prefix + "Quality", value);
            QualitySettings.SetQualityLevel(value, true);
            PlayerPrefs.Save();
        }
    }
}
