using System.Collections.Generic;

namespace RuneTablet
{
	public static class Helper
	{
		public static string GetHashedName(RuneStone component)
		{
			var position = component.transform.position;
			var hash = ((int)position.x * 31) + ((int)position.z * 37);
			return component.name + "-" + hash.ToString();
		}

		public static ZoneSystem.LocationInstance? GetLocationInstance(Location location)
		{
			var zoneId = ZoneSystem.GetZone(location.transform.position);
			if (!ZoneSystem.instance.m_locationInstances.TryGetValue(zoneId, out ZoneSystem.LocationInstance instance))
				return null;

			return instance;
		}

		public static string GetHashedName(Location location)
		{
			var instance = GetLocationInstance(location);
			if (instance.HasValue)
				return GetHashedName(instance.Value);

			Plugin.Logger.LogError("Could not find location instance for " + location.name);
			return null;
		}

		public static string GetHashedName(ZoneSystem.LocationInstance instance)
		{
			var zoneId = ZoneSystem.GetZone(instance.m_position);
			var worldName = ZNet.World.m_name;
			var prefabName = instance.m_location.m_prefab.Name;
			var hash = (zoneId.x * 31) + (zoneId.y * 37);
			return $"{worldName}-{prefabName}-{hash}";
		}

		private static Dictionary<string, string> PinNames = new() {
			{ "Crypt2", "$location_forestcrypt" },
			{ "Crypt3", "$location_forestcrypt" },
			{ "Crypt4", "$location_forestcrypt" },
			{ "Mistlands_DvergrTownEntrance1", "$location_dvergrtown" },
			{ "Mistlands_DvergrTownEntrance2", "$location_dvergrtown" },
			{ "Hildir_crypt", "$hud_pin_hildir1" },
			{ "Hildir_cave", "$hud_pin_hildir2" },
			{ "Hildir_plainsfortress", "$hud_pin_hildir3" },
			{ "MountainCave02", "$location_mountaincave" },
			{ "GoblinCamp2", "$enemy_goblin" },
			{ "SunkenCrypt4", "$location_sunkencrypt" },
			{ "MorgenHole1", "$location_morgenhole" },
			{ "MorgenHole2", "$location_morgenhole" },
			{ "MorgenHole3", "$location_morgenhole" },
		};

		public static string GetPinName(string locationName)
			=> PinNames.TryGetValue(locationName, out string name) ? name : locationName;
	}
}
