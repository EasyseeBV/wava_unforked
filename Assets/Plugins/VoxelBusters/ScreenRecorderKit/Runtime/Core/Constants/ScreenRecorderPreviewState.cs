using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    internal enum ScreenRecorderPreviewState
    {
        Prepare = 1,

        Ready,

        Saved,

        Error,
    }
}