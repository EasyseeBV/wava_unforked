using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public class ScreenRecorderRecordingAvailableResult
    {
        #region Properties

        public object Data => GetDataInternal();

        #endregion

        #region Constructors

        protected ScreenRecorderRecordingAvailableResult()
        { }

        #endregion

        #region Private methods

        protected virtual object GetDataInternal() => null;

        #endregion
    }
}