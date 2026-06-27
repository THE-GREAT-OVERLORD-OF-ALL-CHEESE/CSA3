using HarmonyLib;

namespace CheeseMods.CSA3.Patches;

[HarmonyPatch(typeof(TargetIdentityManager))]
public class Patch_TargetIdentityManager
{
    [HarmonyPatch(nameof(TargetIdentityManager.GetIdentity))]
    [HarmonyPatch([typeof(int)])]
    [HarmonyPrefix]
    private static bool GetIdentityPrefix(int identityIndex, ref TargetIdentity __result)
    {
        if (identityIndex >= 0 && identityIndex < TargetIdentityManager.indexedIdentities.Count)
        {
            __result = TargetIdentityManager.indexedIdentities.Find(x => x.index == identityIndex) ??
                       TargetIdentityManager.indexedIdentities[0];
        }
        return false;
    }
}