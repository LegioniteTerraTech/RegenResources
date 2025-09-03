using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraTechETCUtil;
using UnityEngine;

namespace RegenResources
{
    internal static class DebugRegRes
    {
        internal static bool NotErrored = true;

        internal static bool ShouldLog = true;
        internal static bool DoLogInfos = false;
#if DEBUG
        private static bool LogDev = true;
#else
        private static bool LogDev = false;
#endif

        internal static void Info(string message)
        {
            if (!ShouldLog || !DoLogInfos)
                return;
            UnityEngine.Debug.Log(message);
        }
        internal static void Log(string message)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log(message);
        }
        internal static void LogWarnPlayerOnce(string message, Exception e)
        {
            if (!ShouldLog)
                return;
            if (NotErrored)
            {
                ManUI.inst.ShowErrorPopup("ERROR with " + KickStart.ModID + "\n" + message + "\nContinue with caution");
                NotErrored = false;
            }
            UnityEngine.Debug.Log(KickStart.ModID + ": " + message + e);
        }
        internal static void Log(Exception e)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log(e);
        }

        internal static void Assert(string message)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void Assert(bool shouldAssert, string message)
        {
            if (!ShouldLog || !shouldAssert)
                return;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void LogError(string message)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void LogDevOnly(string message)
        {
            if (!LogDev)
                return;
            UnityEngine.Debug.Log(message);
        }
        internal static void LogDevOnlyAssert(string message)
        {
            if (!LogDev)
                return;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void Exception(string message)
        {
            throw new Exception(KickStart.ModID + ": Exception - ", new Exception(message));
        }
        private static List<string> warning = new List<string>();
        private static bool postStartup = false;
        private static bool seriousError = false;
        internal static void ErrorReport(string Warning)
        {
            warning.Add(Warning);
            Debug.Log(KickStart.ModID + ": Error happened " + Warning + " - " + StackTraceUtility.ExtractStackTrace());
            seriousError = true;
        }
        internal static void Warning(string Warning)
        {
            Debug.Log(KickStart.ModID + ": Warning happened " + Warning + " - " + StackTraceUtility.ExtractStackTrace());
            warning.Add(Warning);
        }
        internal static void DoShowWarnings()
        {
            if (warning.Any())
            {
                foreach (var item in warning)
                {
                    ManUI.inst.ShowErrorPopup("Adv.AI: " + item);
                }
                warning.Clear();
                if (!postStartup && seriousError)
                {
                    Debug.Log(KickStart.ModID + ": Error happened " + StackTraceUtility.ExtractStackTrace());
                    if (TerraTechETCUtil.MassPatcher.CheckIfUnstable())
                        ManUI.inst.ShowErrorPopup(KickStart.ModID + ": Error happened on Unstable Branch.  If the issue persists, switch back to Stable Branch.");
                    else
                        ManUI.inst.ShowErrorPopup(KickStart.ModID + ": Error happened during startup!  Advanced AI might not work correctly.");
                }
                seriousError = false;
            }
            postStartup = true;
        }
        internal static void FatalError()
        {
            ManUI.inst.ShowErrorPopup(KickStart.ModID + ": ENCOUNTERED CRITICAL ERROR");
            Assert(StackTraceUtility.ExtractStackTrace());
            UnityEngine.Debug.Log(KickStart.ModID + ": ENCOUNTERED CRITICAL ERROR");
            UnityEngine.Debug.Log(KickStart.ModID + ": MAY NOT WORK PROPERLY AFTER THIS ERROR, PLEASE REPORT!");
        }
        internal static void FatalError(string e)
        {
            ManUI.inst.ShowErrorPopup(KickStart.ModID + ": ENCOUNTERED CRITICAL ERROR: " + e);
            Assert(e);
            UnityEngine.Debug.Log(KickStart.ModID + ": ENCOUNTERED CRITICAL ERROR");
            UnityEngine.Debug.Log(KickStart.ModID + ": MAY NOT WORK PROPERLY AFTER THIS ERROR, PLEASE REPORT!");
        }
        private static int count = -1;
        internal static void EndlessLoopBreaker()
        {
            if (count == -1)
            {
                InvokeHelper.InvokeSingleRepeat(() => { count = 0; }, 0.001f);
                count = 0;
            }
            if (count > 30)
            {
                throw new InvalidOperationException("Endless loop!");
            }
        }
    }
}
