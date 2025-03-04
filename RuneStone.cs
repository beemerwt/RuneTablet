using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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


    public static class RuneStones
    {
        // There are two different kinds of prefabs
        // the lowercase "stone" one is the one that gets placed in the world
        public static string[] Prefabs = [
            "Runestone_Ashlands",
            "Runestone_BlackForest",
            "Runestone_Boars",
            "Runestone_Draugr",
            "Runestone_Greydwarfs",
            "Runestone_Meadows",
            "Runestone_Mistlands",
            "Runestone_Mountains",
            "Runestone_Plains",
            "Runestone_Swamps",
            "DrakeLorestone",
        ];

        public static GameObject GetRuneStoneOfLocation(Vector3 pos)
        {
            Collider[] objects = Physics.OverlapSphere(pos, 12);
            foreach (var obj in objects)
            {
                var runestone = obj.GetComponent<RuneStone>();
                if (runestone != null)
                    return runestone.gameObject;
            }

            return null;
        }

        public static void RegisterPrefabs(GameObject itemDrop)
        {
            foreach (var prefab in Prefabs)
            {
                var runestone = PrefabManager.Instance.GetPrefab(prefab);
                if (runestone == null)
                {
                    Plugin.Logger.LogWarning($"RuneStone {prefab} not found");
                    continue;
                }

                var stone = runestone.GetComponentInChildren<RuneStone>();
                if (stone != null)
                {
                    Plugin.Logger.LogInfo($"RuneStone {prefab} has a RuneStone component");
                    runestone = stone.gameObject;
                }

                var znetView = runestone.AddComponent<ZNetView>();

                if (ZNetView.m_forceDisableInit || ZDOMan.instance == null)
                {
                    Plugin.Logger.LogWarning("ZNetView.m_forceDisableInit is true or ZDOMan.instance is null");
                    continue;
                }

                var destructible = runestone.AddComponent<Destructible>();
                destructible.m_spawnWhenDestroyed = itemDrop;
                destructible.m_health = 1000.0f;
                destructible.m_destroyed = false;
                destructible.m_damages.m_chop = HitData.DamageModifier.Ignore;
                destructible.m_damages.m_pickaxe = HitData.DamageModifier.Weak;
                destructible.m_damages.m_fire = HitData.DamageModifier.Ignore;
                destructible.m_damages.m_frost = HitData.DamageModifier.Ignore;
                destructible.m_damages.m_lightning = HitData.DamageModifier.Ignore;
                destructible.m_damages.m_pierce = HitData.DamageModifier.Ignore;
                destructible.m_damages.m_poison = HitData.DamageModifier.Ignore;
                destructible.m_damages.m_slash = HitData.DamageModifier.Ignore;
                destructible.m_damages.m_spirit = HitData.DamageModifier.Ignore;
                destructible.m_damages.m_blunt = HitData.DamageModifier.Ignore;
            }
        }

        public static void RegisterLocations()
        {
            List<ZoneSystem.LocationInstance> instances = [];
            if (!ZoneSystem.instance.FindLocations("Runestone_Meadows", ref instances))
            {
                Plugin.Logger.LogWarning("Runestone_Meadows not found");
                return;
            }

            /*
            foreach (var instance in instances)
            {
                Collider[] colliders = Physics.OverlapSphere(instance.m_position, 12);

            }
            */
        }
    }
}
