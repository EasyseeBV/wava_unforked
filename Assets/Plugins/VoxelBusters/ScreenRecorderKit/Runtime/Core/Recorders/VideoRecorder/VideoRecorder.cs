using System;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.ScreenRecorderKit.VideoRecorderCore;
using VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Internal;

namespace VoxelBusters.ScreenRecorderKit
{
    public class VideoRecorder : MonoBehaviourZ, IScreenRecorder
    {
        #region Fields

        private     ScreenRecorderState     m_state;

        #endregion

        #region Static properties

        public static string ErrorDomain { get; private set; } = "VideoRecorder";

        public static string Name
        {
            get
            {
                return "VideoRecorder";
            }
        }

        #endregion

        #region Properties

        private INativeVideoRecorderInterface NativeInterface { get; set; }

        internal ScreenRecorderState State
        {
            get => m_state;
            private set => m_state = value;
        }

        #endregion

        #region Create methods

        public static VideoRecorder Create(params VideoRecorderRuntimeSettingsOption[] runtimeOptions)
        {
            return Create(settings: null, runtimeSettings: runtimeOptions);
        }

        public static VideoRecorder Create(VideoRecorderSettings settings = null, params VideoRecorderRuntimeSettingsOption[] runtimeOptions)
        {
            return Create(settings: settings, runtimeSettings: runtimeOptions);
        }

        public static VideoRecorder Create(VideoRecorderSettings settings = null, VideoRecorderRuntimeSettings runtimeSettings = null)
        {
            var     recorder                = FindObjectOfType<VideoRecorder>();
            if (recorder == null)
            {
                // create recorder object
                recorder                                    = new GameObject(Name).AddComponent<VideoRecorder>();
                recorder.CreateNativeInterface(settings ?? ScreenRecorderKitSettings.Instance.VideoRecorderSettings, runtimeSettings ?? new VideoRecorderRuntimeSettings());

                DontDestroyOnLoad(recorder.gameObject);
            }
            return recorder;
        }

        #endregion

        #region IScreenRecorder implementation

        public bool CanRecord()
        {
            return NativeInterface != null && NativeInterface.CanRecord();
        }

        public bool IsRecording() => NativeInterface != null && NativeInterface.IsRecording();

        public bool IsPausedOrRecording() => IsRecording() || (m_state == ScreenRecorderState.Pause);

        public void PrepareRecording(CompletionCallback callback = null)
        {
            NativeInterface.PrepareRecording((success, error) => CallbackDispatcher.InvokeOnMainThread(callback, success, error));
        }

        public void StartRecording(CompletionCallback callback = null)
        {
            if (State == ScreenRecorderState.Pause)
            {
                NativeInterface.ResumeRecording((success, error) => CallbackDispatcher.InvokeOnMainThread(callback, success, error));
            }
            else
            {
                NativeInterface.StartRecording((success, error) => CallbackDispatcher.InvokeOnMainThread(callback, success, error));
            }
        }

        public void PauseRecording(CompletionCallback callback)
        {
            NativeInterface.PauseRecording((success, error) => CallbackDispatcher.InvokeOnMainThread(callback, success, error));
        }

        public void StopRecording(CompletionCallback callback = null)
        {
            // Send native request
            NativeInterface.StopRecording((success, error) => CallbackDispatcher.InvokeOnMainThread(callback, success, error));
        }

        public void StopRecording(bool flushMemory, CompletionCallback callback = null)
        {
            if(flushMemory)
            {
                Flush();
            }

            StopRecording(callback);
        }

        public void DiscardRecording(CompletionCallback callback = null)
        {
            NativeInterface.DiscardRecording((success, error) => CallbackDispatcher.InvokeOnMainThread(callback, success, error));
        }

        public void SaveRecording(CompletionCallback<ScreenRecorderSaveRecordingResult> callback = null)
        {
            SaveRecording(null, callback);
        }

        public void SaveRecording(string fileName, CompletionCallback<ScreenRecorderSaveRecordingResult> callback = null)
        {
            // Native call
            NativeInterface.SaveRecording(
                filename: fileName,
                (result, error) => CallbackDispatcher.InvokeOnMainThread<ScreenRecorderSaveRecordingResult>(callback, result, error));
        }

        public void SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback)
        {
            NativeInterface.SetOnRecordingAvailable((result) => callback?.Invoke(result));
        }

        public void Flush()
        {
            NativeInterface.Flush();
        }

        #endregion

        #region Additional methods

        public bool IsRecordingAvailable()
        {
            return NativeInterface.IsRecordingAvailable();
        }

        public void OpenRecording(CompletionCallback callback = null)
        {
            NativeInterface.OpenRecording((success, error) => CallbackDispatcher.InvokeOnMainThread(callback, success, error));
        }

        public void ShareRecording(string text = null, string subject = null, CompletionCallback callback = null)
        {
            NativeInterface.ShareRecording(text, subject, (success, error) => CallbackDispatcher.InvokeOnMainThread(callback, success, error));
        }

        #endregion

        #region Base class methods

        protected override void Init()
        {
            base.Init();
            CallbackDispatcher.Initialize();
        }

        protected virtual void OnDisable()
        {
            if (IsPausedOrRecording())
            {
                DiscardRecording();
            }
        }

        protected virtual void OnDestroy()
        {
            if (IsPausedOrRecording())
            {
                DiscardRecording();
            }
        }

        #endregion

        #region Private methods

        private void CreateNativeInterface(VideoRecorderSettings settings, VideoRecorderRuntimeSettings runtimeSettings)
        {
            if (NativeInterface == null)
            {

                // check whether we can create interface object
                if (settings == null)
                {
                    throw new Exception("[VideoRecorder] The requested operation could not be completed. And the reason is that ScreenRecoderKitSettings file is not found. Please configure the plugin in order to use the functionalities.");
                }
                else if (!settings.IsEnabled)
                {
                    throw new Exception("[VideoRecorder] The requested operation could not be completed. And the reason is that the plugin is marked disabled. Please turn it ON in order to use the functionalities.");
                }


                // create interface object
                var targetPackage   = RuntimeConfiguration.VideoRecorder.GetPackageForPlatform(Application.platform);
                var     targetType  = ReflectionUtility.GetType(assemblyName: targetPackage.Assembly, typeName: targetPackage.NativeInterfaceType);

                NativeInterface = NativeFeatureActivator.CreateInterface<INativeVideoRecorderInterface>(RuntimeConfiguration.VideoRecorder, settings.IsEnabled, settings, runtimeSettings);

                NativeInterface.SetRecordingStateChangeListener(new NativeRecorderStateChangeListener()
                {
                    onPrepare   = () => m_state = ScreenRecorderState.Prepare,
                    onRecord    = () => m_state = ScreenRecorderState.Record,
                    onPause     = () => m_state = ScreenRecorderState.Pause,
                    onStop      = () => m_state = ScreenRecorderState.Stop,
                    onInvalid   = () => m_state = ScreenRecorderState.Invalid,
                });
            }
        }

        #endregion
    }
}