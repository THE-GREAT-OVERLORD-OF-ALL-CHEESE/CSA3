using HarmonyLib;
using ReplaySystem;
using UnityEngine;
using VTOLVR.ReplaySystem;

namespace CheeseMods.CSA3.Patches;

[HarmonyPatch(typeof(ReplayPlaybackUI))]
public class Patch_ReplayPlaybackUI
{
    [HarmonyPatch(nameof(ReplayPlaybackUI.Playback_OnEntitySpawned))]
    [HarmonyPrefix]
    private static bool OnEntitySpawnedPrefix(ReplayPlaybackUI __instance, ReplayRecorder.ReplayEntity entity)
    {
        if (!Actor.IsReplayFollowableEntity(entity.entityType, out var teams) ||
            __instance.followListItems.ContainsKey(entity.id)) return false;
        var gameObject = Object.Instantiate(__instance.followUnitTemplate, __instance.followUnitView.content);
        gameObject.SetActive(true);
        var component = gameObject.GetComponent<RPFollowEntityListItem>();
        component.interactable.OnInteract.AddListener(delegate
        {
            __instance.BeginFollowingEntity(entity);
        });
        component.entityLabel.text = entity.metaData.label;
        component.entityLabel.color = __instance.teamColors[(int)teams];
        component.entityIdentity.text = entity.metaData.label;
        var num = ((RectTransform)__instance.followUnitTemplate.transform).rect.height * __instance.followUnitTemplate.transform.localScale.y;
        gameObject.transform.localPosition = new Vector3(0f, -num * __instance.followableEnts.Count, 0f);
        __instance.followableEnts.Add(entity);
        __instance.followListItems.Add(entity.id, gameObject);
        __instance.followUnitView.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, __instance.followableEnts.Count * num);
        __instance.followUnitView.ClampVertical();
        return false;
    }
}