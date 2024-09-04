#if UNITY_IOS
using System;
using System.Runtime.InteropServices;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.iOS
{
    public partial class VideoRecorderInterface : NativeVideoRecorderInterfaceBase
    {
        #region External methods

        [DllImport("__Internal")]
        public static extern int UnityShouldAutorotate();

        [DllImport("__Internal")]
        internal static extern IntPtr ScreenRecorderKit_VideoRecorder_CreateWithSettings(IntPtr settings);

        [DllImport("__Internal")]
        internal static extern bool ScreenRecorderKit_VideoRecorder_CanRecord(IntPtr owner);

        [DllImport("__Internal")]
        internal static extern bool ScreenRecorderKit_VideoRecorder_IsRecording(IntPtr owner);

        [DllImport("__Internal")]
        internal static extern bool ScreenRecorderKit_VideoRecorder_IsRecordingAvailable(IntPtr owner);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_SetRecordingAvailabilityListener(IntPtr owner, IntPtr listener, NativeRecordingAvailabilityListener.RecordingAvailabilityOnAvailableNativeCallback onAvailableCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_SetRecorderStateChangeListener(IntPtr owner, IntPtr listener, NativeStateChangeListener.StateChangeOnNewStateNativeCallback onInvalid, NativeStateChangeListener.StateChangeOnNewStateNativeCallback onPrepare, NativeStateChangeListener.StateChangeOnNewStateNativeCallback onRecord, NativeStateChangeListener.StateChangeOnNewStateNativeCallback onPause, NativeStateChangeListener.StateChangeOnNewStateNativeCallback onStop);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_PrepareRecording(IntPtr owner, IntPtr listener, NativeActionCompleteListener.ActionCompleteOnSuccessNativeCallback onSuccessCallback, NativeActionCompleteListener.ActionCompleteOnFailureNativeCallback onFailureCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_StartRecording(IntPtr owner, IntPtr listener, NativeActionCompleteListener.ActionCompleteOnSuccessNativeCallback onSuccessCallback, NativeActionCompleteListener.ActionCompleteOnFailureNativeCallback onFailureCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_PauseRecording(IntPtr owner, IntPtr listener, NativeActionCompleteListener.ActionCompleteOnSuccessNativeCallback onSuccessCallback, NativeActionCompleteListener.ActionCompleteOnFailureNativeCallback onFailureCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_ResumeRecording(IntPtr owner, IntPtr listener, NativeActionCompleteListener.ActionCompleteOnSuccessNativeCallback onSuccessCallback, NativeActionCompleteListener.ActionCompleteOnFailureNativeCallback onFailureCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_StopRecording(IntPtr owner, IntPtr listener, NativeActionCompleteListener.ActionCompleteOnSuccessNativeCallback onSuccessCallback, NativeActionCompleteListener.ActionCompleteOnFailureNativeCallback onFailureCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_DiscardRecording(IntPtr owner, IntPtr listener, NativeActionCompleteListener.ActionCompleteOnSuccessNativeCallback onSuccessCallback, NativeActionCompleteListener.ActionCompleteOnFailureNativeCallback onFailureCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_OpenRecording(IntPtr owner, IntPtr listener, NativeActionCompleteListener.ActionCompleteOnSuccessNativeCallback onSuccessCallback, NativeActionCompleteListener.ActionCompleteOnFailureNativeCallback onFailureCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_ShareRecording(IntPtr owner, IntPtr listener, NativeActionCompleteListener.ActionCompleteOnSuccessNativeCallback onSuccessCallback, NativeActionCompleteListener.ActionCompleteOnFailureNativeCallback onFailureCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_SaveRecording(IntPtr owner, string fileName, IntPtr listener, NativeSaveRecordingListener.SaveRecordingOnSuccessNativeCallback onSuccessCallback, NativeSaveRecordingListener.SaveRecordingOnFailureNativeCallback onFailureCallback);

        [DllImport("__Internal")]
        internal static extern void ScreenRecorderKit_VideoRecorder_Flush(IntPtr owner);

        #endregion
    }
}
#endif