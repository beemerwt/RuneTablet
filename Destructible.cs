using HarmonyLib;

namespace RuneTablet
{
    [HarmonyPatch(typeof(Destructible), nameof(Destructible.Damage))]
    public class DestructibleDamagePatch
    {
        public static void Prefix(HitData hit, Destructible __instance)
        {
            if (__instance == null)
            {
                Plugin.Logger.LogWarning("Destructible.Damage: Destructible is null");
                return;
            }

            Plugin.Logger.LogInfo("Destructible.Damage: First Frame: " + __instance.m_firstFrame.ToString());

            var isRuneStone = __instance.GetComponentInParent<RuneStone>() != null;
            if (!isRuneStone)
                return;

            Plugin.Logger.LogInfo("Destructible.Damage: Found RuneStone");

            if (__instance.m_nview == null)
                Plugin.Logger.LogWarning("Destructible.Damage: ZNetView is null");
            else
                Plugin.Logger.LogInfo("Destructible.Damage: View Valid: " + __instance.m_nview.IsValid().ToString());

            // __instance.m_nview = znetView;
            // __instance.Awake();
        }
    }

    [HarmonyPatch(typeof(Destructible), nameof(Destructible.RPC_Damage))]
    public class DestructibleRPCDamagePatch
    {
        public static void Prefix(long sender, HitData hit, Destructible __instance)
        {
            Plugin.Logger.LogWarning("Destructible.RPC_Damage: Called");
        }
    }
}