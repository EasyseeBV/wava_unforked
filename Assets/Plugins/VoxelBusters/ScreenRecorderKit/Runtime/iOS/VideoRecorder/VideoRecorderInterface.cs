#if UNITY_IOS
using UnityEngine;
using System.IO;
using VoxelBusters.CoreLibrary;
using VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Internal;
using System;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.iOS
{
    public partial class VideoRecorderInterface : NativeVideoRecorderInterfaceBase
    {
        #region Fields

        private IntPtr m_nativeHandle;

        #endregion


        #region Constructor

        public VideoRecorderInterface(VideoRecorderSettings settings, VideoRecorderRuntimeSettings runtimeSettings)
        {
            try
            {
                m_nativeHandle = ScreenRecorderKit_VideoRecorder_CreateWithSettings(new NativeVideoRecorderSettings(settings, runtimeSettings).NativeHandle);
            }
            catch (Exception e)
            {
                Debug.LogError("VideoRecorderInterface Exception Message : " + e.Message);
            }
        }

        #endregion


        #region Base class implementation

        public override bool CanRecord()
        {
            return ScreenRecorderKit_VideoRecorder_CanRecord(m_nativeHandle);
        }

        public override bool IsRecording()
        {
            return ScreenRecorderKit_VideoRecorder_IsRecording(m_nativeHandle);
        }

        public override bool IsRecordingAvailable()
        {
            return ScreenRecorderKit_VideoRecorder_IsRecordingAvailable(m_nativeHandle);
        }

        public override void SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback)
        {
            NativeRecordingAvailabilityListener listener = new NativeRecordingAvailabilityListener()
            {
                onAvailableCallback = (path) =>
                {
                    ScreenRecorderRecordingAvailableResult result = new VideoRecorderRecordingAvailableResult(path);
                    CallbackDispatcher.InvokeOnMainThread(callback, result);
                }
            };

            ScreenRecorderKit_VideoRecorder_SetRecordingAvailabilityListener(m_nativeHandle, listener.NativeHandle, NativeRecordingAvailabilityListener.RecordingOnAvailableCallback);
        }

        public override void SetRecordingStateChangeListener(IRecorderStateChangeListener listener)
        {

            NativeStateChangeListener nativeListener = new NativeStateChangeListener()
            {
                onInvalid = listener.OnInvalid,
                onPrepare = listener.OnPrepare,
                onRecord = listener.OnRecord,
                onPause = listener.OnPause,
                onStop = listener.OnStop

            };

            ScreenRecorderKit_VideoRecorder_SetRecorderStateChangeListener(m_nativeHandle, nativeListener.NativeHandle, NativeStateChangeListener.OnInvalid, NativeStateChangeListener.OnPrepare, NativeStateChangeListener.OnRecord, NativeStateChangeListener.OnPause, NativeStateChangeListener.OnStop);
        }

        public override void PrepareRecording(CompletionCallback callback)
        {
            ScreenRecorderKit_VideoRecorder_PrepareRecording(m_nativeHandle, GetNativeActionCompleteListener(callback).NativeHandle, NativeActionCompleteListener.OnSuccessCallback, NativeActionCompleteListener.OnFailureCallback);
        }

        public override void StartRecording(CompletionCallback callback)
        {
            ScreenRecorderKit_VideoRecorder_StartRecording(m_nativeHandle, GetNativeActionCompleteListener(callback).NativeHandle, NativeActionCompleteListener.OnSuccessCallback, NativeActionCompleteListener.OnFailureCallback);
        }

        public override void PauseRecording(CompletionCallback callback)
        {
            ScreenRecorderKit_VideoRecorder_PauseRecording(m_nativeHandle, GetNativeActionCompleteListener(callback).NativeHandle, NativeActionCompleteListener.OnSuccessCallback, NativeActionCompleteListener.OnFailureCallback);
        }

        public override void ResumeRecording(CompletionCallback callback)
        {
            ScreenRecorderKit_VideoRecorder_ResumeRecording(m_nativeHandle, GetNativeActionCompleteListener(callback).NativeHandle, NativeActionCompleteListener.OnSuccessCallback, NativeActionCompleteListener.OnFailureCallback);
        }

        public override void StopRecording(CompletionCallback callback)
        {
            ScreenRecorderKit_VideoRecorder_StopRecording(m_nativeHandle, GetNativeActionCompleteListener(callback).NativeHandle, NativeActionCompleteListener.OnSuccessCallback, NativeActionCompleteListener.OnFailureCallback);
        }

        public override void DiscardRecording(CompletionCallback callback)
        {
            ScreenRecorderKit_VideoRecorder_DiscardRecording(m_nativeHandle, GetNativeActionCompleteListener(callback).NativeHandle, NativeActionCompleteListener.OnSuccessCallback, NativeActionCompleteListener.OnFailureCallback);
        }

        public override void Flush()
        {
            ScreenRecorderKit_VideoRecorder_Flush(m_nativeHandle);
        }

        public override void OpenRecording(CompletionCallback callback)
        {
            ScreenRecorderKit_VideoRecorder_OpenRecording(m_nativeHandle, GetNativeActionCompleteListener(callback).NativeHandle, NativeActionCompleteListener.OnSuccessCallback, NativeActionCompleteListener.OnFailureCallback);
        }

        public override void SaveRecording(string filename, CompletionCallback<ScreenRecorderSaveRecordingResult> callback)
        {
            NativeSaveRecordingListener listener = new NativeSaveRecordingListener()
            {
                onSuccessCallback = (path) =>
                {
                    ScreenRecorderSaveRecordingResult result = new ScreenRecorderSaveRecordingResult(path);

                    CallbackDispatcher.InvokeOnMainThread(callback, result, null);
                },
                onFailureCallback = (int code, string description) =>
                {
                    CallbackDispatcher.InvokeOnMainThread(callback, null, new Error(VideoRecorder.ErrorDomain, code, description));
                }
            };

            ScreenRecorderKit_VideoRecorder_SaveRecording(m_nativeHandle, filename, listener.NativeHandle, NativeSaveRecordingListener.OnSuccessCallback, NativeSaveRecordingListener.OnFailureCallback);
        }

        public override void ShareRecording(string text, string subject, CompletionCallback callback)
        {
            ScreenRecorderKit_VideoRecorder_ShareRecording(m_nativeHandle, GetNativeActionCompleteListener(callback).NativeHandle, NativeActionCompleteListener.OnSuccessCallback, NativeActionCompleteListener.OnFailureCallback);
        }


        protected override bool GetIsAvailableInternal()
        {
            return true;
        }

        #endregion

        #region Private methods

        private NativeActionCompleteListener GetNativeActionCompleteListener(CompletionCallback callback)
        {
            NativeActionCompleteListener listener = new NativeActionCompleteListener()
            {
                onSuccessCallback = () =>
                {
                    CallbackDispatcher.InvokeOnMainThread(callback, true, null);
                },
                onFailureCallback = (int code, string description) =>
                {
                    CallbackDispatcher.InvokeOnMainThread(callback, false, new Error(VideoRecorder.ErrorDomain, code, description));
                }
            };

            return listener;
        }

        #endregion
    }
}
#endif