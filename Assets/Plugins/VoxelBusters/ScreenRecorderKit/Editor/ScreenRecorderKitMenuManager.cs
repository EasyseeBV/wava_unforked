using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.Editor;

namespace VoxelBusters.ScreenRecorderKit.Editor
{
    internal static class ScreenRecorderKitMenuManager
    {
        #region Constants

        private const string kMenuItemPath = "Window/Voxel Busters/Screen Recorder Kit";

        #endregion

        #region Menu items

        [MenuItem(kMenuItemPath + "/Open Settings", priority = 0)]
        public static void OpenSettings()
        {
            ScreenRecorderKitSettingsEditorUtility.OpenInProjectSettings();
        }

#if NATIVE_PLUGINS_SHOW_UPM_MIGRATION
        [MenuItem(kMenuItemPath + "/Migrate To UPM", priority = 2)]
        public static void MigrateToUpm()
        {
            ScreenRecorderKitSettings.Package.MigrateToUpm();
        }

        [MenuItem(kMenuItemPath + "/Migrate To UPM", isValidateFunction: true, priority = 2)]
        private static bool ValidateMigrateToUpm()
        {
            return ScreenRecorderKitSettings.Package.IsInstalledWithinAssets();
        }
#endif

        [MenuItem(kMenuItemPath + "/Uninstall", priority = 3)]
        public static void Uninstall()
        {
            ScreenRecorderKitSettingsEditorUtility.RemoveGlobalDefines();
            UninstallPlugin.Uninstall();
        }

#endregion
    }
}