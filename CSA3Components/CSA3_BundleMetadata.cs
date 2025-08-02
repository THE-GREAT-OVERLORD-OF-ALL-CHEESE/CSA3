using System;
using System.Collections.Generic;
using UnityEngine;

namespace CheeseMods.CSA3Components
{
    [System.Serializable]
    public class CSA3_BundleMetadata
    {
        [Tooltip("The display name for your bundle of assets.")]
        public string bundleName;
        [Tooltip("A string used to uniquely identify you assets. " + namingRules)]
        public string bundleId;

        [Tooltip("The display name for the assets to be credited to, please use you username.")]
        public string author;
        [Tooltip("A string used to uniquely identify your assets. Please use your username. " + namingRules)]
        public string authorId;
        [TextArea]
        [Tooltip("Please write a description of the assets in your mod")]
        public string description;
        [Tooltip("The version of CSA3 used to build the assets. Its auto populated, don't worry about filling it in")]
        public CSA3_VersionInfo.VersionNumber csa3VersionNumber = CSA3_VersionInfo.CurrentVersion;
        [Tooltip("Version number of your assets, please update appropriately, it will be used to validating matching assets in mp")]
        public CSA3_VersionInfo.VersionNumber bundleVersionNumber;

        [Tooltip("Are these assets functional in multiplayer? \"MultiplayerUntested\" is only to be used for testing purposes, please test all your assets for an actual release!")]
        public CSA3_MultiplayerCompatibility multiplayerCompatibility;

        [Tooltip("Other mods that your mod has code dependancies on, add the workshop ids of the dependancies")]
        public List<CSA3_Dependency> dependencies;

        public const string namingRules = "Please only use lower case characters, numbers and underscores.";
    }
}
