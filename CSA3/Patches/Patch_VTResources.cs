using CheeseMods.CSA3Components;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CheeseMods.CSA3.Patches
{
    [HarmonyPatch(typeof(VTResources), "GetAllStaticObjectPrefabs")]
    class Patch_VTResources_GetAllStaticObjectPrefabs
    {
        [HarmonyPostfix]
        static void Postfix(ref List<VTStaticObject> __result)
        {
            __result.AddRange(AssetLoader.GetAllCustomObjects(CustomObjectType.StaticObject).Select(o => o.gameObject.GetComponent<VTStaticObject>()));
        }
    }

    [HarmonyPatch(typeof(VTResources), "GetStaticObjectPrefab")]
    class Patch_VTResources_GetStaticObjectPrefab
    {
        [HarmonyPrefix]
        static bool Prefix(out GameObject __result, string id)
        {
            ReplacementManager.GetReplacment(ref id);

            if (BaseAssetInfo.baseStaticObjects.Contains(id))
            {
                __result = null;
                return true;
            }

            CSA3_CustomObject customObject = AssetLoader.GetCustomObject(CustomObjectType.StaticObject, id);
            if (customObject is CSA3_StaticObject staticObject)
            {
                __result = staticObject.gameObject;
                return false;
            }

            customObject = AssetLoader.GetCustomObject(CustomObjectType.StaticObject, Consts.staticObjectFailsafeId);
            if (customObject is CSA3_StaticObject staticObject2)
            {
                __result = staticObject2.gameObject;
                return false;
            }

            Debug.LogError("Bro, whos deleting my failsafe prefabs???");
            __result = null;
            return true;
        }
    }
}
