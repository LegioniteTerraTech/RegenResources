using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DevCommands;
using TerraTechETCUtil;
using static ManDebugMenu;
using static TerraTechETCUtil.ManIngameWiki;

namespace RegenResources
{
    public static class AICommands
    {
        [DevCommand(Name = KickStart.ModCommandID + ".RegrowAll", Access = Access.Cheat, Users = User.Host)]
        public static CommandReturn ForceRegrowAllResNodes()
        {
            KickStart.CheatRespawnAll();
            return new CommandReturn
            {
                message = "All destroyed resource nodes have been force regrown",
                success = true,
            };
        }
    }
    public class KickStart
    {
        internal const string ModID = "RegenResources";
        internal const string ModCommandID = "TAC_AI";

        private static bool ForceRapidRespawnTest = true;

        private static FieldInfo growBlocked = typeof(ResourceDispenser).GetField("m_DontRegrow", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo growTime = typeof(ResourceDispenser).GetField("m_SleepingRegrowDelay", BindingFlags.Instance | BindingFlags.NonPublic);
        public static void MainOfficialInit()
        {
            ModStatusChecker.EncapsulateSafeInit(ModID, Init);
        }
        private static void Init()
        {
            ManMods.inst.ModSessionLoadCompleteEvent.Subscribe(DelayedInit);
        }
        private static MethodInfo growNOW = typeof(ResourceDispenser).GetMethod("Regrow", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static void CheatRespawnAll()
        {
            foreach (var tile in ManWorld.inst.TileManager.IterateTiles(WorldTile.State.Populated))
            {
                if (tile?.m_ResourcesOnTile != null)
                {
                    foreach (var pair in tile.m_ResourcesOnTile)
                    {
                        if (pair.Value == null)
                            continue;
                        foreach (var vis in pair.Value)
                        {
                            if (vis != null && !vis.isActive)
                            {
                                ResourceDispenser RD = vis.resdisp;
                                if (RD != null)
                                {
                                    try
                                    {
                                        RD.SetAwake(true);
                                        growNOW.Invoke(RD, Array.Empty<object>());
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void DelayedInit()
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
                            if (RD.AllDispensableItems().Any() && !(bool)growBlocked.GetValue(RD))
                            {
                                DebugRegRes.Log("Made " + (RD.name != null ? RD.name : "<NULL>") + " renewable");
                                growBlocked.SetValue(RD, false);
                                if (ForceRapidRespawnTest)
                                    growTime.SetValue(RD, 2f);

                            }
                        }
                        catch { } // Does not dispence - do not set!!!
                    }
                }
            }
        }
    }
}
