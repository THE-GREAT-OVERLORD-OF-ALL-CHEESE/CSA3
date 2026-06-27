using System;
using CheeseMods.CSA3Components;
using HarmonyLib;
using ReplaySystem;
using UnityEngine;
using VTOLVR.ReplaySystem;
using Object = UnityEngine.Object;

namespace CheeseMods.CSA3.Patches;

[HarmonyPatch(typeof(ReplayPlayback))]
public class Patch_ReplayPlayback
{
    [HarmonyPatch(nameof(ReplayPlayback.BeginPlayback))]
    [HarmonyPrefix]
    private static void BeginPlaybackPrefix(ReplayPlayback __instance)
    {
        Debug.Log("Beginning ReplayPlayback.BeginPlayback Prefix Patch");
        foreach (var customObject in AssetLoader.GetAllCustomObjects(CustomObjectType.CustomUnit))
        {
            var unit = customObject as CSA3_CustomUnit;
            GameObject replayPrefab;
            try
            {
                replayPrefab = unit?.replayModelPrefab;
                if (replayPrefab == null)
                {//that means it will be using a plain model and not anything custom
                    continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                continue;
            }
            var gameObject = Object.Instantiate(replayPrefab);
            gameObject.SetActive(false);
            
            var modelOverride = new ReplayPlayback.EntityModelOverride
            {
                identities = [unit.customUnitTargetIndex],
                overridePrefab = gameObject
            };
            __instance.modelOverrides.Add(modelOverride);
        }
    }
}