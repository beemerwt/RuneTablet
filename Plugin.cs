using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RuneTablet
{
    [BepInPlugin(RuneTabletGuid, RuneTabletName, NumericVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string RuneTabletGuid = "org.bepinex.plugins.rune_tablet";
        public const string RuneTabletName = "Rune Tablet";
        public const string NumericVersion = "1.0.0";
        public new static ManualLogSource Logger { get; private set; }

        // The lowest game version known to work with this plugin.
        private static readonly GameVersion MinSupportedGameVersion = new(0, 219, 10);

        // The game version this version this plugin was compiled against.
        private static readonly GameVersion TargetGameVersion = new(0, 220, 3);


        /*
         * TODO: ServerSync
        private static readonly object VersionCheck = new VersionCheck(RuneTabletGuid) {
            DisplayName = "Rune Tablet",
            CurrentVersion = NumericVersion,
            MinimumRequiredVersion = NumericVersion,
        };
        */

        public static List<string> FoundRuneStones = [];
        public static List<string> LocatedDungeons = [];

        private CustomLocalization Localization;

        private static bool IsGameVersionTooOld() => Version.CurrentVersion < MinSupportedGameVersion;

        private static readonly Harmony Harmony = new("mod.rune_tablet");

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Valheim game version: {Version.GetVersionString()}");

            if (IsGameVersionTooOld()) {
                LogTooOld();
                Logger.LogFatal("Aborting loading of Rune Tablet due to incompatible version.");
                return;
            }

            LoadAssets();
            AddLocalization();

            PrefabManager.OnVanillaPrefabsAvailable += AddClonedItems;

            Harmony.PatchAll();
            Logger.LogInfo("Rune Tablet done loading.");
        }

        private void AddLocalization()
        {
            Localization = LocalizationManager.Instance.GetLocalization();
            Localization.AddTranslation("English", new Dictionary<string, string>() {
                { "item_runecrystal", "Rune Crystal" },
                { "item_runecrystal_desc", "A potent source of energy from a runestone" },
                { "item_runetablet", "Rune Tablet" },
                { "item_runetablet_desc", "A mysterious tablet inscribed with runes glowing with energy." },
            });
        }

        private void LoadAssets()
        {
            string modPath = Path.GetDirectoryName(Info.Location);
            var runeCrystalTex = AssetUtils.LoadTexture(Path.Combine(modPath, "Assets/rune_crystal.png"));
            var runeTabletTex = AssetUtils.LoadTexture(Path.Combine(modPath, "Assets/rune_tablet.png"));

            RuneCrystal.IconSprite = Sprite.Create(runeCrystalTex, new Rect(0, 0, runeCrystalTex.width, runeCrystalTex.height), Vector2.zero);
            RuneTablet.IconSprite = Sprite.Create(runeTabletTex, new Rect(0, 0, runeTabletTex.width, runeTabletTex.height), Vector2.zero);
        }

        private void AddClonedItems()
        {
            RuneCrystal runeCrystal = new();
            RuneTablet runeTablet = new();

            ItemManager.Instance.AddItem(runeCrystal);
            ItemManager.Instance.AddItem(runeTablet);

            // Make it usable
            runeTablet.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;

            PrefabManager.OnVanillaPrefabsAvailable -= AddClonedItems;
        }

        private static void LogTooOld()
        {
            Logger.LogError(
                $"This version of Rune Tablet ({NumericVersion}) expects a minimum game version of " +
                $"\"{MinSupportedGameVersion}\", but this game version is older at \"{Version.CurrentVersion}\". " +
                "Please either update the Valheim game, or use an older version of Rune Tablet.");
        }
    }
}
