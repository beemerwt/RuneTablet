using Jotunn.Configs;
using Jotunn.Entities;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace RuneTablet
{
    using static Minimap;
    using Vector3 = UnityEngine.Vector3;

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
        }) { }

        public static bool Consume(Player player)
        {
            if (player == null)
            {
                Plugin.Logger.LogWarning("Player.ConsumeItem: Called with null player");
                return false;
            }

            var biome = player.GetCurrentBiome();
            Plugin.Logger.LogInfo("Player.ConsumeItem: Called in biome " + biome);

            if (biome.HasFlag(Heightmap.Biome.Ocean) || biome.HasFlag(Heightmap.Biome.Meadows))
            {
                player.Message(MessageHud.MessageType.Center, "Cannot use that in this biome.");
                return false;
            }

            if (biome.HasFlag(Heightmap.Biome.AshLands) && player.IsBiomeKnown(Heightmap.Biome.AshLands))
                DiscoverNearestDungeon(player, Heightmap.Biome.AshLands);

            else if (biome.HasFlag(Heightmap.Biome.Mistlands) && player.IsBiomeKnown(Heightmap.Biome.Mistlands))
                DiscoverNearestDungeon(player, Heightmap.Biome.Mistlands);

            else if (biome.HasFlag(Heightmap.Biome.Plains) && player.IsBiomeKnown(Heightmap.Biome.Plains))
                DiscoverNearestDungeon(player, Heightmap.Biome.Plains);

            else if (biome.HasFlag(Heightmap.Biome.Mountain) && player.IsBiomeKnown(Heightmap.Biome.Mountain))
                DiscoverNearestDungeon(player, Heightmap.Biome.Mountain);

            else if (biome.HasFlag(Heightmap.Biome.DeepNorth) && player.IsBiomeKnown(Heightmap.Biome.DeepNorth))
                DiscoverNearestDungeon(player, Heightmap.Biome.Mountain);

            else if (biome.HasFlag(Heightmap.Biome.Swamp) && player.IsBiomeKnown(Heightmap.Biome.Swamp))
                DiscoverNearestDungeon(player, Heightmap.Biome.Swamp);

            else if (biome.HasFlag(Heightmap.Biome.BlackForest) && player.IsBiomeKnown(Heightmap.Biome.BlackForest))
                DiscoverNearestDungeon(player, Heightmap.Biome.BlackForest);

            return true;
        }

        private static Tuple<string, Vector3> FindClosestLocationFromTypes(string[] types, Vector3 point)
        {
            string locationType = null;
            Vector3 locationPos = Vector3.zero;
            float distance = float.PositiveInfinity;

            List<ZoneSystem.LocationInstance> locations = [];
            foreach (var type in types) {
                List<ZoneSystem.LocationInstance> addLocations = [];
                if (ZoneSystem.instance.FindLocations(type, ref addLocations))
                    locations.AddRange(addLocations);
            }

            foreach (var location in locations)
            {
                var type = location.m_location.m_prefab.Name;
                if (Minimap.instance.HaveSimilarPin(location.m_position, PinType.Hildir1, Helper.GetPinName(type), true))
                    continue;

                var newDistance = Vector3.Distance(point, location.m_position);
                if (newDistance < distance)
                {
                    locationType = type;
                    locationPos = location.m_position;
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
