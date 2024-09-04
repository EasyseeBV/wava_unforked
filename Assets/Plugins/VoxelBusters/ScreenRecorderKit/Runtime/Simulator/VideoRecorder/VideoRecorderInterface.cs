using UnityEngine;
using System.Collections;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Simulator
{
    using Internal;
	public class VideoRecorderInterface : NativeVideoRecorderInterfaceBase
	{
        #region Fields

        private     ScreenRecorderState m_state     = ScreenRecorderState.Invalid;

        private     bool    m_isPreviewAvailable    = false;

        private     string  m_previewVideoFile      = "https://www.youtube.com/watch?v=aqz-KE-bpKQ";

        private IRecorderStateChangeListener m_stateChangeListener;
        private SuccessCallback<ScreenRecorderRecordingAvailableResult> m_availabilityListener;

        #endregion

        #region Constructors

        public VideoRecorderInterface(VideoRecorderSettings settings, VideoRecorderRuntimeSettings runtimeSettings)
        { }

        #endregion

        #region Base class implementation

        protected override bool GetIsAvailableInternal() => true;

        public override bool IsRecordingAvailable()
        {
            return m_isPreviewAvailable;
        }

        public override bool IsRecording()
        {
            return m_state == ScreenRecorderState.Record;
        }

        public override bool CanRecord()
        {
            return true;
        }

        public override void SetRecordingStateChangeListener(IRecorderStateChangeListener listener)
        {
            m_stateChangeListener = listener;
        }

        public override void SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> listener)
        {
            m_availabilityListener = listener;
        }

        public override void PrepareRecording(CompletionCallback callback)
        {
            if (m_state == ScreenRecorderState.Record)
            {
                callback?.Invoke(false, ScreenRecorderError.RecorderIsBusy(VideoRecorder.ErrorDomain));
                return;
            }

            m_state = ScreenRecorderState.Prepare;
            m_stateChangeListener?.OnPrepare();
            callback?.Invoke(true, null);
        }

        public override void StartRecording(CompletionCallback callback)
        {

            if (m_state == ScreenRecorderState.Record)
            {
                callback?.Invoke(false, ScreenRecorderError.RecorderIsBusy(VideoRecorder.ErrorDomain));
                return;
            }

            if (m_state != ScreenRecorderState.Prepare)
            {
                PrepareRecording((success, error) =>
                {
                    if(success)
                    {
                        StartRecordingInternal(callback);
                    }
                    else
                    {
                        callback?.Invoke(false, error);
                    }
                });
                return;
            }
            else
            {
                StartRecordingInternal(callback);
            }
        }

        public override void PauseRecording(CompletionCallback callback)
        {
            if (m_state == ScreenRecorderState.Record)
            {
                callback?.Invoke(false, ScreenRecorderError.ActiveRecordingUnavailable(VideoRecorder.ErrorDomain));
                return;
            }

            m_state = ScreenRecorderState.Pause;
            m_stateChangeListener?.OnPause();
            callback?.Invoke(true, null);
        }

        public override void ResumeRecording(CompletionCallback callback)
        {
            if (m_state != ScreenRecorderState.Record || m_state != ScreenRecorderState.Pause)
            {
                callback?.Invoke(false, ScreenRecorderError.ActiveRecordingUnavailable(VideoRecorder.ErrorDomain));
                return;
            }

            m_state = ScreenRecorderState.Record;
            m_stateChangeListener?.OnRecord();
            callback?.Invoke(true, null);
        }

        public override void StopRecording(CompletionCallback callback)
        {
            if (m_state != ScreenRecorderState.Record || m_state != ScreenRecorderState.Pause)
            {
                callback?.Invoke(false, ScreenRecorderError.ActiveRecordingUnavailable(VideoRecorder.ErrorDomain));
            }

            m_isPreviewAvailable    = true;

            m_state = ScreenRecorderState.Stop;
            m_stateChangeListener?.OnStop();
            callback?.Invoke(true, null);
            m_availabilityListener?.Invoke(new VideoRecorderRecordingAvailableResult(m_previewVideoFile));
        }

        public override void OpenRecording(CompletionCallback callback)
        {
            if(m_isPreviewAvailable)
            {
                Application.OpenURL(m_previewVideoFile);
                callback?.Invoke(true, null);
            }
            else
            {
                callback?.Invoke(false, ScreenRecorderError.ActiveRecordingUnavailable(VideoRecorder.ErrorDomain));
            }
        }

        public override void DiscardRecording(CompletionCallback callback)
        {
            if (m_state != ScreenRecorderState.Record || m_state != ScreenRecorderState.Pause)
            {
                callback?.Invoke(false, ScreenRecorderError.ActiveRecordingUnavailable(VideoRecorder.ErrorDomain));
            }
            m_state = ScreenRecorderState.Invalid;
            m_isPreviewAvailable    = false;
            callback?.Invoke(true, null);
        }

        public override void ShareRecording(string text, string subject, CompletionCallback callback)
        {
            if(!m_isPreviewAvailable)
            {
                callback?.Invoke(false, ScreenRecorderError.ActiveRecordingUnavailable(VideoRecorder.ErrorDomain));
                return;
            }

            callback?.Invoke(true, null);
        }

        public override void Flush()
        {
            m_stateChangeListener.OnInvalid();
        }

        public override void SaveRecording(string filename, CompletionCallback<ScreenRecorderSaveRecordingResult> callback)
        {
            if (!m_isPreviewAvailable)
            {
                callback?.Invoke(new ScreenRecorderSaveRecordingResult(null), ScreenRecorderError.ActiveRecordingUnavailable(VideoRecorder.ErrorDomain));
                return;
            }


            callback?.Invoke(null, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain));
        }

        private void StartRecordingInternal(CompletionCallback callback)
        {
            m_state = ScreenRecorderState.Record;
            m_stateChangeListener?.OnRecord();
            callback?.Invoke(true, null);
        }

        #endregion
    }
}