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
    }
}
