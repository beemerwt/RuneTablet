using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneTablet
{
    [HarmonyPatch(typeof(Location), nameof(Location.Awake))]
    public class LocationAwakePatch
    {
        public static void Postfix(Location __instance)
        {
            if (__instance == null)
                return;

            var isRunestone = __instance.GetComponent<RuneStone>() != null;
            if (!isRunestone)
                return;

            Plugin.Logger.LogDebug("Location.Awake: Found runestone");

            var destructible = __instance.GetComponent<Destructible>();
            if (destructible == null)
            {
                Plugin.Logger.LogWarning("Location.Awake: Destructible is null");
                return;
            }

            var znetView = destructible.GetComponentInParent<ZNetView>();
            if (znetView == null)
            {
                Plugin.Logger.LogWarning("Location.Awake: ZNetView is null");
                return;
            }

            destructible.m_nview = znetView;
            destructible.Awake();

            Plugin.Logger.LogDebug("Location.Awake: Set ZNetView on Destructible");
        }
    }
}
