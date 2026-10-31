using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Railroader.Modding;

namespace CollegeEducatedSpouts
{
    /// <summary>
    /// Start-up self-check. The game can move things under us in any update, so before patching, every method and
    /// field the spouts rely on is looked up with the exact signature we use. If a target is missing the mod switches
    /// itself off (in memory only; Settings.xml is not touched) and says why in the log. It does not guess renamed
    /// members, write files or throw: a failure is a log line and a disabled feature.
    /// </summary>
    internal static class SelfCheck
    {
        internal const string GameRequirement = "rr/game >=0.6.0";

        private const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        /// <summary>One line per feature (name and status) from the last check, shown in the settings window.</summary>
        internal static readonly List<string> Report = new List<string>();

        private static readonly string[] Needs =
        {
            "CarLoaderBuiltin.Configure(in BuiltinContext)",
            "CarLoadTargetLoader.LoadLoop() reads transform.position once",
            "CarLoadTargetLoader.LoadSlotFromCar(Car, Vector3, out int)",
            "ClearanceLimitedController.probeOrigin, animationClip, CarLoaderSequencer keys",
        };

        private static readonly Type[] Patches =
        {
            typeof(CarLoaderBuiltin_Configure_Patch),
            typeof(CarLoadTargetLoader_LoadLoop_Patch),
            typeof(CarLoadTargetLoader_LoadSlotFromCar_Patch),
        };

        private static Dictionary<string, Func<bool>> Probes()
        {
            return new Dictionary<string, Func<bool>>
            {
                [Needs[0]] = () => AccessTools.Method(typeof(Game.Builtins.CarLoaderBuiltin), "Configure") != null && AccessTools.Method(typeof(Game.Builtins.CarLoaderBuiltin), "Configure").ReturnType == typeof(bool),
                [Needs[1]] = () => CarLoadTargetLoader_LoadLoop_Patch.PositionReads() == 1,
                [Needs[2]] = () => Method(typeof(RollingStock.CarLoadTargetLoader), "LoadSlotFromCar", new[] { typeof(Model.Car), typeof(UnityEngine.Vector3), typeof(int).MakeByRefType() }, typeof(Model.Definition.Data.LoadSlot), "car", "point", "slotIndex"),
                [Needs[3]] = () => Field(typeof(RollingStock.Controls.ClearanceLimitedController), "probeOrigin", typeof(UnityEngine.Transform)) && Field(typeof(RollingStock.Controls.ClearanceLimitedController), "animationClip", typeof(UnityEngine.AnimationClip))
                    && Field(typeof(RollingStock.CarLoaderSequencer), "readWantsLoadingKey", typeof(string)) && Field(typeof(RollingStock.CarLoaderSequencer), "readIsLoadingKey", typeof(string)),
            };
        }

        /// <summary>Checks, patches and logs. Returns false when nothing was applied.</summary>
        internal static bool Run(IModContext context, Harmony harmony)
        {
            var log = new StringBuilder();
            log.AppendLine($"CollegeEducatedSpouts {Main.Mod.Info.Version} self-check, game {UnityEngine.Application.version}");

            bool gameOk;
            try { gameOk = context.CheckDependency(GameRequirement); }
            catch (Exception e) { gameOk = false; log.AppendLine("  Game version check failed: " + e.Message); }
            if (!gameOk)
            {
                context.Log.Error(log + $"  Needs {GameRequirement}. Nothing was applied.");
                Report.Add("Needs " + GameRequirement + ". Nothing was applied.");
                return false;
            }

            var missing = new List<string>();
            foreach (var probe in Probes())
            {
                bool ok;
                try { ok = probe.Value(); }
                catch (Exception) { ok = false; } // e.g. an ambiguous match counts as not found
                if (!ok) missing.Add(probe.Key);
            }

            string status;
            int applied = 0;
            if (missing.Count > 0)
            {
                status = "DISABLED, not found: " + string.Join(", ", missing);
                Main.Settings.Enabled = false;
            }
            else
            {
                var failed = new List<string>();
                foreach (var patch in Patches)
                {
                    try
                    {
                        harmony.CreateClassProcessor(patch).Patch();
                        applied++;
                    }
                    catch (Exception e)
                    {
                        failed.Add(patch.Name + ": " + (e.InnerException ?? e).Message);
                    }
                }
                if (failed.Count > 0)
                {
                    status = "DISABLED, patch failed: " + string.Join("; ", failed);
                    Main.Settings.Enabled = false;
                }
                else
                {
                    status = Main.Settings.Enabled ? "enabled" : "off (can be switched on in the settings window)";
                }
            }

            log.AppendLine("  Swinging spouts: " + status);
            Report.Add("Swinging spouts: " + status);
            int patched = harmony.GetPatchedMethods().Count();
            log.AppendLine($"  Patched methods: {patched} (expected {applied})");
            log.AppendLine("  Effective settings: " + Main.Settings.Describe());

            string text = log.ToString().TrimEnd();
            if (status.StartsWith("DISABLED") || patched != applied) context.Log.Warning(text);
            else context.Log.Info(text);
            return true;
        }

        // ---- Helpers ----

        /// <summary>Exact match: declared on the type, same parameter types and return type; names checked where the patch binds by name.</summary>
        private static bool Method(Type type, string name, Type[] args, Type returns, params string[] paramNames)
        {
            MethodInfo m = type.GetMethod(name, All, null, args, null);
            if (m == null || m.ReturnType != returns) return false;
            var ps = m.GetParameters();
            for (int i = 0; i < paramNames.Length && i < ps.Length; i++)
            {
                if (paramNames[i] != null && ps[i].Name != paramNames[i]) return false;
            }
            return true;
        }

        private static bool Field(Type type, string name, Type fieldType)
        {
            FieldInfo f = AccessTools.Field(type, name);
            return f != null && (fieldType == null || f.FieldType == fieldType);
        }
    }
}
