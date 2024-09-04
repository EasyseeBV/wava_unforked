using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore
{
    public enum VideoRecorderPrepareRecorderState
    {
        /// <summary>
        /// State when prepare recording starts
        /// </summary>
        Started = 0,

        /// <summary>
        /// State when prepare recording ends
        /// </summary>
        Finished,

        /// <summary>
        /// State when prepare recording fails
        /// </summary>
        Failed
    }
}