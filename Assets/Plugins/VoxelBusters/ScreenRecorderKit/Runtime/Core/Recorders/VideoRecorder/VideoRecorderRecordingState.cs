using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public enum VideoRecorderRecordingState
    {
        /// <summary>
        /// State when recording starts
        /// </summary>
        Started = 0,


        /// <summary>
        /// State when recording pauses
        /// </summary>
        Paused,

        /// <summary>
        /// State when recording ends
        /// </summary>
        Stopped,

        /// <summary>
        /// State when recording fails
        /// </summary>
        Failed,

        /// <summary>
        /// Recording Available for preview after record action
        /// </summary>
        Available
    }
}