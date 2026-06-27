using UnityEngine;

namespace CheeseMods.CSA3Components
{
    public class CSA3_CustomUnit : CSA3_CustomObject
    {
        [Tooltip("Index value to use for a custom model in mission replays")]
        public int customUnitTargetIndex;
        [Tooltip("Custom replay Model Prefab (Optional)")]
        public GameObject replayModelPrefab;
    }
}
