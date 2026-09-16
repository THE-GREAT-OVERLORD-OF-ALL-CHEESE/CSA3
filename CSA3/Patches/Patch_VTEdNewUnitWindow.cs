using CheeseMods.CSA3Components;
using HarmonyLib;
using UnityEngine;

namespace CheeseMods.CSA3.Patches;

[HarmonyPatch(typeof(VTEdNewUnitWindow), nameof(VTEdNewUnitWindow.GenerateImage))]
public class Patch_VTEdNewUnitWindow_GenerateImage
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

[HarmonyPatch(typeof(VTEdNewUnitWindow), nameof(VTEdNewUnitWindow.SelectUnit))]
public class Patch_VTEdNewUnitWindow_SelectUnit
{
    public static bool Prefix(VTEdNewUnitWindow __instance, int idx)
    {
        if (idx >= 0 && idx == __instance.selectedUnitIdx && Time.time - __instance.lastSelectUnitTime < 0.4f)
        {
            __instance.OkayButton();
            return false;
        }

        __instance.selectedUnitIdx = idx;
        if (idx >= 0)
        {
            // Patch to not run on custom units
            UnitCatalogue.Unit unit = UnitCatalogue.GetUnit(__instance.units[idx]);
            CSA3_CustomObject customObject = AssetLoader.GetCustomObject(CustomObjectType.CustomUnit, unit.prefabName);
            if (customObject is not CSA3_CustomUnit customUnit)
            {
                // This is a stock, run the base function...
                return true;
            }

            // This unit is modded, do all the same stuff but dont NRE out
            __instance.lastSelectUnitTime = Time.time;
            __instance.unitSelectCursor.gameObject.SetActive(true);
            Vector3 localPosition = __instance.unitSelectCursor.localPosition;
            localPosition.y = (float)(-(float)idx) * __instance.unitLineHeight;
            __instance.unitSelectCursor.localPosition = localPosition;
            __instance.okayButton.interactable = true;
            __instance.unitNameText.text = unit.name;
            string text = unit.description;
            __instance.infoBuffer.Clear();

            IUnitInfoProvider[] componentsInChildrenImplementing = customUnit.gameObject.GetComponentsInChildrenImplementing<IUnitInfoProvider>(false);
            for (int i = 0; i < componentsInChildrenImplementing.Length; i++)
            {
                componentsInChildrenImplementing[i].AddUnitInfoToList(__instance.infoBuffer);
            }
            if (__instance.infoBuffer.Count > 0)
            {
                text += "\n";
                foreach (string str in __instance.infoBuffer)
                {
                    text = text + "\n" + str;
                }
            }
            __instance.unitDescriptionText.text = text;
            RenderTexture texture;
            if (__instance.unitImages.TryGetValue(unit.prefabName, out texture))
            {
                __instance.unitDescriptionImage.gameObject.SetActive(true);
                __instance.unitDescriptionImage.texture = texture;
            }
            else
            {
                __instance.unitDescriptionImage.gameObject.SetActive(false);
            }
            __instance.descriptionObj.SetActive(true);
            return false;
        }
        __instance.unitSelectCursor.gameObject.SetActive(false);
        __instance.okayButton.interactable = false;
        __instance.descriptionObj.SetActive(false);

        return false;
    }
}