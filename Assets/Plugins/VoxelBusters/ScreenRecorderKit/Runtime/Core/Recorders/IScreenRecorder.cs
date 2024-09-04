using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    /// <summary>
    /// Interface for holding an instance of VideoRecorder or GidRecorder used for recording.
    /// </summary>
    [IncludeInDocs]
    public interface IScreenRecorder
    {
        #region Methods
        /// <summary>
        /// Check if recording is allowed or not.
        /// </summary>
        /// <returns>Returns false if recording api is not available or if resources are not available for recording.</returns>
        bool CanRecord();

        /// <summary>
        /// Check if recording is happening or not.
        /// </summary>
        /// <returns>Returns true if recording is active and not paused. False if no active recording exists or a recording is paused</returns>
        bool IsRecording();

        /// <summary>
        /// Check if an active recording exists
        /// </summary>
        /// <returns>Returns true if a recording is active or recording is paused. False if no active recording exists.</returns>
        bool IsPausedOrRecording();

        /// <summary>
        /// Prepare for recording
        /// </summary>
        /// <param name="callback">Callback to get triggered once prepare action is complete</param>
        void PrepareRecording(CompletionCallback callback = null);

        /// <summary>
        /// Start a recording
        /// </summary>
        /// <param name="callback">Callback to get triggered once start action is complete</param>
        void StartRecording(CompletionCallback callback = null);

        /// <summary>
        /// Pause an active recording
        /// </summary>
        /// <param name="callback">Callback to get triggered once after pause action is complete.</param>
        void PauseRecording(CompletionCallback callback = null);

        /// <summary>
        /// Stop an active recording
        /// </summary>
        /// <param name="callback">Callback to get triggered once after stop action is complete.</param>
        void StopRecording(CompletionCallback callback = null);

        /// <summary>
        /// Stop an active recording
        /// </summary>
        /// <param name="flushMemory">If true, clears all the memory allocated for this recording.</param>
        /// <param name="callback">Callback to get triggered once after stop action is complete.</param>
        void StopRecording(bool flushMemory, CompletionCallback callback = null);

        /// <summary>
        /// Discard an active recording. This won't trigger SetOnRecordingAvailable callback as the current recording is disposed.
        /// </summary>
        /// <param name="callback">Callback to get triggered once discard action is complete.</param>
        void DiscardRecording(CompletionCallback callback = null);

        /// <summary>
        /// Save an active recording
        /// </summary>
        /// <param name="callback">Callback to get triggered once save action is complete. This returns a result where you can fetch path.</param>
        void SaveRecording(CompletionCallback<ScreenRecorderSaveRecordingResult> callback = null);

        /// <summary>
        /// Save an active recording with a provided filename.
        /// </summary>
        /// <param name="fileName">Filename to be set on saving a recording.</param>
        /// <param name="callback">Callback to get triggered once save action is complete. This returns a result where you can fetch path.</param>
        void SaveRecording(string fileName, CompletionCallback<ScreenRecorderSaveRecordingResult> callback = null);


        /// <summary>
        /// Set a callback to get triggered when a recording result is available. This will be GifTexture in-case of gif recording or video file path in-case of video recording.
        /// </summary>
        /// <param name="callback">Callback to get triggered once a recording is available.</param>
        void SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback = null);

        /// <summary>
        /// Open an active recording which just got recorded.
        /// </summary>
        /// <param name="callback">Callback to get triggered once the action is complete.</param>
        void OpenRecording(CompletionCallback callback = null);

        /// <summary>
        /// Share an active recording which just got recorded.
        /// </summary>
        /// <param name="text">Share text used for sharing.</param>
        /// <param name="subject">Subject text that needs to be used as subject.</param>
        /// <param name="callback">Callback that gets triggered once share action is complete.</param>
        void ShareRecording(string text = null, string subject = null, CompletionCallback callback = null);

        /// <summary>
        /// Flush any resources created for recording.
        /// </summary>
        void Flush();

        #endregion
    }
}