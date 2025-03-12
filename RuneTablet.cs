using Jotunn.Configs;
using Jotunn.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace RuneTablet
{
	public class RuneTablet : CustomItem
	{
		internal static Sprite IconSprite;
		private static Dictionary<Heightmap.Biome, string[]> BiomeDungeons = new() {
			{ Heightmap.Biome.AshLands, [] },
			{ Heightmap.Biome.BlackForest, ["Hildir_crypt", "Crypt2", "Crypt3", "Crypt4"] },
			{ Heightmap.Biome.Mistlands, ["Mistlands_DvergrTownEntrance1", "Mistlands_DvergrTownEntrance2"] },
			{ Heightmap.Biome.DeepNorth, ["Hildir_cave", "MountainCave02"] },
			{ Heightmap.Biome.Mountain, ["Hildir_cave", "MountainCave02"] },
			{ Heightmap.Biome.Plains, ["GoblinCamp2", "StoneTower1", "StoneTower3", "Hildir_plainsfortress"] },
			{ Heightmap.Biome.Swamp, ["SunkenCrypt4"] }
		};

		public RuneTablet() : base("RuneTablet", true, new ItemConfig() {
			Name = "$item_runetablet",
			Description = "$item_runetablet_desc",
			CraftingStation = CraftingStations.Workbench,
			Icon = IconSprite,
			Requirements =
				[
					new RequirementConfig { Item = "Stone", Amount = 10 },
					new RequirementConfig { Item = "RuneCrystal", Amount = 4 }
				],
			Weight = 10f,
		})
		{ }

		public static bool Consume(Player player)
		{
			if (player == null)
			{
				Plugin.Logger.LogWarning("Player.ConsumeItem: Called with null player");
				return false;
			}

			var biome = player.GetCurrentBiome();
			Plugin.Logger.LogInfo("Player.ConsumeItem: Called in biome " + biome.ToString());

			if (biome.HasFlag(Heightmap.Biome.Ocean) || biome.HasFlag(Heightmap.Biome.Meadows))
			{
				player.Message(MessageHud.MessageType.Center, "Cannot use that in this biome.");
				return false;
			}

			foreach (var bv in Enum.GetValues(typeof(Heightmap.Biome)))
			{
				var biomeType = (Heightmap.Biome)bv;
				if (biomeType == Heightmap.Biome.None || biomeType == Heightmap.Biome.All)
					continue;

				if (biome.HasFlag(biomeType) && player.IsBiomeKnown(biomeType))
					return DiscoverNearestDungeon(player, biomeType);
			}

			return false;
		}

		private static Tuple<string, Vector3> FindClosestLocationFromTypes(string[] types, Vector3 point)
		{
			string locationType = null;
			Vector3 locationPos = Vector3.zero;
			float distance = float.PositiveInfinity;

			List<ZoneSystem.LocationInstance> locations = [];
			foreach (var type in types)
			{
				List<ZoneSystem.LocationInstance> addLocations = [];
				if (ZoneSystem.instance.FindLocations(type, ref addLocations))
					locations.AddRange(addLocations);
			}

			foreach (var instance in locations)
			{
				var type = instance.m_location.m_prefab.Name;
				if (Minimap.instance.HavePinInRange(instance.m_position, 14f))
				{
					// will save if it hasn't already been saved.
					Plugin.SaveLocation(instance);
					continue;
				}

				var hash = Helper.GetHashedName(instance);
				if (Plugin.FoundDungeons.Contains(hash))
					continue;

				var newDistance = Vector3.Distance(point, instance.m_position);
				if (newDistance < distance)
				{
					locationType = type;
					locationPos = instance.m_position;
					distance = newDistance;
				}
			}

			if (String.IsNullOrEmpty(locationType))
				return null;

			return new Tuple<string, Vector3>(locationType, locationPos);
		}

		public static bool DiscoverNearestDungeon(Player player, Heightmap.Biome biome)
		{
			if (!BiomeDungeons.TryGetValue(biome, out var locationTypes))
			{
				Plugin.Logger.LogWarning($"DiscoverNearestDungeon: Invalid biome {biome}");
				return false;
			}

			var playerPos = player.transform.position;
			var closestLocation = FindClosestLocationFromTypes(locationTypes, playerPos);
			if (closestLocation == null)
			{
				Player.m_localPlayer.Message(MessageHud.MessageType.Center,
					"<color=red>No locations found</color>");

				Plugin.Logger.LogWarning("DiscoverNearestDungeon: No locations found");
				return false;
			}

			var closestPos = closestLocation.Item2;
			var pinName = Localization.instance.Localize(Helper.GetPinName(closestLocation.Item1));
			return Minimap.instance.DiscoverLocation(closestPos, Minimap.PinType.Hildir1, pinName, true);
		}
	}
}
