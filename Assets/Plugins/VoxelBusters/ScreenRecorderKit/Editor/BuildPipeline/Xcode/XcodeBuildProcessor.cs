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
    public class XcodeBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        #region IPreprocessBuildWithReport implementation

        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report)
        {
            var     settings        = ScreenRecorderKitSettingsEditorUtility.DefaultSettings;
            if ((report.summary.platform != BuildTarget.iOS) || (settings == null)) return;

            // update exporter settings status
            var     exporters       = NativePluginsExporterSettings.FindAllExporters(includeInactive: true);
            var     target          = System.Array.Find(exporters, (item) => string.Equals(item.name, AssetConstants.NativePluginsExporterName));
            if (target != null)
            {
                target.IsEnabled    = settings.VideoRecorderSettings.IsEnabled;
            }
        }

        #endregion

        #region IPostprocessBuildWithReport implementation

        public void OnPostprocessBuild(BuildReport report)
        {
            var     settings        = ScreenRecorderKitSettingsEditorUtility.DefaultSettings;
            if ((report.summary.platform != BuildTarget.iOS) || (settings == null) || !settings.VideoRecorderSettings.IsEnabled) return;

            // update xcode configuration
            UpdateInfoPlist(report.summary.outputPath);
        }

        private void UpdateInfoPlist(string outputPath)
        {
            DebugLogger.Log("[ScreenRecorderKit] Updating plist configuration.");

            // open plist
            string  plistPath   = outputPath + "/Info.plist";
            var     plist       = new PlistDocument();
            plist.ReadFromString(IOServices.ReadFile(plistPath));

            var     rootDict    = plist.root;

            // add usage permissions
            var     permissions = GetUsagePermissions();
            foreach (string key in permissions.Keys)
            {
                rootDict.SetString(key, permissions[key]);
            }

            // add LSApplicationQueriesSchemes
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

            // save changes to file
            IOServices.CreateFile(plistPath, plist.WriteToString());
        }

        private static Dictionary<string, string> GetUsagePermissions()
        {
            var requiredPermissionsDict = new Dictionary<string, string>(4)
            {
                { InfoPlistKey.kNSPhotoLibraryUsage,    "This app saves videos to your Photo Library." },
                { InfoPlistKey.kNSPhotoLibraryAdd,      "This app saves videos to your Photo Library." },
                { InfoPlistKey.kNSMicrophoneUsage,      "This app uses microphone while recording videos." }
            };
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