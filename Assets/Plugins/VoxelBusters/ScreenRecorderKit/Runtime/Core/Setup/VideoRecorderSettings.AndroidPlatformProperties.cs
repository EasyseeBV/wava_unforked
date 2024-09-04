using UnityEngine;
using UnityEngine.Serialization;

namespace VoxelBusters.ScreenRecorderKit
{
	public partial class VideoRecorderSettings
	{
		/// <summary>
		/// Application Settings specific to Android platform.
		/// </summary>
		[System.Serializable]
		public class AndroidPlatformProperties
		{
            #region Fields

            [FormerlySerializedAs("m_videoMaxQuality")]
            [SerializeField]
            [Tooltip("Set the resolution at which you want to record. Setting higher resolution will have larger final video sizes.")]
            private     VideoRecorderQuality    m_videoQuality              = VideoRecorderQuality.QUALITY_720P;

            [SerializeField]
            [Tooltip("Enabling custom bitrates lets you set recommended bitrates compared to default values which give very big file sizes")]
            private     CustomBitRateSetting    m_customVideoBitrate        = null;

            [FormerlySerializedAs("m_allowExternalStoragePermission")]
            [SerializeField]
            [Tooltip("Enable this if you want to use SavePreview feature. This adds external storage permission to the manifest. Default is true.")]
            private     bool                    m_usesSavePreview           = true;

            [SerializeField]
            [Header("Advanced Settings")]
            [Tooltip("Enabling this will allow VideoRecorder to pause/resume audio sources to reduce load while starting/stopping recording. It is recommended to keep this setting on.")]
            private     bool                    m_allowControllingAudio     = true;

            [SerializeField]
            [Tooltip("This captures app audio better when enabled")]
            private     bool                    m_prioritiseAppAudioWhenUsingMicrophone = false;

            #endregion

            #region Properties

            /// <summary>
            /// Set the resolution at which you want to record. Setting higher resolution will have larger final video sizes.
            /// </summary>
            public VideoRecorderQuality VideoQuality => m_videoQuality;

            /// <summary>
            /// Enabling custom bitrates lets you set recommended bitrates compared to default values which give very big file sizes
            /// </summary>
            public CustomBitRateSetting CustomBitrateSetting => m_customVideoBitrate;

            /// <summary>
            /// Enable this if you want to use SavePreview feature. This adds external storage permission to the manifest. Default is true.
            /// </summary>
            public bool UsesSavePreview => m_usesSavePreview;

            /// <summary>
            /// Enabling this will allow VideoRecorder to pause/resume audio sources to reduce load while starting/stopping recording. It is recommended to keep this setting on.
            /// </summary>
            public bool AllowControllingAudio => m_allowControllingAudio;

            /// <summary>
            /// Enabling this captures app audio better through microphone when enabled
            /// </summary>
            public bool PrioritiseAppAudioWhenUsingMicrophone => m_prioritiseAppAudioWhenUsingMicrophone;

            #endregion

            #region Constructors

            public AndroidPlatformProperties(VideoRecorderQuality videoQuality = VideoRecorderQuality.QUALITY_720P, CustomBitRateSetting customBitRate = null,
                bool usesSavePreview = true, bool allowControllingAudio = true,
                bool prioritiseAppAudioWhenUsingMicrophone = false)
            {
                // set properties
                m_videoQuality                          = videoQuality;
                m_customVideoBitrate                    = customBitRate;
                m_usesSavePreview                       = usesSavePreview;
                m_allowControllingAudio                 = allowControllingAudio;
                m_prioritiseAppAudioWhenUsingMicrophone = prioritiseAppAudioWhenUsingMicrophone;
            }

            #endregion
        }
    }
}