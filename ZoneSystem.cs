using BepInEx.Logging;
using HarmonyLib;
using Jotunn;
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
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
    public class ZoneSystemSpawnLocationPatch
    {
        public static void Postfix(ZoneSystem.ZoneLocation location, int seed, Vector3 pos, GameObject __result)
        {
            if (location == null)
            {
                Plugin.Logger.LogWarning("ZoneSystem.SpawnLocation: location was null");
                return;
            }

            var containsName = false;
            foreach (var name in RuneStones.Prefabs) {
                if (location.m_prefabName == name)
                {
                    containsName = true;
                    break;
                }
            }

            if (!containsName)
                return;

            Plugin.Logger.LogInfo("ZoneSystem.SpawnLocation: RuneStone Location Spawned.");
            GameObject loadedObject = null;

            if (__result != null)
            {
                var runestone = __result.GetComponentInChildren<RuneStone>();
                if (runestone != null)
                    loadedObject = runestone.gameObject;
            }

            if (loadedObject == null)
            {
                Plugin.Logger.LogWarning("ZoneSystem.SpawnLocation: __result was null");
                loadedObject = RuneStones.GetRuneStoneOfLocation(pos);
            }

            Plugin.Logger.LogInfo("ZoneSystem.SpawnLocation: Found runestone");

            // TODO: make it so we replace the stone with our new piece

            var destructible = loadedObject.GetComponent<Destructible>();
            if (destructible == null)
            {
                Plugin.Logger.LogWarning("ZoneSystem.SpawnLocation: Destructible is null");
                return;
            }

            /*
             var initZdo = ZDOMan.instance.CreateNewZDO(
                pos, location.m_prefabName.GetStableHashCode());
            ZNetView.m_initZDO = initZdo;
            var znetView = loadedObject.AddComponent<ZNetView>();
            if (znetView == null)
            {
                Plugin.Logger.LogWarning("ZoneSystem.SpawnLocation: ZNetView is null");
                return;
            }
            */

            //destructible.m_nview = znetView;
            //destructible.Awake();
            Plugin.Logger.LogInfo("ZoneSystem.SpawnLocation: Added destructible to runestone");
        }

    }
}
