using CheeseMods.CSA3.Components;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine;

namespace CheeseMods.CSA3
{
    [ItemId("cheese.csa3")]
    public class Main : VtolMod
    {
        private void Awake()
        {
            Debug.Log("CSA3: Ready!");

            BaseAssetInfo.GetBaseAssetLists();

            AssetLoader.ScanAssets();
            AssetLoader.LoadAssets();

            gameObject.AddComponent<AssetBundleErrorWindow>();
        }

        public override void UnLoad()
        {
            Debug.Log("CSA3: Unloading Assets!");
            AssetLoader.UnloadAssets();
            ReplacementManager.ClearReplacements();
        }
    }
}