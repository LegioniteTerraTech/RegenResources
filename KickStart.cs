using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DevCommands;
using HarmonyLib;
using TerraTechETCUtil;

namespace RegenResources
{
    public static class RegenCommands
    {
        /*
        [DevCommand(Name = KickStart.ModCommandID + ".Regrow", Access = Access.Cheat, Users = User.Host)]
        public static CommandReturn ForceRegrowAllResNodes()
        {
            KickStart.CheatRespawnAll(false);
            return new CommandReturn
            {
                message = "All destroyed resource nodes on loaded tiles have been force regrown",
                success = true,
            };
        }*/
        [DevCommand(Name = KickStart.ModCommandID + ".RegrowAll", Access = Access.Public, Users = User.Host)]
        public static CommandReturn ForceRegrowAllResNodes2()
        {
            KickStart.CheatRespawnAll(true);
            return new CommandReturn
            {
                message = "All destroyed and force-removed resource nodes on loaded tiles have been force regrown",
                success = true,
            };
        }
    }
    public class KickStartRegenResources : ModBase
    {
        public override bool HasEarlyInit() => false;
        public override void EarlyInit() { }

        public override void Init()
        {
            try
            {
                KickStart.MainOfficialInit();
            }
            catch (Exception e)
            {
                DebugRegRes.ErrorReport("Init()", e);
            }
        }
        public override void DeInit()
        {
            try
            {
                KickStart.Deinit();
            }
            catch (Exception e)
            {
                DebugRegRes.ErrorReport("DeInit()", e);
            }
        }


        public override void FixedUpdate()
        {
        }
        public override void Update()
        {
            DebugRegRes.DoShowWarnings();
        }
    }
    internal class KickStart
    {
        internal const string ModID = "RegenResources";
        internal const string ModCommandID = ModID;

        internal static bool MakeInfinite = false;
        internal static bool ForceRapidRespawnTest = true;
        internal static float RapidRespawnDelay = 2f;
        internal static Harmony harmonyInstance = new Harmony("legionite." + ModID.ToLower());

        
        internal static FieldInfo rmvFromWorld = typeof(ResourceDispenser).GetField("m_RemovedFromWorld", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo noRegrow = typeof(ResourceDispenser).GetField("m_DontRegrow", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo regrowTimeOverride = typeof(ResourceDispenser).GetField("m_RegrowTimeOverride", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo timeTillRegrow = typeof(ResourceDispenser).GetField("m_SleepingRegrowDelay", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static MethodInfo growIMMEDEATELY = typeof(ResourceDispenser).GetMethod("Regrow", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void MainOfficialInit()
        {
            ModStatusChecker.EncapsulateSafeInit(ModID, Init);
        }
        private static bool patched = false;
        private static void Init()
        {
            //ManMods.inst.ModSessionLoadCompleteEvent.Subscribe(DelayedInit_LEGACY);
            MakeInfinite = true;
            DebugRegRes.Log(ModID + ".Init()");
            if (patched)
                return;
            patched = true;
            if (rmvFromWorld == null)
                throw new NullReferenceException(nameof(rmvFromWorld));
            if (noRegrow == null)
                throw new NullReferenceException(nameof(noRegrow));
            if (regrowTimeOverride == null)
                throw new NullReferenceException(nameof(regrowTimeOverride));
            if (timeTillRegrow == null)
                throw new NullReferenceException(nameof(timeTillRegrow));
            if (growIMMEDEATELY == null)
                throw new NullReferenceException(nameof(growIMMEDEATELY));

            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            DebugRegRes.Log(ModID + ".Patched");
        }
        internal static void Deinit()
        {
            DebugRegRes.Log(ModID + ".Deinit()");
            MakeInfinite = false;
            //ManMods.inst.ModSessionLoadCompleteEvent.Unsubscribe(DelayedInit_LEGACY);
        }
        internal static void CheatRespawnAll(bool forceAllReexist)
        {
            foreach (var tile in ManWorld.inst.TileManager.IterateTiles(WorldTile.State.Populated))
            {
                if (tile?.Visibles != null && tile.Visibles[(int)ObjectTypes.Scenery] != null)
                {
                    foreach (var pair in tile.Visibles[(int)ObjectTypes.Scenery])
                    {
                        var vis = pair.Value;
                        ResourceDispenser RD = vis?.resdisp;
                        if (RD != null && !vis.isActive)
                        {
                            if (RD.IsDeactivated && !forceAllReexist)
                                continue;
                            try
                            {
                                RD.SetAwake(true);
                                rmvFromWorld.SetValue(RD, false);
                                growIMMEDEATELY.Invoke(RD, Array.Empty<object>());
                            }
                            catch (Exception e)
                            {
                                DebugRegRes.ErrorReport("ResourceDispenser.CheatRespawnAll()", e);
                            }
                        }
                    }
                }
            }
        }
        [Obsolete]
        private static void DelayedInit_LEGACY()
        {
            foreach (var diction in SpawnHelper.IterateSceneryTypes())
            {
                if (diction == null)
                    continue;
                foreach (var pair in diction)
                {
                    if (pair.Value == null)
                        continue;
                    foreach (var terraObj in pair.Value)
                    {
                        if (terraObj == null)
                            continue;
                        ResourceDispenser RD = terraObj.GetComponent<ResourceDispenser>();
                        if (RD == null)
                            continue;
                        try
                        {
                            if (RD.AllDispensableItems().Any() && !(bool)noRegrow.GetValue(RD))
                            {
                                DebugRegRes.Log("Made " + (RD.name != null ? RD.name : "<NULL>") + " renewable");
                                noRegrow.SetValue(RD, false);
                                if (ForceRapidRespawnTest)
                                    timeTillRegrow.SetValue(RD, 2f);

                            }
                        }
                        catch { } // Does not dispence - do not set!!!
                    }
                }
            }
        }
    }
}
