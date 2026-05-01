using CheeseMods.CSA3Components;
using CheeseMods.VTOLTaskProgressUI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace CheeseMods.CSA3
{
    public static class AssetLoader
    {
        public static List<LocalAssetBundle> localAssetBundles = new List<LocalAssetBundle>();

        public static void ScanAssets()
        {
            string localModsDir = Path.Combine(Directory.GetCurrentDirectory(), "@Mod Loader", @"Mods");
            string workshopModsDir = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\workshop\content\3018410");

            ScanAssets(localModsDir);
            ScanAssets(workshopModsDir);
        }

        public static void ScanAssets(string dir)
        {
            Debug.Log($"Searching for assets in {dir}");

            if (Directory.Exists(dir))
            {
                DirectoryInfo info = new DirectoryInfo(dir);
                foreach (DirectoryInfo item in info.GetDirectories())
                {
                    foreach (FileInfo files in item.GetFiles())
                    {
                        if (files.Name.EndsWith(".csa") && !localAssetBundles.Any(b => b.bundleFile.Name == files.Name))
                        {
                            Debug.Log($"CSA bundle found: {files.Name}");
                            localAssetBundles.Add(new LocalAssetBundle(files));
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"{dir} doesn't exist, thats not right...");
            }
        }

        public static IEnumerator LoadAssets()
        {
            TaskInfo task = VTOLTaskProgressManager.RegisterTask(Main.instance, $"CSA3: Loading assets");
            int assetBundleNumber = 0;
            foreach (LocalAssetBundle localAssetBundle in localAssetBundles)
            {
                task.SetProgress((float)assetBundleNumber / (float)localAssetBundles.Count);
                yield return localAssetBundle.Load();
                assetBundleNumber++;
            }
            task.FinishTask();
        }

        public static void UnloadAssets()
        {
            foreach (LocalAssetBundle localAssetBundle in localAssetBundles)
            {
                localAssetBundle.Unload();
            }
        }

        public static List<CSA3_CustomObject> GetAllCustomObjects(CustomObjectType customObjectType)
        {
            List<CSA3_CustomObject> allStaticObjects = new List<CSA3_CustomObject>();

            foreach (LocalAssetBundle localAssetBundle in localAssetBundles)
            {
                if (localAssetBundle.HasErrors)
                    continue;

                List<CSA3_CustomObject> customObjects = localAssetBundle.GetAllCustomObjects(customObjectType);
                if (customObjects != null)
                {
                    allStaticObjects.AddRange(customObjects);
                }
            }

            return allStaticObjects;
        }

        public static CSA3_CustomObject GetCustomObject(CustomObjectType customObjectType, string id)
        {
            foreach (LocalAssetBundle localAssetBundle in localAssetBundles)
            {
                if (localAssetBundle.HasErrors)
                    continue;

                CSA3_CustomObject staticObject = localAssetBundle.GetCustomObject(customObjectType, id);
                if (staticObject != null)
                {
                    return staticObject;
                }
            }

            return null;
        }
    }
}
