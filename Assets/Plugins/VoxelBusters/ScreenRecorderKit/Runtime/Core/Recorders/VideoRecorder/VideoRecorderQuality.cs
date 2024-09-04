using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public enum VideoRecorderQuality
    {
        /// <summary>
        /// Quality setting for matching the screen resolution
        /// </summary>
        QUALITY_MATCH_SCREEN_SIZE = -1,

        /// <summary>
        /// Quality setting for 1080p resolution
        /// </summary>
        QUALITY_1080P,

        /// <summary>
        /// Quality setting for 720p resolution
        /// </summary>
        QUALITY_720P,

        /// <summary>
        /// Quality setting for 480p resolution
        /// </summary>
        QUALITY_480P
    }
}