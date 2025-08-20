using CheeseMods.CSA3Components;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CheeseMods.CSA3.Patches
{
    [HarmonyPatch(typeof(VTMapEdResources), "GetAllCategories")]
    class Patch_VTMapEdResources_GetAllStaticObjectPrefabs
    {
        [HarmonyPostfix]
        static void Postfix(ref List<string> __result)
        {
            if (BaseAssetInfo.disableModdedObjects)
            {
                return;
            }

            __result.AddRange(AssetLoader.GetAllCustomObjects(CustomObjectType.MapObject).Select(o => o.gameObject.GetComponent<VTMapEdPrefab>().category).Distinct());

            __result = __result.Distinct().ToList();
        }
    }

    [HarmonyPatch(typeof(VTMapEdResources), "GetPrefabs")]
    class Patch_VTMapEdResources_GetPrefabs
    {
        [HarmonyPostfix]
        static void Postfix(ref VTMapEdPrefab[] __result, string category)
        {
            if (BaseAssetInfo.disableModdedObjects)
            {
                return;
            }

            List<VTMapEdPrefab> prefabs = new List<VTMapEdPrefab>();
            if (__result != null)
            {
                prefabs.AddRange(__result);
            }

            prefabs.AddRange(AssetLoader.GetAllCustomObjects(CustomObjectType.MapObject)
                .Where(o => o.gameObject.GetComponent<VTMapEdPrefab>().category == category)
                .Select(o => o.gameObject.GetComponent<VTMapEdPrefab>()));

            __result = prefabs.ToArray();
        }
    }

    [HarmonyPatch(typeof(VTMapEdResources), "GetPrefab")]
    class Patch_VTMapEdResources_GetPrefab
    {
        [HarmonyPrefix]
        static bool Prefix(out VTMapEdPrefab __result, string id)
        {
            if (BaseAssetInfo.disableModdedObjects)
            {
                __result = null;
                return true;
            }

            ReplacementManager.GetReplacment(ref id);

            if (BaseAssetInfo.baseMapObjects.Contains(id))
            {
                __result = null;
                return true;
            }

            CSA3_CustomObject customObject = AssetLoader.GetCustomObject(CustomObjectType.MapObject, id);
            if (customObject is CSA3_MapObject mapObject)
            {
                __result = mapObject.gameObject.GetComponent<VTMapEdPrefab>();
                return false;
            }

            customObject = AssetLoader.GetCustomObject(CustomObjectType.MapObject, Consts.mapObjectFailsafeId);
            if (customObject is CSA3_MapObject mapObject2)
            {
                __result = mapObject2.gameObject.GetComponent<VTMapEdPrefab>();
                return false;
            }

            Debug.LogError("Bro, whos deleting my failsafe prefabs???");
            __result = null;
            return true;
        }
    }
}
