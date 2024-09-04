#if UNITY_ANDROID
using UnityEngine;
using System.Collections;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Android
{
	/*public partial class VideoRecorderInterface
    {
        public void OnVideoRecorderPrepareRecordingStarted(string message)
        {
            SendPrepareRecordingStateChangeCallback(VideoRecorderPrepareRecorderState.Started);
        }

        public void OnVideoRecorderPrepareRecordingFinished(string message)
        {
            SendPrepareRecordingStateChangeCallback(VideoRecorderPrepareRecorderState.Finished);
        }

        public void OnVideoRecorderPrepareRecordingFailed(string message)
        {
            SendPrepareRecordingStateChangeCallback(VideoRecorderPrepareRecorderState.Failed, new Error(message));
        }

        public void OnVideoRecorderInitialiseSuccess(string message)
        {
            SendInitialiseCompleteCallback(success: true);
        }

        public void OnVideoRecorderInitialiseFailed(string message)
        {
            SendInitialiseCompleteCallback(success: false, error: new Error(message));
        }

        public void OnVideoRecorderRecordingStarted(string message)
        {
            if (m_allowControllingAudio)
            {
                ResumeAudio();
            }
            SendRecordingStateChangeCallback(VideoRecorderRecordingState.Started);
        }

        public void OnVideoRecorderRecordingStopped(string message)
        {
            if (m_allowControllingAudio)
            {
                ResumeAudio();
            }
            SendRecordingStateChangeCallback(VideoRecorderRecordingState.Stopped);
        }

        public void OnVideoRecorderRecordingAvailable(string message)
        {
            // This is required as in future we may have some video processing (Audio+Video Mux)
            SendRecordingStateChangeCallback(VideoRecorderRecordingState.Available);
        }

        public void OnVideoRecorderRecordingFailed(string message)
        {
            if (m_allowControllingAudio)
                ResumeAudio();

            SendRecordingStateChangeCallback(VideoRecorderRecordingState.Failed, new Error(message));
        }

        public void OnVideoRecorderRecordingOpened(string message)
        {
            SendPreviewStateChangeCallback(VideoRecorderPreviewState.Opened);
        }

        public void OnVideoRecorderRecordingClosed(string message)
        {
            SendPreviewStateChangeCallback(VideoRecorderPreviewState.Closed);
        }

        public void OnVideoRecorderRecordingShared(string message)
        {
            SendPreviewStateChangeCallback(VideoRecorderPreviewState.Shared);
        }

        public void OnVideoRecorderRecordingSaveSuccess(string message)
        {
            SendSavePreviewCompleteCallback(success: true);
        }

        public void OnVideoRecorderRecordingSaveFailed(string message)
        {
            //PREVIEW_UNAVAILABLE
            //PERMISSION_UNAVAILABLE
            SendSavePreviewCompleteCallback(success: false, error: new Error(message));
        }
    }*/
}
#endif