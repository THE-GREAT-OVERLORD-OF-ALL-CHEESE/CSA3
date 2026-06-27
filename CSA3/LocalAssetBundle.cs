using CheeseMods.CSA3Components;
using CheeseMods.VTOLTaskProgressUI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Valve.Newtonsoft.Json;
using VTNetworking;

namespace CheeseMods.CSA3
{
    public class LocalAssetBundle
    {
        [Flags]
        public enum LocalAssetBundleErrors
        {
            NoErrors = 0,
            NoAssetBundle = 1 << 0,
            NoMetaDataFile = 1 << 1,
            MetaDataReadFail = 1 << 2,
            MetaDataFileMismatch = 1 << 3,
            MissingDLL = 1 << 4,
            NoBundle = 1 << 5,
            MoreThanOneBundle = 1 << 6,
            AssetBundleLoadFail = 1 << 7,
            MissingDependancies = 1 << 8,
            MissingComponent = 1 << 9,
            UnsupportedCustomAssetType = 1 << 10,
            InvalidDependancy = 1 << 11, 
            MissingEquipment = 1 << 12,
            OldDLLVersion =  1 << 13,
        }

        public enum Fault
        {
            NoFault,
            BundleDev,
            Cheese,
            UserError,
        }

        public FileInfo bundleFile;
        public FileInfo metaDataFile;

        public string Name => metaData?.bundleName ?? $"Unknown ({bundleFile.Name})";

        public CSA3_BundleMetadata metaData;

        public AssetBundle assetBundle;
        public CSA3_ObjectBundle bundle;

        public bool HasErrors => errors != LocalAssetBundleErrors.NoErrors;
        public LocalAssetBundleErrors errors = LocalAssetBundleErrors.NoErrors;

        public LocalAssetBundle(FileInfo bundleFile)
        {
            this.bundleFile = bundleFile;
            if (!bundleFile.Exists)
            {
                ReportErrors(LocalAssetBundleErrors.NoAssetBundle,
                    $"No asset bundle file found???",
                    Fault.Cheese);
                return;
            }

            metaDataFile = new FileInfo(bundleFile.FullName.Replace(".csa", "_metadata.json"));
            if (!metaDataFile.Exists)
            {
                ReportErrors(LocalAssetBundleErrors.NoMetaDataFile,
                    $"No metadata file found? It should be at {metaDataFile.FullName}. Make sure to export it and copy it into your mod.",
                Fault.BundleDev);
                return;
            }

            metaData = JsonConvert.DeserializeObject<CSA3_BundleMetadata>(File.ReadAllText(metaDataFile.FullName));
            if (metaData == null)
            {
                ReportErrors(LocalAssetBundleErrors.MetaDataReadFail,
                    $"Unable to read metadata file, please re-export it and try again...",
                Fault.BundleDev);
                return;
            }

            Debug.Log($"Found asset bundle for {metaData.bundleName} by {metaData.author}");
        }

        public void ReportErrors(LocalAssetBundleErrors errorType, string errorMessage, Fault fault)
        {
            errors = errors | errorType;
            Debug.LogError($"CSA3: ISSUE WITH ASSET BUNDLE DETECTED");
            Debug.LogError($"CSA3: Error Type: {errorType}");
            Debug.LogError($"CSA3: {errorMessage}");

            switch (fault)
            {
                case Fault.NoFault:
                    // Hmm, not sure
                    break;
                case Fault.BundleDev:
                    Debug.LogError($"CSA3: Please report this issue to the developer of the asset bundle.");
                    Debug.LogError($"CSA3: If you are the developer of this asset bundle, do not release this without fixing the bug first.");
                    break;
                case Fault.Cheese:
                    Debug.LogError($"CSA3: Please report this issue to the THE GREAT OVERLORD OF ALL CHEESE");
                    Debug.LogError($"CSA3: I can be found in the VTOL VR Modding Discord...");
                    break;
                case Fault.UserError:
                    Debug.LogError($"CSA3: Hello User! Unfortunately this apears to be your fault, and you must fix it.");
                    Debug.LogError($"CSA3: Please try checking you dependancies, and failing that, ask for help on the modding Discord.");
                    break;
            }
        }

        private IEnumerator LoadAssetBundle(FileInfo file)
        {
            TaskInfo task = VTOLTaskProgressManager.RegisterTask(Main.instance, $"CSA3: Loading {Name}");

            UnityEngine.Debug.Log($"CSA3: Trying to load {Name}");

            if (errors != LocalAssetBundleErrors.NoErrors)
            {
                UnityEngine.Debug.Log($"CSA3: Refusing to load {Name} as it has errors...");
                task.FinishTask("Failed");
                yield break;
            }

            task.SetStatus("Loading Dependancies");
            if (metaData.dependencies != null) {
                foreach (CSA3_Dependency dependency in metaData.dependencies)
                {
                    TaskInfo dependancyTask = VTOLTaskProgressManager.RegisterTask(Main.instance, $"CSA3: Loading Dependancy of {Name} ({dependency.workshopId})");
                    if (ulong.TryParse(dependency.workshopId, out ulong result))
                    {
                        if (result == Consts.steamworkshopId)
                        {
                            ReportErrors(LocalAssetBundleErrors.InvalidDependancy,
                                $"Illegal dependancy {result}, please tell the dev to edit the mod metadata to not reference CSA3",
                                Fault.BundleDev);
                            dependancyTask.FinishTask();
                            task.FinishTask("Failed");
                            yield break;
                        }

                        UniTask<bool> loadResult = DependancyLoader.Load(result, dependancyTask);
                        while (loadResult.Status != UniTaskStatus.Succeeded)
                        {
                            yield return null;
                        }

                        if (loadResult.result)
                        {
                            UnityEngine.Debug.Log($"CSA3: Loaded dependancy {result}");
                            dependancyTask.FinishTask();
                        }
                        else
                        {
                            ReportErrors(LocalAssetBundleErrors.MissingDependancies,
                                $"Couldn't load mod depedancy {result}, please check your dependancies!",
                            Fault.UserError);
                            dependancyTask.FinishTask("Failed");
                        }
                    }
                    else
                    {
                        ReportErrors(LocalAssetBundleErrors.MetaDataReadFail,
                            $"Couldn't parse dependancy ID, failed to load dependancies!",
                        Fault.BundleDev);
                        dependancyTask.FinishTask("Failed");
                    }
                }
            }
            else
            {
                Debug.Log("CSA3: No dependanices? Suspicious...");
            }

            task.SetStatus("Loading AssetBundle");
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(file.FullName);
            while (!request.isDone)
            {
                task.SetProgress(Mathf.Lerp(0f, 0.5f, request.progress));
                yield return null;
            }
            assetBundle = request.assetBundle;

            if (assetBundle == null)
            {
                ReportErrors(LocalAssetBundleErrors.AssetBundleLoadFail,
                    $"Unable to load asset bundle.",
                Fault.BundleDev);
                task.FinishTask("Failed");
                yield break;
            }

            task.SetStatus("Loading Assets");
            AssetBundleRequest request2 = assetBundle.LoadAllAssetsAsync(typeof(CSA3_ObjectBundle));
            while (!request2.isDone)
            {
                task.SetProgress(Mathf.Lerp(0.5f, 0.9f, request2.progress));
                yield return null;
            }
            UnityEngine.Object[] bundles = request2.allAssets;

            if (bundles.Length == 0)
            {
                ReportErrors(LocalAssetBundleErrors.NoBundle,
                    $"No CSA3_ObjectBundle in Asset Bundle",
                Fault.BundleDev);
                task.FinishTask("Failed");
                yield break;
            }
            if (bundles.Length > 1)
            {
                ReportErrors(LocalAssetBundleErrors.MoreThanOneBundle,
                    $"More than one CSA3_ObjectBundle in Asset Bundle",
                Fault.BundleDev);
                task.FinishTask("Failed");
                yield break;
            }

            bundle = bundles.First() as CSA3_ObjectBundle;

            task.SetStatus("Processing Assets");
            foreach (CSA3_CustomObject customObject in bundle.customObjects)
            {
                ValidateAssets(customObject);

                BuiltMixerFixer.FixAudioSourcesInChildren(customObject.gameObject);

                CSA3_Replacement replacement = customObject.gameObject.GetComponent<CSA3_Replacement>();
                if (replacement != null)
                {
                    foreach (string target in replacement.targets)
                    {
                        ReplacementManager.TryAddReplacment(target, customObject);
                    }
                }

                CustomObjectType customObjectType = GetCustomObjectType(customObject);
                if (customObjectType == CustomObjectType.CustomUnit)
                {
                    UnitSpawn unit = customObject.GetComponent<UnitSpawn>();
                    
                    string resourcePath = $"csa/units/{customObject.name}";
                    
                    VTResources.RegisterOverriddenResource(resourcePath, customObject.gameObject);
                    VTNetworkManager.RegisterOverrideResource(resourcePath, customObject.gameObject);
                    
                    SetupTargetIdentity(customObject, unit);
                    
                    if (unit is AIUnitSpawnEquippable aiUnitSpawnEquippable)
                    {
                        WeaponManager wm = aiUnitSpawnEquippable.GetComponent<WeaponManager>();
                        if (wm)
                            wm.resourcePath = "csa/equips";
                        
                        foreach (GameObject equipPrefab in aiUnitSpawnEquippable.equipPrefabs)
                        {
                            if (equipPrefab == null)
                            {
                                ReportErrors(LocalAssetBundleErrors.MissingEquipment,
                                    $"AIUnitSpawn on {unit.name} has missing equipment prefabs",
                                Fault.BundleDev);
                                task.FinishTask("Failed");
                                yield break;
                            }

                            string eqResourcePath = $"csa/equips/{equipPrefab.name}";
                            
                            VTResources.RegisterOverriddenResource(eqResourcePath, equipPrefab.gameObject);
                            VTNetworkManager.RegisterOverrideResource(eqResourcePath, equipPrefab.gameObject);

                            HPEquipMissileLauncher hpEquipMl = equipPrefab.GetComponent<HPEquipMissileLauncher>();
                            if (hpEquipMl)
                            {
                                GameObject missilePrefab = hpEquipMl.ml.missilePrefab;
                                string missileResourcePath = $"csa/missiles/{missilePrefab.name}";
                                VTResources.RegisterOverriddenResource(missileResourcePath, missilePrefab);
                                VTNetworkManager.RegisterOverrideResource(missileResourcePath, missilePrefab);
                                hpEquipMl.missileResourcePath = missileResourcePath;
                                
                                SetupTargetIdentity(missilePrefab);
                            }
                        }
                    }
                }
            }

            Debug.Log($"CSA3: {Name} loaded!");
            task.FinishTask();
        }

        public IEnumerator Load()
        {
            Unload();
            yield return LoadAssetBundle(bundleFile);
        }

        public void Unload()
        {
            if (assetBundle == null)
                return;

            assetBundle.Unload(false);
            Debug.Log($"CSA3: {Name} unloaded!");

            bundle = null;
            assetBundle = null;
        }

        public List<CSA3_CustomObject> GetAllCustomObjects(CustomObjectType customObjectType)
        {
            if (bundle == null)
                return null;

            switch (customObjectType)
            {
                case CustomObjectType.StaticObject:
                    return bundle.customObjects.Where(o => o is CSA3_StaticObject staticObject).ToList();
                case CustomObjectType.MapObject:
                    return bundle.customObjects.Where(o => o is CSA3_MapObject mapObject).ToList();
                case CustomObjectType.CustomUnit:
                    return bundle.customObjects.Where(o => o is CSA3_CustomUnit customUnit).ToList();
            }

            return null;
        }

        public CSA3_CustomObject GetCustomObject(CustomObjectType customObjectType, string id)
        {
            if (bundle == null)
                return null;


            switch (customObjectType)
            {
                case CustomObjectType.StaticObject:
                    return bundle.customObjects.FirstOrDefault(o => o is CSA3_StaticObject staticObject && staticObject.gameObject.name == id);
                case CustomObjectType.MapObject:
                    return bundle.customObjects.FirstOrDefault(o => o is CSA3_MapObject mapObject && mapObject.gameObject.name == id);
                case CustomObjectType.CustomUnit:
                    return bundle.customObjects.FirstOrDefault(o => o is CSA3_CustomUnit customUnit && customUnit.gameObject.name == id);
            }

            return null;
        }

        public static CustomObjectType GetCustomObjectType(CSA3_CustomObject customObject)
        {
            if (customObject == null)
                return CustomObjectType.InvalidType;
            
            if (customObject is CSA3_MapObject)
                return CustomObjectType.MapObject;
            if (customObject is CSA3_StaticObject)
                return CustomObjectType.StaticObject;
            if (customObject is CSA3_CustomUnit)
                return CustomObjectType.CustomUnit;
            
            return CustomObjectType.InvalidType;
        }

        public void ValidateAssets(CSA3_CustomObject customObject)
        {
            if (customObject is CSA3_MapObject mapObjects)
            {
                if (!customObject.gameObject.GetComponent<VTMapEdPrefab>())
                {
                    ReportErrors(LocalAssetBundleErrors.MissingComponent,
                        $"A map object ({customObject.gameObject.name}) needs a {nameof(VTMapEdPrefab)} component attached, please add one in unity.",
                        Fault.BundleDev);
                }
                return;
            }
            if (customObject is CSA3_StaticObject staticObject)
            {
                if (!customObject.gameObject.GetComponent<VTStaticObject>())
                {
                    ReportErrors(LocalAssetBundleErrors.MissingComponent,
                        $"A static object ({customObject.gameObject.name}) needs a {nameof(VTStaticObject)} component attached, please add one in unity.",
                        Fault.BundleDev);
                }
                return;
            }
            if (customObject is CSA3_CustomUnit customUnit)
            {
                if (!customObject.gameObject.GetComponent<UnitSpawn>())
                {
                    ReportErrors(LocalAssetBundleErrors.MissingComponent,
                        $"A custom ({customObject.gameObject.name}) needs a {nameof(UnitSpawn)} component attached, please add one in unity.",
                        Fault.BundleDev);
                }
                if (!customObject.gameObject.GetComponent<Actor>())
                {
                    ReportErrors(LocalAssetBundleErrors.MissingComponent,
                        $"A custom ({customObject.gameObject.name}) needs an {nameof(Actor)} component attached, please add one in unity.",
                        Fault.BundleDev);
                }
                return;
            }

            ReportErrors(LocalAssetBundleErrors.UnsupportedCustomAssetType,
                $"Support for {customObject.GetType()} assets has not been added, beg me to add it on Discord.",
                Fault.Cheese);
        }
        
        private void SetupTargetIdentity(CSA3_CustomObject customObject, UnitSpawn unit)
        {
            UnitIDIdentifier unitID = customObject.gameObject.GetComponent<UnitIDIdentifier>();
            if (!unitID)
            {
                unitID = customObject.gameObject.AddComponent<UnitIDIdentifier>();

                unitID.targetName = unit.unitName;
                unitID.unitID = $"csa.{unit.name}";
                Actor.Roles role = Actor.Roles.Ground;

                switch (unit.groupType)
                {
                    case VTUnitGroup.GroupTypes.Ground:
                        role = Actor.Roles.Ground;
                        break;
                    case VTUnitGroup.GroupTypes.Air:
                        role = Actor.Roles.Air;
                        break;
                    case VTUnitGroup.GroupTypes.Sea:
                        role = Actor.Roles.Ship;
                        break;
                }

                unitID.role = role;
            }

            TargetIdentity targetIdentity = TargetIdentityManager.RegisterNonSpawnIdentity(unitID.unitID, unitID.targetName, unitID.role);
            if (customObject is CSA3_CustomUnit customUnit)
            {
                try
                {
                    targetIdentity.index = customUnit.customUnitTargetIndex;
                }
                catch (Exception)
                {
                    ReportErrors(LocalAssetBundleErrors.OldDLLVersion, "Ask the Mod author to update their CSA3Components DLL in their Unity project", Fault.BundleDev);
                    targetIdentity.index = 0;
                }
            }
            if (!TargetIdentityManager.indexedIdentities.Contains(targetIdentity))
                TargetIdentityManager.indexedIdentities.Add(targetIdentity);
        }

        private void SetupTargetIdentity(GameObject missile)
        {
            UnitIDIdentifier unitID = missile.GetComponent<UnitIDIdentifier>();
            if (!unitID)
            {
                unitID = missile.gameObject.AddComponent<UnitIDIdentifier>();

                unitID.targetName = missile.name;
                unitID.unitID = $"csa.missile.{missile.name}";

                unitID.role = Actor.Roles.Missile;
            }

            TargetIdentity targetIdentity = TargetIdentityManager.RegisterNonSpawnIdentity(unitID.unitID, unitID.targetName, unitID.role);
            if (!TargetIdentityManager.indexedIdentities.Contains(targetIdentity))
                TargetIdentityManager.indexedIdentities.Add(targetIdentity);
        }
    }
}
