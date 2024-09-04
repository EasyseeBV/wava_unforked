using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.CoreLibrary.Editor;

namespace VoxelBusters.ScreenRecorderKit.Editor
{
    [InitializeOnLoad]
    public static class ScreenRecorderKitSettingsEditorUtility
    {
        #region Constants

        private     const       string                      kLocalPathInProjectSettings     = "Project/Voxel Busters/Screen Recorder Kit";

        #endregion

        #region Static fields

        private     static      ScreenRecorderKitSettings         s_defaultSettings;

        #endregion

        #region Static properties

        public static ScreenRecorderKitSettings DefaultSettings
        {
            get
            {
                if (s_defaultSettings == null)
                {
                    var     instance    = LoadDefaultSettingsObject(throwError: false);
                    if (null == instance)
                    {
                        instance        = CreateDefaultSettingsObject();
                    }
                    s_defaultSettings   = instance;
                }
                return s_defaultSettings;
            }
            set
            {
                Assert.IsPropertyNotNull(value, nameof(value));

                // set new value
                s_defaultSettings       = value;
            }
        }

        public static bool SettingsExists
        {
            get
            {
                if (s_defaultSettings == null)
                {
                    s_defaultSettings   = LoadDefaultSettingsObject(throwError: false);
                }
                return (s_defaultSettings != null);
            }
        }

        #endregion

         #region Constructors

        static ScreenRecorderKitSettingsEditorUtility()
        {
            AddGlobalDefines();
        }

        #endregion

        #region Static methods

        public static void ShowSettingsNotFoundErrorDialog()
        {
            EditorUtility.DisplayDialog(
                title: "Error",
                message: "Recorder Kit plugin is not configured. Please select plugin settings file from menu and configure it according to your preference.",
                ok: "Ok");
        }

        public static void AddGlobalDefines()
        {
            ScriptingDefinesManager.RemoveDefine("ENABLE_VOXELBUSTERS_RECORDER_KIT");
            ScriptingDefinesManager.AddDefine("ENABLE_VOXELBUSTERS_SCREEN_RECORDER_KIT");
        }
        
        public static void RemoveGlobalDefines()
        {
            ScriptingDefinesManager.RemoveDefine("ENABLE_VOXELBUSTERS_SCREEN_RECORDER_KIT");
        }

        public static void OpenInProjectSettings()
        {
            if (!SettingsExists)
            {
                CreateDefaultSettingsObject();
            }
            Selection.activeObject  = null;
            SettingsService.OpenProjectSettings(kLocalPathInProjectSettings);
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return SettingsProviderZ.Create(
                settingsObject: DefaultSettings,
                path: kLocalPathInProjectSettings,
                scopes: SettingsScope.Project);
        }

        #endregion

        #region Private static methods

        private static ScreenRecorderKitSettings CreateDefaultSettingsObject()
        {
            return AssetDatabaseUtility.CreateScriptableObject<ScreenRecorderKitSettings>(
                assetPath: ScreenRecorderKitSettings.DefaultSettingsAssetPath);
        }

        private static ScreenRecorderKitSettings LoadDefaultSettingsObject(bool throwError = true)
        {
            var     throwErrorFunc      = throwError ? () => Diagnostics.PluginNotConfiguredException() : (System.Func<System.Exception>)null;
            return AssetDatabaseUtility.LoadScriptableObject<ScreenRecorderKitSettings>(
                assetPath: ScreenRecorderKitSettings.DefaultSettingsAssetPath,
                throwErrorFunc: throwErrorFunc);
        }

        #endregion

        #region Internal methods

        internal static void Rebuild()
        {
            // Refresh Database
            AssetDatabase.Refresh();
        }

        #endregion
    }
}