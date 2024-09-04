using UnityEngine;
using System;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Internal;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore
{
    public abstract class NativeVideoRecorderInterfaceBase : INativeVideoRecorderInterface
    {
        #region Fields

        #endregion

        #region IDispose implementation

        public virtual void Dispose()
        { }

        #endregion

        #region INativeObject implementation

        public NativeObjectRef NativeObjectRef => GetNativeObjectRefInternal();

        public virtual IntPtr AddrOfNativeObject() => IntPtr.Zero;

        protected virtual NativeObjectRef GetNativeObjectRefInternal() => null;

        #endregion

        #region INativeFeatureInterface implementation

        public bool IsAvailable => GetIsAvailableInternal();

        protected abstract bool GetIsAvailableInternal();

        #endregion

        #region INativeReportingKitInterface implementation

        public abstract bool CanRecord();

        public abstract bool IsRecording();

        public abstract bool IsRecordingAvailable();

        public abstract void PrepareRecording(CompletionCallback callback);
        public abstract void StartRecording(CompletionCallback callback);
        public abstract void PauseRecording(CompletionCallback callback);
        public abstract void ResumeRecording(CompletionCallback callback);
        public abstract void StopRecording(CompletionCallback callback);
        public abstract void DiscardRecording(CompletionCallback callback);
        public abstract void OpenRecording(CompletionCallback callback);
        public abstract void ShareRecording(string text, string subject, CompletionCallback callback);
        public abstract void SaveRecording(string filename, CompletionCallback<ScreenRecorderSaveRecordingResult> callback);
        public abstract void SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback);
        public abstract void SetRecordingStateChangeListener(IRecorderStateChangeListener listener);
        public abstract void Flush();

        #endregion
    }
}