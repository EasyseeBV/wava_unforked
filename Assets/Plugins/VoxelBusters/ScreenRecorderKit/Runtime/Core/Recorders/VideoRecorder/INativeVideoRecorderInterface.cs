using UnityEngine;
using System.Collections;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.CoreLibrary;
using VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Internal;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore
{
    public interface INativeVideoRecorderInterface : INativeFeatureInterface
    {
        #region Methods

        // Query
        bool CanRecord();
        bool IsRecording();
        bool IsRecordingAvailable();

        // Setup listeners
        void SetRecordingStateChangeListener(IRecorderStateChangeListener listener);
        void SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback);

        // Actions
        void PrepareRecording(CompletionCallback callback);
        void StartRecording(CompletionCallback callback);
        void PauseRecording(CompletionCallback callback);
        void ResumeRecording(CompletionCallback callback);
        void StopRecording(CompletionCallback callback);
        void DiscardRecording(CompletionCallback callback);

        
        // Utilities
        void OpenRecording(CompletionCallback callback);
        void SaveRecording(string filename, CompletionCallback<ScreenRecorderSaveRecordingResult> callback);
        void ShareRecording(string text, string subject, CompletionCallback callback);
        void Flush();

        #endregion
    }
}