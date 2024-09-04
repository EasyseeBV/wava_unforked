using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore
{
    public delegate void OnRecordingStateChangeInternalDelegate(VideoRecorderRecordingState state, Error error);

    public delegate void OnRecordingAvailableInternalDelegate(VideoRecorderRecordingAvailableResult result, Error error);
}