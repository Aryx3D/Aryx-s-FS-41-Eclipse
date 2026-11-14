using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aryx_NavalInterceptor1
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded.");

            string modRoot = Path.GetDirectoryName(Info.Location);
            AryxCatapultPatchRuntime.LoadOnce(modRoot, ".catapults", includeSubdirectories: true);

            Harmony harmony = new Harmony("com.aryx.interceptor");
            harmony.PatchAll();
            //DiscoverCatapultConfigs();

        }

        internal static class AryxLandingGearBreakTracker
        {
            private static readonly ConditionalWeakTable<Aircraft, object> BrokenAircraft = new ConditionalWeakTable<Aircraft, object>();
            private static readonly object BrokenMarker = new object();

            public static bool HasBrokenGear(Aircraft aircraft)
            {
                return aircraft != null && BrokenAircraft.TryGetValue(aircraft, out _);
            }

            public static void Clear(Aircraft aircraft)
            {
                if (aircraft != null)
                    BrokenAircraft.Remove(aircraft);
            }

            private static void RegisterBreak(Aircraft aircraft, LandingGear gear)
            {
                if (aircraft == null || gear == null)
                    return;

                gear.onGearBreak += _ => OnGearBroken(aircraft);
            }

            private static void OnGearBroken(Aircraft aircraft)
            {
                if (aircraft == null)
                    return;

                BrokenAircraft.GetValue(aircraft, _ => BrokenMarker);

            }

            [HarmonyPatch(typeof(LandingGear), "Awake")]
            private static class LandingGearAwakePatch
            {
                private static void Postfix(LandingGear __instance, Aircraft ___aircraft)
                {
                    RegisterBreak(___aircraft, __instance);
                }
            }
        }
    }
}
