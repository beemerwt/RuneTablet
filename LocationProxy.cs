using HarmonyLib;

namespace RuneTablet
{
    [HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.SpawnLocation))]
    public class LocationProxySpawnPatch
    {
        public static void Postfix(LocationProxy __instance, ref bool __result)
        {
            var possibleRunestone = __instance.m_instance;
            if (possibleRunestone == null)
            {
                Plugin.Logger.LogWarning("LocationProxy.SpawnLocation: m_instance is null");
                return;
            }

            var isRunestone = possibleRunestone.GetComponent<RuneStone>() != null;
            if (!isRunestone)
            {
                Plugin.Logger.LogDebug("LocationProxy.SpawnLocation: m_instance is not a runestone");
                return;
            }

            var destructible = possibleRunestone.GetComponent<Destructible>();
            if (destructible == null)
            {
                Plugin.Logger.LogWarning("LocationProxy.SpawnLocation: Destructible is null");
                return;
            }

            var znetView = destructible.GetComponentInParent<ZNetView>();
            if (znetView == null)
            {
                Plugin.Logger.LogWarning("LocationProxy.SpawnLocation: ZNetView is null");
                return;
            }

            Plugin.Logger.LogDebug("LocationProxy.SpawnLocation: Found runestone");
            destructible.m_nview = znetView;
            destructible.Awake();
        }
    }
}
