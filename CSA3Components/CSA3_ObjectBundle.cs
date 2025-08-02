using System.Collections.Generic;
using UnityEngine;

namespace CheeseMods.CSA3Components
{
    [CreateAssetMenu(fileName = "ObjectBundle", menuName = "CSA3/ObjectBundle")]
    public class CSA3_ObjectBundle : ScriptableObject
    {
        public CSA3_BundleMetadata bundleMetadata;

        [Tooltip("Prefabs to be included in your mod.")]
        public List<CSA3_CustomObject> customObjects;

        /*
        [ContextMenu("Build Bundle")]
        public void BuildBundle()
        {
            string assetBundleDirectory = "Assets/AssetBundles";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }
            BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                            BuildAssetBundleOptions.UncompressedAssetBundle,
                                            BuildTarget.StandaloneWindows64);
        }
        */
    }
}
