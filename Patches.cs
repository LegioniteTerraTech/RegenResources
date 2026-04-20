using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace RegenResources
{
    internal static class Patches
    {
        [HarmonyPatch(typeof(ResourceDispenser))]
        [HarmonyPatch("Regrow")]
        [HarmonyPriority(-9001)]
        internal static class ResourceDispenserInfinityPatches0
        {
            internal static void Prefix(ResourceDispenser __instance)
            {
                try
                {
                    if (KickStart.MakeInfinite)
                        FbroningExist(__instance);
                }
                catch (Exception e)
                {
                    DebugRegRes.ErrorReport("ResourceDispenser.Regrow()", e);
                } // Does not dispence - do not set!!!
            }
        }
        [HarmonyPatch(typeof(ResourceDispenser))]
        [HarmonyPatch("Deactivate")]
        [HarmonyPriority(-9001)]
        internal static class ResourceDispenserInfinityPatches1
        {
            internal static void Prefix(ResourceDispenser __instance)
            {
                try
                {
                    if (KickStart.MakeInfinite)
                        FbroningExist(__instance);
                }
                catch (Exception e)
                {
                    DebugRegRes.ErrorReport("ResourceDispenser.Deactivate()", e);
                } // Does not dispence - do not set!!!
            }
        }
        [HarmonyPatch(typeof(ResourceDispenser))]
        [HarmonyPatch("InitState")]
        [HarmonyPriority(-9001)]
        internal static class ResourceDispenserInfinityPatches2
        {
            internal static void Prefix(ResourceDispenser __instance)
            {
                try
                {
                    DoRespawnable(__instance);
                }
                catch (Exception e)
                {
                    DebugRegRes.ErrorReport("ResourceDispenser.InitState()", e);
                } // Does not dispence - do not set!!!
            }
        }
        [HarmonyPatch(typeof(ResourceDispenser))]
        [HarmonyPatch("TryRestoreSavedState")]
        [HarmonyPriority(-9001)]
        internal static class ResourceDispenserInfinityPatches3
        {
            internal static void Prefix(ResourceDispenser __instance)
            {
                try
                {
                    DoRespawnable(__instance);
                }
                catch (Exception e)
                {
                    DebugRegRes.ErrorReport("ResourceDispenser.TryRestoreSavedState()", e);
                } // Does not dispence - do not set!!!
            }
        }
        
        [HarmonyPatch(typeof(ResourceDispenser))]
        [HarmonyPatch("Restore")]
        [HarmonyPriority(-9001)]
        internal static class ResourceDispenserInfinityPatches4
        {
            internal static void Prefix(ResourceDispenser __instance)
            {
                try
                {
                    DoRespawnable(__instance);
                }
                catch (Exception e)
                {
                    DebugRegRes.ErrorReport("ResourceDispenser.Restore()", e);
                } // Does not dispence - do not set!!!
            }
        }
        private class TrackedResType
        {
            public bool ogNoRespawn;
        }
        private static Dictionary<int, TrackedResType> TypesLim = new Dictionary<int, TrackedResType>();
        private static void FbroningExist(ResourceDispenser __instance)
        {
            __instance.SetAwake(true);
        }
        private static void DoRespawnable(ResourceDispenser __instance)
        {
            var type = __instance?.name;
            if (type == null)
                DebugRegRes.LogDevOnly(KickStart.ModID + ".name is <NULL>");
            if (type == null || ManGameMode.inst.GetCurrentGameType() == ManGameMode.GameType.RaD)
                return;
            if (KickStart.MakeInfinite)
            {   // Make infinite respawn
                if (!TypesLim.TryGetValue(type.GetHashCode(), out var tracked))
                {
                    bool noRegrow = (bool)KickStart.noRegrow.GetValue(__instance);
                    tracked = new TrackedResType()
                    {
                        ogNoRespawn = noRegrow,
                    };
                    TypesLim.Add(type.GetHashCode(), tracked);
                    if (noRegrow)
                        DebugRegRes.LogDevOnly(KickStart.ModID + ".Made " + type + " renewable");
                    if (__instance.AllDispensableItems() != null && __instance.AllDispensableItems().Any())
                        DebugRegRes.LogDevOnly(KickStart.ModID + ".scenery " + type + " has resources");
                    else
                        DebugRegRes.LogDevOnly(KickStart.ModID + ".scenery " + type + " has no resources");
                }
                KickStart.noRegrow.SetValue(__instance, false);
                KickStart.rmvFromWorld.SetValue(__instance, false);
                if (KickStart.ForceRapidRespawnTest)
                {
                    KickStart.regrowTimeOverride.SetValue(__instance, KickStart.RapidRespawnDelay);
                    //KickStart.growTime.SetValue(__instance, KickStart.RapidRespawnDelay);
                }
                else
                {
                    KickStart.regrowTimeOverride.SetValue(__instance, 0f);
                    //KickStart.growTime.SetValue(__instance, tracked.respawnTime);
                }
                FbroningExist(__instance);
            }
            else
            {   // Ignore respawn
                if (TypesLim.TryGetValue(type.GetHashCode(), out var tracked))
                {
                    KickStart.regrowTimeOverride.SetValue(__instance, 0f);
                    KickStart.noRegrow.SetValue(__instance, tracked.ogNoRespawn);
                }
            }
        }
    }
}
