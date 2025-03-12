using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static FileHelpers;

namespace RuneTablet
{
	[BepInDependency(Jotunn.Main.ModGuid)]
	[BepInPlugin(RuneTabletGuid, RuneTabletName, NumericVersion)]
	public class Plugin : BaseUnityPlugin
	{
		public const string RuneTabletGuid = "org.bepinex.plugins.rune_tablet";
		public const string RuneTabletName = "Rune Tablet";
		public const string NumericVersion = "1.1.0";

		public static string SaveFile
		{
			get
			{
				var name = Player.m_localPlayer?.GetPlayerName();
				if (string.IsNullOrEmpty(name) || name == "...")
					return null;

				name += "_runetablet.fch";
				return Path.Combine(Utils.GetSaveDataPath(FileSource.Legacy), name);
			}
		}

		public new static ManualLogSource Logger { get; private set; }

		// The lowest game version known to work with this plugin.
		private static readonly GameVersion MinSupportedGameVersion = new(0, 219, 10);

		public static List<string> FoundRuneStones = [];
		public static List<string> FoundDungeons = [];

		private CustomLocalization Localization;

		private static bool IsGameVersionTooOld() => Version.CurrentVersion < MinSupportedGameVersion;

		private static readonly Harmony Harmony = new("mod.rune_tablet");

		private void Awake()
		{
			// Plugin startup logic
			Logger = base.Logger;
			Logger.LogInfo($"Valheim game version: {Version.GetVersionString()}");

			if (IsGameVersionTooOld())
			{
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

			runeTablet.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;

			PrefabManager.OnVanillaPrefabsAvailable -= AddClonedItems;
		}

		public static void SaveLocation(ZoneSystem.LocationInstance instance)
		{
			var hashedName = Helper.GetHashedName(instance);
			if (FoundDungeons.Contains(hashedName))
				return;

			FoundDungeons.Add(hashedName);
			Logger.LogInfo("Saved location " + hashedName);
		}

		public static void RecoverPins()
		{
			foreach (var instance in ZoneSystem.instance.m_locationInstances)
			{
				var hashedName = Helper.GetHashedName(instance.Value);
				if (FoundDungeons.Contains(hashedName))
				{
					if (!Minimap.instance.HavePinInRange(instance.Value.m_position, 14f))
						Minimap.instance.AddPin(instance.Value.m_position, Minimap.PinType.Hildir1,
							Helper.GetPinName(instance.Value.m_location.m_prefabName), true, false);
				}
			}
		}

		private static void LogTooOld()
		{
			Logger.LogError(
				$"This version of Rune Tablet ({NumericVersion}) expects a minimum game version of " +
				$"\"{MinSupportedGameVersion}\", but this game version is older at \"{Version.CurrentVersion}\". " +
				"Please either update the Valheim game, or use an older version of Rune Tablet.");
		}
		public static void SaveToDisk()
		{
			if (SaveFile == null)
			{
				Logger.LogError("Something went wrong when trying to save to disk");
				return;
			}

			ZPackage pkg = new();

			ZPackage runestonePkg = new();
			foreach (var runestone in FoundRuneStones)
				runestonePkg.Write(runestone);

			ZPackage dungeonPkg = new();
			foreach (var location in FoundDungeons)
				dungeonPkg.Write(location);

			pkg.Write(FoundRuneStones.Count);
			pkg.Write(runestonePkg);

			pkg.Write(FoundDungeons.Count);
			pkg.Write(dungeonPkg);

			byte[] array = pkg.GetArray();

			try
			{
				FileWriter fileWriter = new(SaveFile, fileSource: FileSource.Local);
				fileWriter.m_binary.Write(array.Length);
				fileWriter.m_binary.Write(array);
				fileWriter.Finish();
			} catch (Exception e)
			{
				Logger.LogWarning($"Error saving CoinPurse data: Path: {SaveFile}, Error: {e.Message}");
			}
		}
		public static void LoadFromDisk()
		{
			if (SaveFile == null)
			{
				Logger.LogError("Something went wrong when trying to load from disk");
				return;
			}

			if (!File.Exists(SaveFile))
			{
				File.Create(SaveFile).Close();
				return;
			}

			FoundRuneStones.Clear();
			FoundDungeons.Clear();

			FileReader fileReader;
			try
			{
				fileReader = new(SaveFile, fileSource: FileSource.Local);
			} catch (Exception e)
			{
				Logger.LogWarning($"Error loading RuneTablet data: Path: {SaveFile}, Error: {e.Message}");
				return;
			}

			byte[] data;
			try
			{
				BinaryReader binary = fileReader.m_binary;
				data = binary.ReadBytes(binary.ReadInt32());
				ZPackage pkg = new(data);

				int numRunestones = pkg.ReadInt();
				ZPackage runestonePkg = pkg.ReadPackage();

				int numDungeons = pkg.ReadInt();
				ZPackage dungeonPkg = pkg.ReadPackage();

				for (int i = 0; i < numRunestones; i++)
					FoundRuneStones.Add(runestonePkg.ReadString());

				for (int i = 0; i < numDungeons; i++)
					FoundDungeons.Add(dungeonPkg.ReadString());

			} catch (Exception e)
			{
				Logger.LogWarning($"Error loading RuneTablet data: Path: {SaveFile}, Error: {e.Message}");
				return;
			} finally
			{
				fileReader?.Dispose();
			}
		}
	}
}
