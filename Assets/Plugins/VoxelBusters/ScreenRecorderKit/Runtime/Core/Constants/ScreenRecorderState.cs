using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    internal enum ScreenRecorderState
    {
        Invalid = 0,

        Prepare,

        Record,

        Pause,

        Stop
    }
}