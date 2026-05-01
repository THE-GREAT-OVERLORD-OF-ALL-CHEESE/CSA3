using CheeseMods.CSA3.Components;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine;

namespace CheeseMods.CSA3
{
    [ItemId("cheese.csa3")]
    public class Main : VtolMod
    {
        public static Main instance;
        public static Coroutine coroutine;

        private void Awake()
        {
            instance = this;

            Debug.Log("CSA3: Ready!");

            BaseAssetInfo.GetBaseAssetLists();

            AssetLoader.ScanAssets();
            if (coroutine != null)
                return;

            //AssetLoader.LoadAssets();
            coroutine = StartCoroutine(AssetLoader.LoadAssets());

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