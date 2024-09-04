using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    internal class ScreenRecorderPreviewStateChangeResult
    {
        #region Properties

        public ScreenRecorderPreviewState State { get; private set; }

        public string Path { get; private set; }

        #endregion

        #region Constructors

        public ScreenRecorderPreviewStateChangeResult(ScreenRecorderPreviewState state, string path = null)
        {
            // set properties
            State       = state;
            Path        = path;
        }

        #endregion
    }
}