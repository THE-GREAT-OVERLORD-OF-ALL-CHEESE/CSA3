using CheeseMods.CSA3Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CSA3_ObjectBundle))]
class CSA3_ObjectBundleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Build"))
        {
            CSA3_ObjectBundle bundle = target as CSA3_ObjectBundle;
            if (bundle == null)
            {
                Debug.LogError("What?");
                return;
            }

            if (!bundle.bundleMetadata.authorId.Any())
            {
                EditorUtility.DisplayDialog("Invalid ID", "Author ID is empty, please fill it in!", "Ok");
                return;
            }
            if (!MeetsNameRestrictions(bundle.bundleMetadata.authorId))
            {
                EditorUtility.DisplayDialog("Invalid ID", $"Author ID ({bundle.bundleMetadata.authorId}) doesn't match naming rules. {namingHint}", "Ok");
                return;
            }

            if (!bundle.bundleMetadata.bundleId.Any())
            {
                EditorUtility.DisplayDialog("Invalid ID", "Bundle ID is empty, please fill it in!", "Ok");
                return;
            }
            if (!MeetsNameRestrictions(bundle.bundleMetadata.bundleId))
            {
                EditorUtility.DisplayDialog("Invalid ID", $"Bundle ID ({bundle.bundleMetadata.bundleId}) doesn't match naming rules. {namingHint}", "Ok");
                return;
            }

            if (!bundle.bundleMetadata.authorId.Any())
            {
                EditorUtility.DisplayDialog("Invalid name", "Author field is empty, please fill it in!", "Ok");
                return;
            }
            if (!bundle.bundleMetadata.description.Any())
            {
                EditorUtility.DisplayDialog("Invalid description", "Description field is empty, please fill it in!", "Ok");
                return;
            }

            string abName = $"{bundle.bundleMetadata.authorId}_{bundle.bundleMetadata.bundleId}";
            string abExtension = "csa";

            bundle.bundleMetadata.csa3VersionNumber = CSA3_VersionInfo.CurrentVersion;

            string assetPath = AssetDatabase.GetAssetPath(bundle);
            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(abName, abExtension);

            foreach (CSA3_CustomObject customObject in bundle.customObjects)
            {
                if (!customObject.objectId.Any())
                {
                    EditorUtility.DisplayDialog("Invalid ID", $"Object ID on custom object ({customObject.objectId}) is empty, please fill it in!", "Ok");
                    return;
                }
                if (!MeetsNameRestrictions(customObject.objectId))
                {
                    EditorUtility.DisplayDialog("Invalid ID", $"Object ID ({customObject.objectId}) doesn't match naming rules. {namingHint}", "Ok");
                    return;
                }

                string expectedName = $"{abName}_{customObject.objectId}";
                if (customObject.gameObject.name != expectedName)
                {
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(customObject.gameObject), expectedName);

                    return;
                }

                if (!customObject.displayName.Any())
                {
                    EditorUtility.DisplayDialog("Invalid name", $"Display name field on custom object {customObject.gameObject.name} is empty, please fill it in!", "Ok");
                    return;
                }
                if (!customObject.description.Any())
                {
                    EditorUtility.DisplayDialog("Invalid description", $"Description field on custom object {customObject.gameObject.name} is empty, please fill it in!", "Ok");
                    return;
                }

                string assetPath2 = AssetDatabase.GetAssetPath(customObject);
                AssetImporter.GetAtPath(assetPath2).SetAssetBundleNameAndVariant(abName, abExtension);

                // Load the contents of the Prefab Asset.
                GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath2);

                // Modify Prefab contents.
                contentsRoot.GetComponent<CSA3_CustomObject>().parentBundle = bundle;

                // Save contents back to Prefab Asset and unload contents.
                PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath2);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }

            string assetBundleDirectory = "Assets/AssetBundles";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle($"{abName}.{abExtension}");

            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = $"{abName}.{abExtension}";
            build.assetNames = assetPaths;

            Debug.Log("assetBundle to build: " + build.assetBundleName);
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>() { build };

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

            File.WriteAllText($"Assets/AssetBundles/{abName}_metadata.json", JsonUtility.ToJson(bundle.bundleMetadata, true));
        }
    }

    private bool MeetsNameRestrictions(string name)
    {
        return regex.IsMatch(name);
    }

    private static Regex regex = new Regex("^[a-z0-9_]+$");
    private static string namingHint = "Please only use lowercase letters, numbers or underscores!";
}
