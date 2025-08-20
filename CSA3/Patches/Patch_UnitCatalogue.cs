using CheeeseMods.CSA3;
using CheeseMods.CSA3Components;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CheeseMods.CSA3.Patches
{
    // I hate the unit catalog with a passion, how dare you make me do this danku :(
    [HarmonyPatch(typeof(UnitCatalogue), "UpdateCatalogue")]
    class Patch_UnitCatalogue_UpdateCatalogue
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            if (BaseAssetInfo.disableModdedObjects)
            {
                return;
            }

            Debug.Log("UnitCatalogue.UpdateCatalogue() was called, checking if we need to add custom units.");
            if (VTScenario.current != null && VTScenario.current.multiplayer != UnitCatalogHelper.isCatalogMp)
            {
                UnitCatalogHelper.isCatalogMp = VTScenario.current.multiplayer;
                UnitCatalogHelper.upToDate = false;
                Debug.Log("The MP state has changed, we will need to generate new custom units for the unit catalog.");
            }
            if (UnitCatalogHelper.upToDate)
            {
                Debug.Log("Custom AI aircraft are up to date, no need to do anything.");
                return;
            }
            UnitCatalogHelper.upToDate = true;

            Debug.Log("Adding custom units to unit catalog.");

            foreach (CSA3_CustomObject customObject in AssetLoader.GetAllCustomObjects(CustomObjectType.CustomUnit))
            {
                UnitSpawn unit = customObject.GetComponent<UnitSpawn>();

                Teams team = unit.gameObject.GetComponentInChildren<Actor>().team;

                UnitCatalogue.Unit catalogueUnit = new UnitCatalogue.Unit();
                catalogueUnit.prefabName = unit.name;
                catalogueUnit.name = unit.unitName;
                catalogueUnit.description = unit.unitDescription;
                catalogueUnit.teamIdx = (int)team;
                catalogueUnit.isPlayerSpawn = false;
                catalogueUnit.hideFromEditor = false;
                catalogueUnit.resourcePath = $"csa/units/{unit.name}"; // im not implementing equipable weapons, i can't deal with this, someone else on github will have to make an implmentation if they want it

                UnitCatalogue.UnitTeam unitTeam = UnitCatalogue.catalogue[team];

                UnitCatalogue.UnitCategory category;
                if (unitTeam.categories.TryGetValue(unit.category, out category) == false)
                {
                    category = new UnitCatalogue.UnitCategory();
                    category.name = unit.category;
                    unitTeam.categories.Add(unit.category, category);
                    unitTeam.keys.Add(unit.category);
                }

                catalogueUnit.categoryIdx = unitTeam.keys.IndexOf(unit.category);
                catalogueUnit.unitIdx = category.keys.Count - 1;

                UnitCatalogue.unitPrefabs.Add(unit.name, customObject.gameObject);
                UnitCatalogue.units.Add(unit.name, catalogueUnit);
                UnitCatalogue.catalogue[team].allUnits.Add(catalogueUnit);

                Debug.Log("updating lists and dictionaries");
                if (category.units.ContainsKey(unit.name) == false)
                {
                    category.units.Add(unit.name, catalogueUnit);
                }
            }
            Debug.Log("setting up allied and enemy catagory options");

            UnitCatalogue.categoryOptions = new Dictionary<Teams, string[]>();
            string[] alliedUnits = new string[UnitCatalogue.catalogue[Teams.Allied].categories.Count];
            int aCount = 0;
            foreach (string alliedUnit in UnitCatalogue.catalogue[Teams.Allied].categories.Keys)
            {
                alliedUnits[aCount] = alliedUnit;
                aCount++;
            }
            UnitCatalogue.categoryOptions.Add(Teams.Allied, alliedUnits);
            string[] enemyUnits = new string[UnitCatalogue.catalogue[Teams.Enemy].categories.Count];
            int eCount = 0;
            foreach (string enemyUnit in UnitCatalogue.catalogue[Teams.Enemy].categories.Keys)
            {
                enemyUnits[eCount] = enemyUnit;
                eCount++;
            }
            UnitCatalogue.categoryOptions.Add(Teams.Enemy, enemyUnits);
        }
    }

    [HarmonyPatch(typeof(UnitCatalogue), "GetUnitPrefab")]
    class Patch_UnitCatalogue_GetUnitPrefab
    {
        [HarmonyPrefix]
        static bool Prefix(out GameObject __result, string unitID)
        {
            if (BaseAssetInfo.disableModdedObjects)
            {
                __result = null;
                return true;
            }

            ReplacementManager.GetReplacment(ref unitID);

            if (BaseAssetInfo.baseMapObjects.Contains(unitID))
            {
                __result = null;
                return true;
            }

            CSA3_CustomObject customObject = AssetLoader.GetCustomObject(CustomObjectType.CustomUnit, unitID);
            if (customObject is CSA3_CustomUnit mapObject)
            {
                __result = customObject.gameObject;
                return false;
            }

            __result = null;
            return true;
        }
    }

    [HarmonyPatch(typeof(UnitCatalogue), "GetUnit", new Type[] { typeof(string) })]
    class Patch_UnitCatalogue_GetUnit
    {
        [HarmonyPrefix]
        static bool Prefix(ref UnitCatalogue.Unit __result, ref string unitID)
        {
            if (BaseAssetInfo.disableModdedObjects)
            {
                return true;
            }

            ReplacementManager.GetReplacment(ref unitID);
            return true;
        }
    }
}