using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.ScreenRecorderKit.GifRecorderCore;

namespace VoxelBusters.ScreenRecorderKit
{
    public abstract class GifRecorder : MonoBehaviourZ, IScreenRecorder
    {
        #region Fields

        private     ObjectPool<RenderTexture>   m_renderTexturePool;

        private     List<RenderTexture>         m_frames;

        private     ScreenRecorderState         m_state;

        private     GifRecorderSettings         m_customSettings;

        private     GifRecorderSettings         m_settings;

        private     int                         m_width;

        private     int                         m_height;

        private     float                       m_timePerFrame;

        private     float                       m_timeSinceLastFrame;

        private     SuccessCallback<GifRecordingAvailableResult>    m_recordingAvailableCallback;

        #endregion

        #region Static properties

        public static string ErrorDomain { get; private set; } = "GifRecorder";

        public static string Name { get; private set; } = "GifRecorder";

        #endregion

        #region Properties

        internal ScreenRecorderState State => m_state;

        public GifRecorderSettings Settings
        {
            get
            {
                EnsureInitialized();

                return m_settings;
            }
            set
            {
                EnsureInitialized();

                if (IsPausedOrRecording())
                {
                    DebugLogger.LogWarning("[GifRecorder] The requested operation could not be completed.");
                    return;
                }

                // update value
                m_customSettings    = value;

                SetDirty();
            }
        }

        public float EstimatedMemoryUse
        {
            get
            {
                float   mem     = m_settings.FramePerSecond * m_settings.BufferSize;
                mem            *= m_width * m_height * 4;
                mem            /= 1024 * 1024;
                return mem;
            }
        }

        private GifTexture LastPreview { get; set; }

        #endregion

        #region Events

        internal static event EventCallback<ScreenRecorderStateChangeResult> OnRecorderStateChange;

        internal static event EventCallback<GifRecorderPreviewStateChangeResult> OnPreviewStateChange;

        internal static event EventCallback<ScreenRecorderSavePreviewStateChangeResult> OnSavePreviewStateChange;

        #endregion

        #region Base class methods

        protected override void Init()
        {
            base.Init();

            // Set properties
            m_renderTexturePool = new ObjectPool<RenderTexture>(
                createFunc: CreateRenderTexture,
                actionOnRelease: DestroyTexture);
            m_frames            = new List<RenderTexture>(capacity: 16);
            m_state             = 0;
            CallbackDispatcher.Initialize();
            SetDirty();
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

        #region IScreenRecorder implementation

        public bool CanRecord() => CanRecord(out Error error);

        public bool IsRecording() => (State == ScreenRecorderState.Record);

        public bool IsPausedOrRecording() => (State == ScreenRecorderState.Pause) || (State == ScreenRecorderState.Record);

        public void PrepareRecording(CompletionCallback callback = null)
        {
            EnsureInitialized();

            // Gaurd case
            if (IsRecording())
            {
                callback?.Invoke(success: false, error: ScreenRecorderError.RecorderIsBusy(ErrorDomain));
                return;
            }

            // Check whether recorder is already initailised
            if (!IsPrepareRecorderPending())
            {
                callback?.Invoke(success: true, error: null);
                return;
            }

            // We don't have to perform any specific action wrto preparing recorder
            // So we are mimicking this behaviour
            SetStateInternal(newState: ScreenRecorderState.Prepare);
            callback?.Invoke(success: true, error: null);
        }

        public void StartRecording(CompletionCallback callback = null)
        {
            EnsureInitialized();

            // Gaurd case
            if (!CanRecord(out Error error))
            {
                Debug.LogError($"[GifRecorder] The requested record operation could not be completed. The reason is: {error}");
                callback?.Invoke(success: false, error: error);
                return;
            }

            // Update state
            SetStateInternal(newState: ScreenRecorderState.Record);

            // Send completed callback
            callback?.Invoke(success: true, error: null);
        }

        public void PauseRecording(CompletionCallback callback = null)
        {
            EnsureInitialized();

            // Gaurd case
            if (!IsRecording())
            {
                callback?.Invoke(success: false, error: ScreenRecorderError.ActiveRecordingUnavailable(ErrorDomain));
                return;
            }

            // Update state
            SetStateInternal(newState: ScreenRecorderState.Pause);

            // Send completed callback
            callback?.Invoke(success: true, error: null);
        }

        public void StopRecording(CompletionCallback callback = null)
        {
            StopRecordingInternal(
                discard: false,
                flushMemory: false,
                callback: callback);
        }

        public void StopRecording(bool flushMemory, CompletionCallback callback = null)
        {
            StopRecordingInternal(
                discard: false,
                flushMemory: flushMemory,
                callback: callback);
        }

        public void DiscardRecording(CompletionCallback callback = null)
        {
            // Gaurd case
            if (!IsPausedOrRecording())
            {
                callback?.Invoke(success: false, error: ScreenRecorderError.ActiveRecordingUnavailable(ErrorDomain));
                return;
            }

            // Perform stop action
            StopRecordingInternal(
                discard: true,
                flushMemory: true,
                callback: callback);
        }

        public void SaveRecording(CompletionCallback<ScreenRecorderSaveRecordingResult> callback = null)
        {
            SaveRecording(fileName: null, callback: callback);
        }

        public void SaveRecording(string fileName, CompletionCallback<ScreenRecorderSaveRecordingResult> callback = null)
        {
            // Gaurd case
            if (LastPreview == null)
            {
                callback?.Invoke(result: null, error: ScreenRecorderError.ActiveRecordingUnavailable(ErrorDomain));
                return;
            }

            // Perform save action
            SaveRecording(
                texture: LastPreview,
                fileName: fileName,
                callback: (result, error) =>
                {
                    LastPreview = null;

                    // send callback
                    callback?.Invoke(result, error);
                });
        }

        void IScreenRecorder.SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback)
        {
            // Forward call
            SetOnRecordingAvailable((result) => callback?.Invoke(result));
        }

        public void Flush()
        {
            // Gaurd case
            if (IsPausedOrRecording())
            {
                DebugLogger.LogWarning("[GifRecorder] The requested operation could not be completed.");
                return;
            }

            // Release objects from the pool
            m_renderTexturePool.Reset();
            while (m_frames.Count != 0)
            {
                var     item    = m_frames.PopLast();
                DestroyTexture(item);
            }
        }

        #endregion

        #region Additional methods

        public void SetOnRecordingAvailable(SuccessCallback<GifRecordingAvailableResult> callback = null)
        {
            // Store reference
            m_recordingAvailableCallback    = callback;
        }

        #endregion

        #region Public methods

        public void SaveRecording(GifTexture texture, string fileName = null, CompletionCallback<ScreenRecorderSaveRecordingResult> callback = null)
        {
            // Check whether filename is provided
            if (string.IsNullOrEmpty(fileName))
            {
                fileName        = GenerateFileName();
            }

            // Ensure save directory is available
            string  saveFolder  = $"{ScreenRecorderKitSettings.PersistentDataPath}/GifImages";
            IOServices.CreateDirectory(saveFolder, overwrite: false);

            // Initiate export process
            string  savePath    = $"{saveFolder}/{fileName}.gif";
			var     encoder     = new GifEncoder(repeat: m_settings.Repeat, quality: m_settings.Quality);
			encoder.SetDelay(Mathf.RoundToInt(m_settings.TimePerFrame * 1000f));
			var     exportOp    = new GifExportOperation(
                encoder: encoder,
                texture: texture,
                savePath: savePath,
                priority: m_settings.ThreadPriority);
            exportOp.OnExportProgress   += (progress) =>
            {
                SendSavePreviewStateChangeResult(
                    state: ScreenRecorderSavePreviewState.InProgress,
                    path: savePath,
                    progress: progress);
            };
            exportOp.OnExportComplete   += (success, error) =>
            {
                // Check operation status
                if (success)
                {
                    // Update information
                    texture.Name    = fileName;
                    texture.Path    = savePath;
                    LastPreview     = texture;

                    // Send result to the callback function
                    CallbackDispatcher.InvokeOnMainThread(callback, new ScreenRecorderSaveRecordingResult(savePath), null);
                }
                else
                {
                    // Send result to the callback function
                    CallbackDispatcher.InvokeOnMainThread(callback, null, error);
                }

                SendSavePreviewStateChangeResult(
                    state: ScreenRecorderSavePreviewState.Done,
                    path: savePath,
                    progress: success ? 1f : 0f,
                    error: error);
            };
            SendSavePreviewStateChangeResult(state: ScreenRecorderSavePreviewState.Start, path: savePath);
			exportOp.Start();
        }

        public void ShareRecording(string text = null, string subject = null, CompletionCallback callback = null)
        {
            if (callback != null)
            {
                callback(false, ScreenRecorderError.FeatureUnsupported(ErrorDomain));
            }
        }

        public void OpenRecording(CompletionCallback callback = null)
        {
            if (callback != null)
            {
                callback(false, ScreenRecorderError.FeatureUnsupported(ErrorDomain));
            }
        }

        #endregion

        #region Behaviour methods

        public virtual bool CanRecord(out Error error)
        {
            // Set default value
            error = null;

            // Check whether recorder is available
            if (State == ScreenRecorderState.Record)
            {
                error = ScreenRecorderError.RecorderIsBusy(ErrorDomain);
                return false;
            }
            return true;
        }

        protected virtual bool IsVirtualRecorder() => false;

        protected virtual float AspectRatio()
        {
            return ((float)Screen.width / Screen.height);
        }

        #endregion

        #region Private methods

        private bool IsPrepareRecorderPending()
        {
            return (State == 0);
        }

        protected void UpdateFrameCaptureTimer(out bool canRecord)
        {
            // Set default value
            canRecord   = false;

            // Update ticker value
			m_timeSinceLastFrame       += Time.unscaledDeltaTime;
            if (m_timeSinceLastFrame >= m_timePerFrame)
			{
				m_timeSinceLastFrame   -= m_timePerFrame;
                canRecord               = true;
            }
        }

        private void StopRecordingInternal(bool discard, bool flushMemory,
            CompletionCallback callback = null)
        {
            // Gaurd case
            if (!IsPausedOrRecording())
            {
                Debug.LogError($"[GifRecorder] The requested stop operation could not be completed as there is no active recording.");
                callback?.Invoke(success: false, error: ScreenRecorderError.ActiveRecordingUnavailable(ErrorDomain, "No active recording available"));
                return;
            }

            // Update state
            SetStateInternal(newState: ScreenRecorderState.Stop);

            // Define stop actions
            System.Action<Error> stopAction = (anyError) =>
            {
                RecycleFrames();
                if (flushMemory)
                {
                    Flush();
                }

                // Send callback data
                callback?.Invoke(success: (anyError == null), error: anyError);
            };

            // Execute associated actions
            if (discard)
            {
                stopAction(null);
            }
            else
            {
                // Send prepare event
                var     prepareResult   = new GifRecorderPreviewStateChangeResult(state: ScreenRecorderPreviewState.Prepare);
                SendPreviewStateChangeResult(prepareResult);

                // Start convertion
                GifConverter.ConvertFramesToGifAsync(
                    frameTextures: m_frames.ToArray(),
                    name: GenerateFileName(),
                    width: m_width,
                    height: m_height,
                    fps: m_settings.FramePerSecond,
                    repeat: m_settings.Repeat,
                    quality: m_settings.Quality,
                    callback: (gif, error) =>
                    {
                        // Handle according to the operation result
                        if (error == null)
                        {
                            // Store recently saved recording
                            LastPreview     = gif;

                            // Send result to the completion handler method
                            stopAction(null);

                            // Send event data
                            var     readyResult     = new GifRecorderPreviewStateChangeResult(state: ScreenRecorderPreviewState.Ready, texture: gif);
                            SendPreviewStateChangeResult(readyResult);

                            var     availableResult = new GifRecordingAvailableResult(texture: gif);
                            SendRecordingAvailableResult(availableResult);
                        }
                        else
                        {
                            // Update preview reference to null
                            LastPreview     = null;

                            // Send result to the completion handler method
                            stopAction(error);

                            // Send error event data
                            var     errorResult     = new GifRecorderPreviewStateChangeResult(state: ScreenRecorderPreviewState.Error);
                            SendPreviewStateChangeResult(errorResult, error);
                        }
                    });
            }
        }

        private GifRecorder GetReference() => IsVirtualRecorder() ? null : this;

        private string GenerateFileName()
        {
            return $"Gif{System.DateTime.Now.ToString("yyyyMMddHHmmssffff")}";
        }

        private void SendPreviewStateChangeResult(GifRecorderPreviewStateChangeResult result, Error error = null)
        {
            CallbackDispatcher.InvokeOnMainThread(OnPreviewStateChange, result, error);
        }

        private void SendRecordingAvailableResult(GifRecordingAvailableResult result)
        {
            CallbackDispatcher.InvokeOnMainThread(m_recordingAvailableCallback, result);
        }

        private void SendSavePreviewStateChangeResult(ScreenRecorderSavePreviewState state, string path,
            float progress = 0f, Error error = null)
        {
            var     exportProgressResult    = new ScreenRecorderSavePreviewStateChangeResult(
                state: state,
                path: path,
                progress: progress);
            CallbackDispatcher.InvokeOnMainThread(OnSavePreviewStateChange, exportProgressResult, error);
        }

        #endregion

        #region State methods

        private void SetDirty()
        {
            // Update settings properties
            float   aspectRatio = AspectRatio();
            m_settings          = m_customSettings ?? ScreenRecorderKitSettings.Instance.GifRecorderSettings;
            m_width             = m_settings.Width;
            m_height            = m_settings.AutoAspect ? Mathf.RoundToInt(m_width / aspectRatio) : m_settings.Height;
            m_timePerFrame      = m_settings.TimePerFrame;

            // Reset cache
            if (m_frames != null)
            {
                Flush();
            }
        }

        private void SetStateInternal(ScreenRecorderState newState, bool sendEvent = true)
        {
            if (m_state == newState) return;

            // Store new value
            m_state     = newState;

            // Invoke state handle methods
            OnStateChange(newState);
            if (sendEvent)
            {
                var     result  = new ScreenRecorderStateChangeResult(state: newState, recorder: GetReference());
                CallbackDispatcher.InvokeOnMainThread(OnRecorderStateChange, result, null);
            }
        }

        internal virtual void OnStateChange(ScreenRecorderState state)
        {
            if (state == ScreenRecorderState.Record)
            {
                // hack to capture frames since, camera was set to record mode
                m_timeSinceLastFrame    = m_timePerFrame - Time.unscaledDeltaTime;
            }
        }

        #endregion

        #region RenderTexture handling methods

        protected RenderTexture GetOrCreateFrame()
        {
            // Check whether we have reusable texture
            RenderTexture   targetTexture;
            if (m_frames.Count == m_settings.MaxFrameCount)
            {
                targetTexture   = m_frames.PopFirst();
            }
            else
            {
                targetTexture   = m_renderTexturePool.Get();
            }
            return targetTexture;
        }

        protected void AddFrame(RenderTexture texture)
        {
            m_frames.AddLast(texture);
        }

        private RenderTexture CreateRenderTexture()
        {
            return new RenderTexture(m_width, m_height, 0, RenderTextureFormat.ARGB32)
            {
                wrapMode    = TextureWrapMode.Clamp,
                filterMode  = FilterMode.Bilinear,
                anisoLevel  = 0,
            }; 
        }

        private void RecycleFrames()
        {
            while (m_frames.Count > 0)
            {
                var     item    = m_frames.PopLast();
                m_renderTexturePool.Add(item);
            }
        }

        protected void DestroyTexture(Texture obj)
        {
            if (obj)
            {
                Destroy(obj);
            }
        }

        #endregion
    }
}