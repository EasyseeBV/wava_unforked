using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public class VideoRecorderRecordingAvailableResult : ScreenRecorderRecordingAvailableResult
    {
        #region Properties

        /// <summary>
        /// String that describes the reason for the state, if any.
        /// </summary>
        public string Path { get; private set; }

        #endregion

        #region Constructors

        public VideoRecorderRecordingAvailableResult(string path)
        {
            // set properties
            Path    = path;
        }

        #endregion

        #region Base class methods

        protected override object GetDataInternal() => Path;

        #endregion
    }
}