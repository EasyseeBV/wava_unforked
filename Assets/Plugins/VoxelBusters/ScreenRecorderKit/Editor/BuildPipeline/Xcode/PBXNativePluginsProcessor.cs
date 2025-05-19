#if UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.iOS.Xcode;
using UnityEditor.Build.Reporting;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.Editor;
using VoxelBusters.CoreLibrary.Editor.NativePlugins.Build;
using VoxelBusters.CoreLibrary.Editor.NativePlugins.Build.Xcode;

namespace VoxelBusters.ScreenRecorderKit.Editor.Build.Xcode
{
    public class PBXNativePluginsProcessor : CoreLibrary.Editor.NativePlugins.Build.Xcode.PBXNativePluginsProcessor
    {
        #region Properties

        private ScreenRecorderKitSettings Settings { get; set; }

        #endregion

        #region Base class methods

        public override void OnUpdateExporterObjects()
        {
            // Check whether plugin is configured
            if (!EnsureInitialised()) return;

            var     exporters       = NativePluginsExporterObject.FindObjects<PBXNativePluginsExporterObject>(includeInactive: true);
            var     target          = System.Array.Find(exporters, (item) => string.Equals(item.name, AssetConstants.NativePluginsVideoRecorderFeatureExporterName));
            if (target != null)
            {
                target.IsEnabled    = Settings.VideoRecorderSettings.IsEnabled;
            }
        }

        public override void OnUpdateInfoPlist(PlistDocument doc)
        {
            var     rootDict    = doc.root;

            // Add usage permissions
            var     permissions = GetUsagePermissions();
            foreach (string key in permissions.Keys)
            {
                rootDict.SetString(key, permissions[key]);
            }

            // Add LSApplicationQueriesSchemes
            string[]    appQuerySchemes = GetApplicationQueriesSchemes();
            if (appQuerySchemes.Length > 0)
            {
                PlistElementArray   array;
                if (false == rootDict.TryGetElement(InfoPlistKey.kNSQuerySchemes, out array))
                {
                    array = rootDict.CreateArray(InfoPlistKey.kNSQuerySchemes);
                }

                // add required schemes
                for (int iter = 0; iter < appQuerySchemes.Length; iter++)
                {
                    if (false == array.Contains(appQuerySchemes[iter]))
                    {
                        array.AddString(appQuerySchemes[iter]);
                    }
                }
            }
        }

        private bool EnsureInitialised()
        {
            if (Settings != null) return true;

            if (ScreenRecorderKitSettingsEditorUtility.TryGetDefaultSettings(out ScreenRecorderKitSettings settings))
            {
                Settings    = settings;
                return true;
            }

            return false;
        }

        private Dictionary<string, string> GetUsagePermissions()
        {
            var requiredPermissionsDict = new Dictionary<string, string>(4)
            {
                { InfoPlistKey.kNSPhotoLibraryUsage,    "This app saves videos to your Photo Library." },
                { InfoPlistKey.kNSPhotoLibraryAdd,      "This app saves videos to your Photo Library." },
                { InfoPlistKey.kNSMicrophoneUsage,      "This app uses microphone while recording videos." }
            };

            if(!ScreenRecorderKitSettingsEditorUtility.DefaultSettings.VideoRecorderSettings.UsesMicrophone)
            {
               requiredPermissionsDict.Remove(InfoPlistKey.kNSMicrophoneUsage); 
            }

            return requiredPermissionsDict;
        }

        private string[] GetApplicationQueriesSchemes()
        {
            var     schemeList  = new string[]
            {
                "fb",
                "twitter",
                "whatsapp"
            };
            return schemeList;
        }

        #endregion
    }
}
#endif