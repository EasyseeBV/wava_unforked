using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore
{
    using Internal;
    using VoxelBusters.CoreLibrary;

    public class NullVideoRecorderInterface : NativeVideoRecorderInterfaceBase
    {
        #region Private Fields

        private static string kFeatureNotSupported = "Feature not supported";

        #endregion

        #region Private static methods

        private static void LogNotSupported()
        {
            Diagnostics.LogNotSupported("VideoRecorder");
        }

        #endregion

        #region Constructors

        public NullVideoRecorderInterface(VideoRecorderSettings settings, VideoRecorderRuntimeSettings runtimeSettings)
        { }

        #endregion

        #region Base class implementation

        protected override bool GetIsAvailableInternal() => false;

        public override bool IsRecordingAvailable()
        {
            LogNotSupported();
            return false;
        }

        public override bool IsRecording()
        {
            LogNotSupported();
            return false;
        }

        public override bool CanRecord()
        {
            LogNotSupported();
            return false;
        }

        public override void PrepareRecording(CompletionCallback callback)
        {
            LogNotSupported();
            callback?.Invoke(false, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain, kFeatureNotSupported));
        }

        public override void StartRecording(CompletionCallback callback)
        {
            LogNotSupported();
            callback?.Invoke(false, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain, kFeatureNotSupported));
        }

        public override void PauseRecording(CompletionCallback callback)
        {
            LogNotSupported();
            callback?.Invoke(false, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain, kFeatureNotSupported));
        }

        public override void ResumeRecording(CompletionCallback callback)
        {
            LogNotSupported();
            callback?.Invoke(false, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain, kFeatureNotSupported));
        }

        public override void StopRecording(CompletionCallback callback)
        {
            LogNotSupported();
            callback?.Invoke(false, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain, kFeatureNotSupported));
        }

        public override void OpenRecording(CompletionCallback callback)
        {
            LogNotSupported();
            callback?.Invoke(false, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain, kFeatureNotSupported));
        }

        public override void DiscardRecording(CompletionCallback callback)
        {
            LogNotSupported();
            callback?.Invoke(false, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain, kFeatureNotSupported));
        }


        public override void SaveRecording(string filename, CompletionCallback<ScreenRecorderSaveRecordingResult> callback)
        {
            LogNotSupported();
            callback?.Invoke(null, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain, kFeatureNotSupported));
        }

        public override void ShareRecording(string text, string subject, CompletionCallback callback)
        {
            LogNotSupported();
            callback?.Invoke(false, ScreenRecorderError.FeatureUnsupported(VideoRecorder.ErrorDomain, kFeatureNotSupported));
        }

        public override void SetRecordingStateChangeListener(IRecorderStateChangeListener listener)
        {
            LogNotSupported();
        }

        public override void SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback)
        {
            LogNotSupported();
        }

        public override void Flush()
        {
            LogNotSupported();
        }

        #endregion
    }
}