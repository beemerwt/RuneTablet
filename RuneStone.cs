using HarmonyLib;
using Jotunn.Managers;

namespace RuneTablet
{
    [HarmonyPatch(typeof(RuneStone), nameof(RuneStone.Interact))]
    public class RuneStoneInteractPatch
    {
        public static void Postfix(Humanoid character, bool hold, bool alt, RuneStone __instance)
        {
            Plugin.Logger.LogInfo("RuneStone.Interact: Called");
            var name = Helper.GetHashedName(__instance);

            Plugin.Logger.LogInfo("RuneStone.Interact: Found runestone " + name);

            if (Plugin.FoundRuneStones.Contains(name))
                return;

            // Spawn a rune crystal for the player
            var crystalPrefab = PrefabManager.Instance.GetPrefab("RuneCrystal");
            if (character.m_inventory.CanAddItem(crystalPrefab))
            {
                Player.m_localPlayer.m_inventory.AddItem(crystalPrefab, 1);
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "A mysterious crystal falls into your hand.");
                Plugin.FoundRuneStones.Add(name);
            }
        }
    }
}
