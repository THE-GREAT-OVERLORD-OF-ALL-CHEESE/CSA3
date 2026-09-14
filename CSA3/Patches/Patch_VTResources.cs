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
            if (BaseAssetInfo.disableModdedObjects)
            {
                return;
            }

            __result.AddRange(AssetLoader.GetAllCustomObjects(CustomObjectType.StaticObject).Select(o => o.gameObject.GetComponent<VTStaticObject>()));
        }
    }

    [HarmonyPatch(typeof(VTResources), "GetAllStaticObjectInfos")]
    class Patch_VTResources_GetAllStaticObjectInfos
    {
        [HarmonyPostfix]
        static void Postfix(List<VTStaticObjectInfo> inoutList)
        {
            if (BaseAssetInfo.disableModdedObjects)
            {
                return;
            }

            foreach (CSA3_CustomObject customObject in AssetLoader.GetAllCustomObjects(CustomObjectType.StaticObject))
            {
                CSA3_StaticObject staticObject = customObject as CSA3_StaticObject;

                Debug.Log(staticObject.name);
                VTStaticObjectInfo info = new VTStaticObjectInfo
                {
                    itemType = VTStaticObjectInfo.ItemTypes.BuiltIn,
                    id = staticObject.name,
                    name = staticObject.displayName,
                    description = staticObject.description,
                    category = "CSA",
                    editorOnly = false,
                    mpOnly = false,
                };
                inoutList.Add(info);
            }
        }
    }

    [HarmonyPatch(typeof(VTResources), "GetStaticObjectPrefab")]
    class Patch_VTResources_GetStaticObjectPrefab
    {
        [HarmonyPrefix]
        static bool Prefix(out GameObject __result, string id)
        {
            if (BaseAssetInfo.disableModdedObjects)
            {
                __result = null;
                return true;
            }

            ReplacementManager.GetReplacment(ref id);

            if (BaseAssetInfo.baseStaticObjects.Contains(id))
            {
                // Asset was a base game
                __result = null;
                return true;
            }
            VTResources.LoadStaticObjectPrefabs();
            if (VTResources.vteditStaticObjectInfos.ContainsKey(id))
            {
                // Asset was a CSO
                __result = null;
                return true;
            }

            CSA3_CustomObject customObject = AssetLoader.GetCustomObject(CustomObjectType.StaticObject, id);
            if (customObject is CSA3_StaticObject staticObject)
            {
                // CSA3 Static Object
                __result = staticObject.gameObject;
                return false;
            }

            customObject = AssetLoader.GetCustomObject(CustomObjectType.StaticObject, Consts.staticObjectFailsafeId);
            if (customObject is CSA3_StaticObject staticObject2)
            {
                // Failsafe
                __result = staticObject2.gameObject;
                return false;
            }

            Debug.LogError("Bro, whos deleting my failsafe prefabs???");
            __result = null;
            return true;
        }
    }
}
