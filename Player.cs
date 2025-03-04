using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneTablet
{
    [HarmonyPatch(typeof(Player))]
    public class PlayerPatches {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.Save))]
        public static void SavePostfix(ZPackage pkg, Player __instance)
        {
            Plugin.Logger.LogInfo("Player.Save: Called");

            foreach (var name in Plugin.FoundRuneStones)
                pkg.Write(name);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.Load))]
        public static void LoadPostfix(ZPackage pkg, Player __instance)
        {
            Plugin.Logger.LogInfo("Player.Load: Called");

            Plugin.FoundRuneStones.Clear();
            while (pkg.GetPos() < pkg.Size())
            {
                var found = pkg.ReadString();
                Plugin.FoundRuneStones.Add(found);
                Plugin.Logger.LogInfo("Player.Load: Found runestone " + found);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.ConsumeItem))]
        public static bool ConsumeItemPrefix(Inventory inventory, ItemDrop.ItemData item, ref Player __instance, ref bool __result)
        {
            if (item.m_shared.m_name != "$item_runetablet")
                return true;

            Plugin.Logger.LogInfo("Player.ConsumeItem: Called on Rune Tablet");
            __result = RuneTablet.Consume(__instance);

            if (__result)
                inventory.RemoveItem(item);

            return !__result; // return false to skip the original method (opposite of true if we consumed the item)
        }
    }
}
