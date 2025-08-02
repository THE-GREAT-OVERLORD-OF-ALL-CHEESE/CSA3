using System.Collections.Generic;
using UnityEngine;

namespace CheeseMods.CSA3Components
{
    public class CSA3_Replacement : MonoBehaviour
    {
        [Tooltip("Prefab names/prefab ids of built in game assets you want to replace with this one.")]
        public List<string> targets;
    }
}
