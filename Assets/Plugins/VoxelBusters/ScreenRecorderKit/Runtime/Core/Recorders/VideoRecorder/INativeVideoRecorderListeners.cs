using UnityEngine;
using System.Collections;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Internal
{
    public interface IRecorderStateChangeListener
    {
        void OnPrepare();
        void OnRecord();
        void OnPause();
        void OnStop();
        void OnInvalid();
    }
}