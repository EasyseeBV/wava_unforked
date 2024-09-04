using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    /// <summary>
    /// Settings for configuring a video recorder.
    /// </summary>
    [IncludeInDocs]
    public class VideoRecorderRuntimeSettings
    {
        #region Properties

        /// <summary>
        /// Set this to true if you want to use microphone.
        /// </summary>
        public bool? EnableMicrophone { get; private set; }

        //private bool? ShowControls { get; private set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Create an instance of VideoRecorderRuntimeSettings by passing microphone status.
        /// </summary>
        /// <param name="enableMicrophone"></param>
        public VideoRecorderRuntimeSettings(bool? enableMicrophone = null/*, bool? showControls = null*/)
        {
            // Set properties
            EnableMicrophone    = enableMicrophone;
            //ShowControls        = showControls;
        }

        #endregion

        #region Operator methods

        public static implicit operator VideoRecorderRuntimeSettings(VideoRecorderRuntimeSettingsOption[] options)
        {
            var     newInstance = new VideoRecorderRuntimeSettings();
            foreach (var item in options)
            {
                if (item.OptionType == VideoRecorderRuntimeSettingsOption.RuntimeSettingsOptionType.EnableMicrophone)
                {
                    newInstance.EnableMicrophone    = item.BoolValue;
                }
                /*else if (item.OptionType == VideoRecorderRuntimeSettingsOption.RuntimeSettingsOptionType.ShowControls)
                {
                    newInstance.ShowControls        = item.BoolValue;
                }*/
            }
            return newInstance;
        }

        #endregion
    }
}