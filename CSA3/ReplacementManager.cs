using CheeseMods.CSA3Components;
using System.Collections.Generic;
using System.Linq;

namespace CheeseMods.CSA3
{
    public static class ReplacementManager
    {
        private static List<ObjectReplacement> replacements = new List<ObjectReplacement>();

        public static bool TryAddReplacment(string oldId, CSA3_CustomObject customObject)
        {
            replacements.Add(new ObjectReplacement(oldId, customObject.gameObject.name));
            return true;
        }

        public static void GetReplacment(ref string id)
        {
            string originalId = id;
            ObjectReplacement replacement = replacements.FirstOrDefault(r => r.active && r.originalId == originalId);

            if (replacement != null)
            {
                id = replacement.newId;
            }
        }

        public static void ClearReplacements()
        {
            replacements.Clear();
        }
    }
}
