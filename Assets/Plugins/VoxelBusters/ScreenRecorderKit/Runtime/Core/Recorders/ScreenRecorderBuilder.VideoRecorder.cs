using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public partial class ScreenRecorderBuilder
    {
        #region Create methods

        /// <summary>
        /// Creates an instance of the video recorder.
        /// </summary>
        /// <param name="settings">Basic settings properties.</param>
        /// <param name="runtimeSettingsOptions">Runtime configurable properties.</param>
        public static ScreenRecorderBuilder CreateVideoRecorder(VideoRecorderSettings settings = null, params VideoRecorderRuntimeSettingsOption[] runtimeSettingsOptions)
        {
            var     instance    = VideoRecorder.Create(
                settings: settings ?? ScreenRecorderKitSettings.Instance.VideoRecorderSettings,
                runtimeSettings: runtimeSettingsOptions);
            return new ScreenRecorderBuilder(instance);
        }

        /// <summary>
        /// Creates an instance of the video recorder.
        /// </summary>
        /// <param name="settings">Basic settings properties.</param>
        /// <param name="runtimeSettings">Runtime configurable properties.</param>
        public static ScreenRecorderBuilder CreateVideoRecorder(VideoRecorderSettings settings = null, VideoRecorderRuntimeSettings runtimeSettings = null)
        {
            var     instance    = VideoRecorder.Create(
                settings: settings ?? ScreenRecorderKitSettings.Instance.VideoRecorderSettings,
                runtimeSettings: runtimeSettings);
            return new ScreenRecorderBuilder(instance);
        }

        /// <summary>
        /// Creates an instance of the video recorder.
        /// </summary>
        /// <param name="runtimeSettings">Runtime configurable properties.</param>
        public static ScreenRecorderBuilder CreateVideoRecorder(VideoRecorderRuntimeSettings runtimeSettings)
        {
            var instance = VideoRecorder.Create(
                runtimeSettings: runtimeSettings);
            return new ScreenRecorderBuilder(instance);
        }

        /// <summary>
        /// Creates an instance of the video recorder.
        /// </summary>
        /// <param name="runtimeSettingsOptions">Runtime configurable properties.</param>
        public static ScreenRecorderBuilder CreateVideoRecorder(params VideoRecorderRuntimeSettingsOption[] runtimeSettingsOptions)
        {
            var instance = VideoRecorder.Create(
                runtimeSettings: runtimeSettingsOptions);
            return new ScreenRecorderBuilder(instance);
        }

        #endregion
    }
}