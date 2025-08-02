using RewiredConsts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MFDSpecificObjects;

namespace CheeseMods.CSA3
{
    public static class BaseAssetInfo
    {
        public static List<string> baseStaticObjects;
        public static List<string> baseMapObjects;

        public static List<string> baseUnits;

        public static void GetBaseAssetLists()
        {
            Debug.Log("CSA3: Getting base game assets.");

            Debug.Log("CSA3: Getting base game static object names.");
            baseStaticObjects = new List<string>();
            List<VTStaticObject> staticObjects = VTResources.GetAllStaticObjectPrefabs();
            foreach (VTStaticObject staticObject in staticObjects)
            {
                baseStaticObjects.Add(staticObject.name);
            }

            Debug.Log("CSA3: Getting base game map object names.");
            VTMapEdResources.LoadAll();
            baseMapObjects = new List<string>();
            List<string> categories = VTMapEdResources.GetAllCategories();
            foreach (string category in categories)
            {
                foreach (VTMapEdPrefab prefab in VTMapEdResources.GetPrefabs(category))
                {
                    baseMapObjects.Add(prefab.gameObject.name);
                    //Debug.Log($"{category}: {prefab.gameObject.name}");
                }
            }

            Debug.Log("CSA3: Getting base game units.");
            UnitCatalogue.UpdateCatalogue();
            baseUnits = new List<string>();
            foreach (KeyValuePair<Teams, UnitCatalogue.UnitTeam> team in UnitCatalogue.catalogue)
            {
                foreach (UnitCatalogue.Unit unit in team.Value.allUnits)
                {
                    baseUnits.Add(unit.prefabName);
                }
            }
        }
    }
}
