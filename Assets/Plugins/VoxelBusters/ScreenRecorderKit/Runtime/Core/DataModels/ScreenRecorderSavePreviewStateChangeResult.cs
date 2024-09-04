using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    internal class ScreenRecorderSavePreviewStateChangeResult
    {
        #region Properties

        public ScreenRecorderSavePreviewState State { get; private set; }

        public string Path { get; private set; }

        public float Progress { get; private set; }

        public int ProgressPercentage => Mathf.RoundToInt(Progress * 100);

        #endregion

        #region Constructors

        public ScreenRecorderSavePreviewStateChangeResult(ScreenRecorderSavePreviewState state, string path,
            float progress = 0f)
        {
            // set properties
            State       = state;
            Path        = path;
            Progress    = progress;
        }

        #endregion
    }

}