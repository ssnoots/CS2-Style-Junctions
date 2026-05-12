using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CS2StyleJunctions
{
    public static class HarmonyManager
    {
        public const string HarmonyId = "com.cs2styleJunctions.mod";

        private static bool _patched = false;

        public static void Install()
        {
            if (_patched) return;

            try
            {
                var harmony = new Harmony(HarmonyId);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                _patched = true;

                // Verify what actually got patched. PatchAll fails silently if a
                // [HarmonyPatch(...)] target method can't be resolved (wrong
                // signature, wrong overload, etc). This loop reports the truth.
                var patched = Harmony.GetAllPatchedMethods().ToList();
                Debug.Log($"[CS2SJ] Harmony patches installed. {patched.Count} method(s) globally patched.");

                foreach (var method in patched)
                {
                    var info = Harmony.GetPatchInfo(method);
                    if (info == null) continue;

                    bool oursPrefix = info.Prefixes.Any(p => p.owner == HarmonyId);
                    bool oursPostfix = info.Postfixes.Any(p => p.owner == HarmonyId);
                    bool oursTranspiler = info.Transpilers.Any(p => p.owner == HarmonyId);

                    if (oursPrefix || oursPostfix || oursTranspiler)
                    {
                        Debug.Log($"[CS2SJ]   -> our patch on: {method.DeclaringType?.FullName}.{method.Name}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CS2SJ] Failed to install Harmony patches: {e}");
            }
        }

        public static void Uninstall()
        {
            if (!_patched) return;

            try
            {
                var harmony = new Harmony(HarmonyId);
                harmony.UnpatchAll(HarmonyId);
                _patched = false;
                Debug.Log("[CS2SJ] Harmony patches removed.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CS2SJ] Failed to remove Harmony patches: {e}");
            }
        }
    }
}
