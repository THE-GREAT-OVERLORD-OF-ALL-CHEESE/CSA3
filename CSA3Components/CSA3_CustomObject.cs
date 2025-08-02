using System;
using System.Collections.Generic;
using UnityEngine;

namespace CheeseMods.CSA3Components
{
    public abstract class CSA3_CustomObject : MonoBehaviour
    {
        [Tooltip("Autopopulated, don't worry about it")]
        public CSA3_ObjectBundle parentBundle;

        [Tooltip("Disaply name of your asset")]
        public string displayName;
        [Tooltip("id for uniquely identifying your asset. " + CSA3_BundleMetadata.namingRules)]
        public string objectId;
        [TextArea]
        [Tooltip("Discription of your asset")]
        public string description;
    }
}
