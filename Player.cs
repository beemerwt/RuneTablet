using HarmonyLib;

namespace RuneTablet
{
    [HarmonyPatch(typeof(Player))]
    public class PlayerPatches {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.Save))]
        public static void SavePostfix()
        {
            if (Player.m_localPlayer == null)
                return;

            Plugin.SaveToDisk();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.Load))]
        public static void LoadPostfix()
        {
            if (Player.m_localPlayer == null)
                return;

            Plugin.LoadFromDisk();
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
            {
                InventoryGui.instance.Hide();
                inventory.RemoveItem(item);
            }

            return false;
        }
    }
}
