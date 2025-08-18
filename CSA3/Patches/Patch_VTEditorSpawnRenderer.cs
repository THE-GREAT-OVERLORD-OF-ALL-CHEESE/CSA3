using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CheeseMods.CSA3.Patches;

// Stolen from original csa, vtol uses Resources.Load and not the VTResources one >:~(
[HarmonyPatch(typeof(VTEditorSpawnRenderer), nameof(VTEditorSpawnRenderer.Start))]
class Patch_VTEditorSpawnRenderer
{
    [HarmonyPrefix]
    public static bool Prefix(VTEditorSpawnRenderer __instance)
    {
	    UnitSpawner spawner = __instance.GetComponent<UnitSpawner>();
	    UnitCatalogue.Unit unit = UnitCatalogue.GetUnit(spawner.unitID);
	    
		if (BaseAssetInfo.baseUnits.Contains(unit.prefabName))
			return true;

		__instance.spawner = spawner;
		
		Material mat = new Material(Shader.Find("Particles/MF-Alpha Blended"));
		__instance.mat = mat;
		
		Color colour = ((spawner.team == Teams.Allied) ? Color.green : Color.red);
		colour.a = 0.06f;
		__instance.unitColor = colour;
		mat.SetColor("_TintColor", colour);
		__instance.unit = unit;

		Debug.Log("Setup colour.");

		bool flag = false;
		GameObject gameObject = VTResources.Load<GameObject>(unit.resourcePath);
		MeshFilter[] componentsInChildren = gameObject.GetComponentsInChildren<MeshFilter>();
		Vector3 pos = -gameObject.transform.position;
		Quaternion q = Quaternion.Inverse(gameObject.transform.rotation);
		Matrix4x4 lhs = Matrix4x4.TRS(pos, q, Vector3.one);
		List<Mesh> list = new List<Mesh>();
		List<Matrix4x4> list2 = new List<Matrix4x4>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].GetComponent<MeshRenderer>().enabled && componentsInChildren[i].sharedMesh != null && !componentsInChildren[i].gameObject.name.ToLower().Contains("lod"))
			{
				flag = true;
				list.Add(componentsInChildren[i].sharedMesh);
				Matrix4x4 item = lhs * componentsInChildren[i].transform.localToWorldMatrix;
				list2.Add(item);
			}
		}
		List<Mesh> bakedMeshes = new List<Mesh>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			flag = true;
			Mesh mesh = new Mesh();
			skinnedMeshRenderer.BakeMesh(mesh);
			list.Add(mesh);
			bakedMeshes.Add(mesh);
			Matrix4x4 item2 = lhs * skinnedMeshRenderer.transform.localToWorldMatrix;
			list2.Add(item2);
		}

		__instance.bakedMeshes = bakedMeshes;
		__instance.meshes = list.ToArray();
		__instance.matrices = list2.ToArray();

		Debug.Log("Setup meshes.");

		GameObject gameObject2 = new GameObject("Sprite");
		gameObject2.transform.parent = __instance.transform;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localScale = Vector3.one;
		SpriteRenderer sprite = gameObject2.AddComponent<SpriteRenderer>();
		__instance.sprite = sprite;
		VTScenarioEditor.EditorSprite edSprite;
		if (!string.IsNullOrEmpty(unit.editorSprite))
		{
			edSprite = __instance.editor.GetSprite(unit.editorSprite);
		}
		else
		{
			edSprite = __instance.editor.defaultSprite;
		}

		__instance.edSprite = edSprite;
		sprite.sprite = edSprite.sprite;
		sprite.color = edSprite.color;
		sprite.sharedMaterial = __instance.editor.spriteMaterial;
		IconScaleTest iconScaleTest = gameObject2.AddComponent<IconScaleTest>();
		iconScaleTest.maxDistance = __instance.editor.spriteMaxDist;
		iconScaleTest.applyScale = true;
		iconScaleTest.directional = false;
		iconScaleTest.faceCamera = true;
		iconScaleTest.scale = edSprite.size * __instance.editor.globalSpriteScale;
		iconScaleTest.cameraUp = true;
		iconScaleTest.updateRoutine = true;
		iconScaleTest.enabled = false;
		iconScaleTest.enabled = true;

		Debug.Log("Setup sprites.");

		GameObject gameObject3 = new GameObject("Label");
		TextMesh textMesh = gameObject3.AddComponent<TextMesh>();
		textMesh.text = spawner.GetUIDisplayName();
		gameObject3.transform.parent = gameObject2.transform;
		gameObject3.transform.localPosition = new Vector3(0f, 0.062f / iconScaleTest.scale, 0f);
		gameObject3.transform.localRotation = Quaternion.identity;
		textMesh.fontSize = __instance.editor.iconLabelFontSize;
		gameObject3.transform.localScale = 0.035f / iconScaleTest.scale * Vector3.one;
		textMesh.anchor = TextAnchor.LowerCenter;
		textMesh.color = sprite.color;
		__instance.nameText = textMesh;
		//__instance.editor.OnScenarioObjectsChanged += __instance.Editor_OnScenarioObjectsChanged;
		__instance.SetupMouseDowns();
		if (!flag)
		{
			__instance.enabled = false;
		}

		Debug.Log("Setup text.");

		return false;
    }
}