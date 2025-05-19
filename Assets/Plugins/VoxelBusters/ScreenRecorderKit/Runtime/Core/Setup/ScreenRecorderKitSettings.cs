using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.ScreenRecorderKit
{
    public class ScreenRecorderKitSettings : SettingsObject
    {
        #region Static fields

        private     static      ScreenRecorderKitSettings   s_sharedInstance;
        private     static      UnityPackageDefinition      s_package;

        #endregion

        #region Fields

        [SerializeField]
        private     VideoRecorderSettings       m_videoRecorderSettings         = new VideoRecorderSettings();

        [SerializeField]
        private     GifRecorderSettings         m_gifRecorderSettings           = new GifRecorderSettings();

        #endregion

        #region Static properties

        internal static UnityPackageDefinition Package
        {
            get
            {
                if (s_package == null)
                {
                    s_package   = new UnityPackageDefinition(
                        name: "com.voxelbusters.screenrecorderKit",
                        displayName: "Screen Recorder Kit",
                        version: "2.4.2",
                        defaultInstallPath: $"Assets/Plugins/VoxelBusters/ScreenRecorderKit",
                        dependencies: CoreLibrarySettings.Package);
                }
                return s_package;
            }
        }

        public static string PackageName => Package.Name;

        public static string DisplayName => Package.DisplayName;

        public static string Version => Package.Version;

        public static string DefaultSettingsAssetName => "ScreenRecorderKitSettings";

        public static string DefaultSettingsAssetPath => $"{Package.GetMutableResourcesPath()}/{DefaultSettingsAssetName}.asset";

        public static string PersistentDataPath => Package.PersistentDataPath;

        public static ScreenRecorderKitSettings Instance
        {
            get { return GetSharedInstanceInternal(); }
        }

        #endregion

        #region Properties

        public VideoRecorderSettings VideoRecorderSettings
        {
            get => m_videoRecorderSettings;
            internal set => m_videoRecorderSettings = value;
        }

        public GifRecorderSettings GifRecorderSettings
        {
            get => m_gifRecorderSettings;
            internal set => m_gifRecorderSettings   = value;
        }

        #endregion

        #region Static methods

        public static ScreenRecorderKitSettings Create(VideoRecorderSettings videoRecorderSettings = null, GifRecorderSettings gifRecorderSettings = null)
        {
            var     newInstance                 = CreateInstance<ScreenRecorderKitSettings>();
            newInstance.VideoRecorderSettings   = videoRecorderSettings ?? new VideoRecorderSettings();
            newInstance.GifRecorderSettings     = gifRecorderSettings ?? new GifRecorderSettings();

            return newInstance;
        }

        private static ScreenRecorderKitSettings GetSharedInstanceInternal(bool throwError = true)
        {
            if (null == s_sharedInstance)
            {
                // check whether we are accessing in edit or play mode
                var     assetPath   = DefaultSettingsAssetName;
                var     settings    = Resources.Load<ScreenRecorderKitSettings>(assetPath);
                if (throwError && (null == settings))
                {
                    throw Diagnostics.PluginNotConfiguredException();
                }

                // store reference
                s_sharedInstance = settings;
            }

            return s_sharedInstance;
        }

        #endregion
    }
}