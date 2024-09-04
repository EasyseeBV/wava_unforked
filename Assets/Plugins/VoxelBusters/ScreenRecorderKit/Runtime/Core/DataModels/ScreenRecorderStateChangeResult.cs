using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    internal class ScreenRecorderStateChangeResult
    {
        #region Properties

        public ScreenRecorderState State { get; private set; }

        public IScreenRecorder Recorder { get; private set; }

        #endregion

        #region Constructors

        public ScreenRecorderStateChangeResult(ScreenRecorderState state, IScreenRecorder recorder)
        {
            // set properties
            State       = state;
            Recorder    = recorder;
        }

        #endregion
    }
}