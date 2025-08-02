using System;
using System.Linq;
using UnityEngine;

namespace CheeseMods.CSA3.Components
{
    public class AssetBundleErrorWindow : MonoBehaviour
    {
        private static bool hidden;

        private static void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                hidden = !hidden;
            }
        }

        private void OnGUI()
        {
            if (AssetLoader.localAssetBundles.Any(b => b.errors != LocalAssetBundle.LocalAssetBundleErrors.NoErrors))
            {
                windowRect = GUI.Window(402, windowRect, WindowFunction, "CSA3 - Asset Bundle Errors");
            }
        }

        private static void WindowFunction(int windowID)
        {
            float startingPos = 20;

            GUI.Label(new Rect(20, startingPos, 360, 60), $"Something has gone wrong, check logs for details...");
            startingPos += 20;

            foreach (LocalAssetBundle bundle in AssetLoader.localAssetBundles)
            {
                if (bundle.HasErrors)
                {
                    GUI.Label(new Rect(20, startingPos, 360, 60), $"{bundle.Name} Errors:");
                    startingPos += 20;

                    for (int i = 0; i < 32; i++)
                    {
                        if (bundle.errors.HasFlag((LocalAssetBundle.LocalAssetBundleErrors)(1 << i)))
                        {
                            GUI.Label(new Rect(20, startingPos, 360, 60), $"{(LocalAssetBundle.LocalAssetBundleErrors)(1 << i)}");

                            startingPos += 20;
                        }
                    }

                    startingPos += 20;
                }
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        private static Rect windowRect = new Rect(20, 20, 400, 600);
    }
}
