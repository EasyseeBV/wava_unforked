using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    /// <summary>
    /// VideoRecorderSettings used for configuring a video recorder.
    /// </summary>
    [System.Serializable]
    public partial class VideoRecorderSettings : SettingsPropertyGroup
    {
        #region Fields

        [SerializeField]
        private     bool                        m_usesMicrophone;

        [SerializeField]
        private     IosPlatformProperties       m_iosProperties;

        [SerializeField]
        private     AndroidPlatformProperties   m_androidProperties;

        #endregion

        #region Properties

        /// <summary>
        /// Enable if you use microphone while recording a video.
        /// </summary>
        public bool UsesMicrophone
        {
            get => m_usesMicrophone;
            private set => m_usesMicrophone = value;
        }

        /// <summary>
        /// Properties specific to iOS platform.
        /// </summary>
        public IosPlatformProperties IosProperties
        {
            get => m_iosProperties;
            private set => m_iosProperties  = value;
        }

        /// <summary>
        /// Properties specific to Android platform.
        /// </summary>
        public AndroidPlatformProperties AndroidProperties
        {
            get => m_androidProperties;
            private set => m_androidProperties  = value;
        }

        #endregion

        #region Constructors

        public VideoRecorderSettings(bool isEnabled = true, bool usesMicrophone = true,
            IosPlatformProperties iosProperties = null, AndroidPlatformProperties androidProperties = null)
            : base(name: "Replay Kit Settings", isEnabled: isEnabled)
        {
            // set properties
            UsesMicrophone      = usesMicrophone;
            IosProperties       = iosProperties ?? new IosPlatformProperties();
            AndroidProperties   = androidProperties ?? new AndroidPlatformProperties();
        }

        #endregion
    }
}