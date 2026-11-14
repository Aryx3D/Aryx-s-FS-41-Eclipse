using HarmonyLib;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aryx_NavalInterceptor1
{
    [HarmonyPatch(typeof(Hangar), nameof(Hangar.TrySpawnAircraft))]
    internal static class HangarTrySpawnAircraftPatch
    {
        private static bool Prefix(Hangar __instance, Player player, ref Airbase.TrySpawnResult __result)
        {
            // AI deployment passes no player.
            if (player != null)
                return true;
            
            if (!__instance.TryGetComponent<AryxPreventHangarAISpawn>(out _))
                return true;
            
            __result = default;
            return false;
        }
    }
}
