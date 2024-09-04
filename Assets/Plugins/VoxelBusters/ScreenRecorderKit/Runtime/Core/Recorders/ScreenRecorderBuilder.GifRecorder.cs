using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public partial class ScreenRecorderBuilder
    {
        #region Create methods

        /// <summary>
        /// Creates an instance of GIF recorder.
        /// </summary>
        /// <param name="camera">Camera which needs to be recorded. If null, whole screen will be recorded.</param>
        /// <param name="settings">Settings for configuring this gif recorder.</param>
        public static ScreenRecorderBuilder CreateGifRecorder(Camera camera = null, GifRecorderSettings settings = null)
        {
            var     instance    = (camera == null)
                ? ScreenGifRecorder.Create(settings) as GifRecorder
                : CameraGifRecorder.Create(camera, settings);
            return new ScreenRecorderBuilder(instance);
        }

        #endregion
    }
}