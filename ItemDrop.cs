using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ItemDrop;

namespace RuneTablet
{
    [HarmonyPatch(typeof(ItemDrop))]
    public class ItemDropPatches
    {

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemDrop.Eat))]
        public static bool EatPrefix(ref ItemDrop __instance, ref bool __result)
        {
            Plugin.Logger.LogInfo("ItemDrop.UseItem: Called on " + __instance.m_itemData.m_shared.m_name);

            if (__instance.m_itemData.m_shared.m_name != "$item_runetablet")
                return true;

            Plugin.Logger.LogInfo("ItemDrop.UseItem: Called on RuneTablet");

        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemDrop.UseItem))]
        public static bool UseItem(Humanoid user, ItemData item)
        {
            Plugin.Logger.LogInfo("ItemDrop.UseItem: Called on " + item.m_shared.m_name);
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemDrop.Interact))]
        public static bool Interact(Humanoid character, bool repeat, bool alt, ref ItemDrop __instance)
        {
            Plugin.Logger.LogInfo("ItemDrop.Interact: Called on " + __instance.m_itemData.m_shared.m_name);
            return true;
        }
    }
}
