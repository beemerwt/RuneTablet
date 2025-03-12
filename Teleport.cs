using HarmonyLib;

namespace RuneTablet
{
	[HarmonyPatch(typeof(Teleport), nameof(Teleport.Interact))]
	public class Teleport_Interact_Patch
	{
		private static void Postfix(Humanoid character, Teleport __instance, ref bool __result)
		{
			// We didn't enter...
			if (!__result || !character.InInterior()) return;

			var otherSide = __instance.m_targetPoint; // ExteriorGateway
			var dungeon = otherSide.GetComponentInParent<Location>();
			if (dungeon == null) return;

			var locationInstance = Helper.GetLocationInstance(dungeon);
			if (locationInstance.HasValue)
				Plugin.SaveLocation(locationInstance.Value);
			else
				Plugin.Logger.LogError("Could not find location instance for " + dungeon.name);
		}
	}
}
