using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace CheeseMods.CSA3.Patches;

[HarmonyPatch(typeof(VTEdNewUnitWindow), nameof(VTEdNewUnitWindow.GenerateImage))]
public class Patch_VTEdNewUnitWindow
{
    public static bool Prefix(VTEdNewUnitWindow __instance, UnitCatalogue.Unit unit)
    {
        if (BaseAssetInfo.baseUnits.Contains(unit.prefabName))
            return true;

        if (__instance.unitImages.ContainsKey(unit.prefabName))
            return false;

        RenderTexture rt = new RenderTexture(8, 8, 0, RenderTextureFormat.Default);
        
        __instance.unitImages.Add(unit.prefabName, rt);
        return false;
    }
}