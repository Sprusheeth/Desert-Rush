using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DesertRush
{
    public sealed class ExpeditionUI : MonoBehaviour
    {
        private static readonly Color Ink = new(.055f, .075f, .09f);
        private static readonly Color Slate = new(.085f, .126f, .145f, .96f);
        private static readonly Color Gold = new(1f, .68f, .31f);
        private static readonly Color Ivory = new(1f, .93f, .81f);
        private static readonly Color Muted = new(.72f, .78f, .76f);
        private static readonly string[] Outfits = { "Caravan scout", "Oasis sentinel", "Dawn cartographer" };
        private static readonly int[] Prices = { 0, 125, 300 };
        private static readonly string[] Powers = { "MAGNET", "OVERDRIVE", "SHIELD", "DOUBLE", "GLIDER" };

        private Expedition run;
        private RectTransform canvas, page;
        private Text telemetry, powerStatus;
        private CanvasGroup fade;
        private Font font;
        private Button first;
        private float nextHud;
        private readonly StringBuilder powers = new(100);

        public void Bind(Expedition expedition)
        {
            run = expedition;
            var screen = GetComponent<Canvas>();
            screen.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas = (RectTransform)transform;
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (EventSystem.current == null)
                new GameObject("UI navigation", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public void Show(RunState state)
        {
            switch (state)
            {
                case RunState.Menu: Main(); break;
                case RunState.Running: Hud(); break;
                case RunState.Paused: Pause(); break;
                case RunState.Finished: Finish(); break;
            }
        }

        private void Update()
        {
            if (fade != null)
                fade.alpha = Mathf.MoveTowards(fade.alpha, 1f, Time.unscaledDeltaTime * 7f);
        }

        private void Main()
        {
            BeginPage(false);
            var cover = ImageAt(page, 0, 0, 520, 720, new Color(.048f, .07f, .079f, .96f));
            var side = (RectTransform)cover.transform;
            ImageAt(side, 45, 52, 32, 3, Gold);
            Label(side, "THE SUNKEN KINGDOM  /  EXPEDITION 01", 90, 38, 390, 32, 15, Gold);
            Label(side, "DESERT", 41, 80, 462, 100, 91, Ivory, true);
            Label(side, "RUSH", 44, 154, 452, 112, 103, Gold, true);
            Label(side, "A courier's race through the buried trade roads", 48, 277, 444, 36, 19, Muted);
            Action(side, "START EXPEDITION     >", 48, 348, 424, 65, run.Begin, true);
            Action(side, "CHARACTERS", 48, 431, 205, 49, () => Wardrobe(false));
            Action(side, "RELIC SHOP", 266, 431, 206, 49, () => Wardrobe(true));
            Action(side, "MISSIONS", 48, 490, 205, 49, Missions);
            Action(side, "SETTINGS", 266, 490, 206, 49, () => Settings(Main));
            Action(side, "CREDITS", 48, 549, 205, 44, Credits);
            Action(side, "QUIT", 266, 549, 206, 44, Application.Quit);
            Label(side, $"BEST  {ExpeditionSave.Best:N0}       /       GOLD  {ExpeditionSave.Coins:N0}",
                48, 647, 420, 28, 19, Gold, true);
            Label(page, "01   /   THE SUNSTONE ROAD", 901, 637, 320, 35, 17, Ivory, true, TextAnchor.MiddleRight);
            SelectFirst();
        }

        private void Hud()
        {
            BeginPage(false);
            ImageAt(page, 32, 28, 576, 72, new Color(.043f, .069f, .08f, .88f));
            ImageAt(page, 32, 28, 576, 3, Gold);
            telemetry = Label(page, "", 48, 38, 550, 49, 22, Ivory, true);
            powerStatus = Label(page, "", 46, 101, 920, 37, 19, Gold, true);
            Action(page, "II   PAUSE", 1098, 29, 150, 61, run.TogglePause);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            nextHud = 0f;
            UpdateHud();
        }

        public void UpdateHud()
        {
            if (Time.unscaledTime < nextHud || telemetry == null) return;
            nextHud = Time.unscaledTime + .1f;
            telemetry.text = $"{run.Score:N0}  PTS     /     {run.Distance:0} m     /     {run.Coins} GOLD     /     {run.Speed:0} m/s";
            powers.Clear();
            for (int i = 0; i < Powers.Length; i++)
            {
                float remaining = run.Effects.Remaining((PickupKind)(i + 3));
                if (remaining > 0f) powers.Append(Powers[i]).Append("  ").Append(Mathf.CeilToInt(remaining)).Append("s     ");
            }
            powerStatus.text = powers.ToString();
        }

        private void Pause()
        {
            var card = Card("EXPEDITION PAUSED", "Take a breath before the next dune.");
            Action(card, "RESUME", 45, 153, 500, 59, run.TogglePause, true);
            Action(card, "RESTART", 45, 225, 500, 55, run.Begin);
            Action(card, "SETTINGS", 45, 292, 500, 55, () => Settings(Pause));
            Action(card, "MAIN MENU", 45, 359, 500, 55, run.Menu);
            SelectFirst();
        }

        private void Finish()
        {
            var card = Card(run.NewBest ? "NEW EXPEDITION RECORD" : "RUN COMPLETE", "The desert keeps its secrets.");
            Label(card, $"{run.Score:N0}", 44, 140, 502, 92, 78, Gold, true);
            Label(card, $"BEST {ExpeditionSave.Best:N0}    /    {(int)run.Distance:N0} m    /    {run.Coins:N0} GOLD",
                48, 239, 497, 43, 22, Ivory, true);
            Action(card, "RUN AGAIN", 45, 337, 244, 67, run.Begin, true);
            Action(card, "MAIN MENU", 301, 337, 244, 67, run.Menu);
            SelectFirst();
        }

        private void Wardrobe(bool shop)
        {
            var card = Card(shop ? "RELIC SHOP" : "CHARACTERS", $"Caravan wardrobe   /   {ExpeditionSave.Coins:N0} gold available");
            for (int i = 0; i < Outfits.Length; i++)
            {
                int outfit = i;
                bool owned = ExpeditionSave.Owns(i);
                string status = i == ExpeditionSave.Outfit ? "EQUIPPED" : owned ? "EQUIP" : $"BUY  {Prices[i]}";
                var button = Action(card, $"{Outfits[i].ToUpperInvariant()}    /    {status}", 45, 146 + i * 70, 500, 55, () =>
                {
                    if (!ExpeditionSave.Equip(outfit, Prices[outfit])) return;
                    run.Dress();
                    Wardrobe(shop);
                }, i == ExpeditionSave.Outfit);
                button.interactable = i != ExpeditionSave.Outfit
                    && (owned || (shop && ExpeditionSave.Coins >= Prices[i]));
            }
            Action(card, "BACK", 45, 381, 240, 49, Main);
            SelectFirst();
        }

        private void Missions()
        {
            var card = Card("CARAVAN CONTRACTS", "Rewards are credited to your expedition balance.");
            Mission(card, 0, "CROSS THE SALT ROAD", ExpeditionSave.Longest, 800, 100, "m");
            Mission(card, 1, "RECOVER THE ROYAL TITHE", ExpeditionSave.TotalCoins, 100, 150, "gold");
            Mission(card, 2, "SET A ROUTE RECORD", ExpeditionSave.Best, 3000, 200, "pts");
            Action(card, "BACK", 45, 397, 240, 49, Main);
            SelectFirst();
        }

        private void Mission(RectTransform card, int i, string title, int progress, int target, int reward, string unit)
        {
            float y = 136 + i * 79;
            Label(card, title, 47, y, 330, 29, 18, Ivory, true);
            Label(card, $"{Mathf.Min(progress, target):N0} / {target:N0} {unit}    +{reward} gold", 47, y + 30, 330, 25, 17, Muted);
            bool claimed = ExpeditionSave.Claimed(i);
            var button = Action(card, claimed ? "CLAIMED" : "CLAIM", 385, y + 7, 157, 45, () =>
            {
                ExpeditionSave.Claim(i, progress, target, reward);
                Missions();
            }, progress >= target && !claimed);
            button.interactable = progress >= target && !claimed;
        }

        private void Settings(Action back)
        {
            var card = Card("FIELD SETTINGS", "Audio and render quality");
            Label(card, "MASTER VOLUME", 47, 142, 300, 35, 21, Ivory, true);
            Label(card, $"{ExpeditionSave.Volume:P0}", 387, 142, 155, 35, 21, Gold, true, TextAnchor.MiddleRight);
            Action(card, "-   LOWER", 45, 184, 245, 53, () => ChangeVolume(-.1f, back));
            Action(card, "+   RAISE", 299, 184, 245, 53, () => ChangeVolume(.1f, back));
            string[] names = QualitySettings.names;
            int quality = ExpeditionSave.Quality;
            Action(card, $"GRAPHICS    /    {names[quality].ToUpperInvariant()}", 45, 263, 500, 57, () =>
            {
                ExpeditionSave.SetQuality((quality + 1) % names.Length);
                Settings(back);
            });
            Action(card, "BACK", 45, 382, 240, 49, back);
            SelectFirst();
        }

        private void ChangeVolume(float amount, Action back)
        {
            ExpeditionSave.SetVolume(Mathf.Clamp01(ExpeditionSave.Volume + amount));
            Settings(back);
        }

        private void Credits()
        {
            var card = Card("FIELD NOTES", "DESERT RUSH   /   The Sunken Kingdom");
            Label(card, "Original procedural geometry, material patterns, character rig, score and sound effects.\nBuilt with Unity and the Universal Render Pipeline.",
                47, 153, 490, 144, 23, Ivory);
            Label(card, "A/D or arrows · Change lanes\nW/Up/Space · Jump     S/Down · Slide     Esc · Pause\nGamepad · Stick, South, East, Start",
                47, 282, 490, 91, 18, Muted);
            Action(card, "BACK", 45, 397, 240, 49, Main);
            SelectFirst();
        }

        public void ShowLoading()
        {
            var card = Card("CROSSING THE DUNES", "Preparing the expedition route");
            ImageAt(card, 47, 167, 497, 4, Gold);
        }

        private RectTransform Card(string title, string subtitle)
        {
            BeginPage(true);
            var panel = (RectTransform)ImageAt(page, 345, 118, 590, 480, Slate).transform;
            ImageAt(panel, 0, 0, 590, 5, Gold);
            Label(panel, title, 44, 38, 507, 52, 38, Ivory, true);
            Label(panel, subtitle, 47, 94, 497, 33, 19, Muted);
            return panel;
        }

        private void BeginPage(bool dim)
        {
            if (page != null)
            {
                page.gameObject.SetActive(false);
                Destroy(page.gameObject);
            }
            first = null;
            page = At(canvas, "Screen", 0, 0, 1280, 720, true);
            fade = page.gameObject.AddComponent<CanvasGroup>();
            fade.alpha = .68f;
            if (dim)
            {
                var veil = ImageAt(page, 0, 0, 1280, 720, new Color(.027f, .044f, .049f, .69f));
                var t = veil.rectTransform;
                t.anchorMin = Vector2.zero;
                t.anchorMax = Vector2.one;
                t.offsetMin = new Vector2(-5000, -5000);
                t.offsetMax = new Vector2(5000, 5000);
            }
        }

        private void SelectFirst()
        {
            var options = page.GetComponentsInChildren<Button>();
            foreach (var button in options)
            {
                if (!button.interactable) continue;
                first = button;
                break;
            }
            if (first != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        private Button Action(RectTransform parent, string text, float x, float y, float w, float h,
            Action action, bool primary = false)
        {
            var image = ImageAt(parent, x, y, w, h, Color.white);
            image.gameObject.name = text;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = primary ? Gold : new Color(.145f, .205f, .224f);
            colors.highlightedColor = primary ? new Color(1f, .80f, .47f) : new Color(.26f, .36f, .38f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = primary ? new Color(.87f, .48f, .19f) : new Color(.12f, .19f, .21f);
            colors.disabledColor = new Color(.13f, .15f, .16f);
            button.colors = colors;
            button.onClick.AddListener(() => action());
            Label(image.rectTransform, text, 9, 0, w - 18, h, 20, primary ? Ink : Ivory, true, TextAnchor.MiddleCenter);
            return button;
        }

        private Text Label(RectTransform parent, string value, float x, float y, float w, float h, int size,
            Color color, bool bold = false, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var label = At(parent, "Text", x, y, w, h).gameObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            label.alignment = alignment;
            label.color = color;
            label.text = value;
            label.raycastTarget = false;
            return label;
        }

        private static Image ImageAt(RectTransform parent, float x, float y, float w, float h, Color color)
        {
            var image = At(parent, "Panel", x, y, w, h).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static RectTransform At(RectTransform parent, string title, float x, float y, float w, float h,
            bool centered = false)
        {
            var rect = (RectTransform)new GameObject(title, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = centered ? new Vector2(.5f, .5f) : new Vector2(0, 1);
            rect.pivot = centered ? new Vector2(.5f, .5f) : new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }
    }
}

