using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.ScreenRecorderKit;

namespace VoxelBusters.ScreenRecorderKit.Demo
{
    public class ScreenRecorderDemo : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private     Text                m_statusText    = null;

        private     IScreenRecorder     m_recorder;

        #endregion

        #region Creation

        public void CreateVideoRecorder()
        {
            //Dispose if any recorder instance created earlier
            Cleanup();
            VideoRecorderRuntimeSettings settings = new VideoRecorderRuntimeSettings(enableMicrophone: true);
            ScreenRecorderBuilder builder = ScreenRecorderBuilder.CreateVideoRecorder(settings);
            m_recorder = builder.Build();

            //Register for recording path
            m_recorder.SetOnRecordingAvailable((result) =>
            {
                string path = result.Data as string;
                SetStatus($"File path: {path}");
            });

            SetStatus("Video Recorder Created");
        }

        public void CreateGifRecorder()
        {
            //Dispose if any recorder instance created earlier
            Cleanup();

            ScreenRecorderBuilder builder = ScreenRecorderBuilder.CreateGifRecorder();
            m_recorder = builder.Build();

            //Register for Gif Texture
            m_recorder.SetOnRecordingAvailable((result) =>
            {
                GifTexture gifTexture = result.Data as GifTexture;
                SetStatus($"Gif Texture : {gifTexture}");
            });

            SetStatus("Gif Recorder Created");
        }

        #endregion

        #region Query

        public void CanRecord()
        {
            bool canRecord = m_recorder.CanRecord();
            SetStatus($"Can record : {canRecord}");
        }

        public bool IsRecording()
        {
            bool isRecording = m_recorder.IsRecording();
            SetStatus($"Is currently recording: {isRecording}");
            return isRecording;
        }

        public void IsPausedOrRecording()
        {
            bool isPausedOrRecording = m_recorder.IsPausedOrRecording();
            SetStatus($"Is currently paused or recording: {isPausedOrRecording}");
        }

        #endregion

        #region Utilities

        public void ShareRecording()
        {
            m_recorder.ShareRecording(callback: (success, error) =>
            {
                if (success)
                {
                    SetStatus("Shared recording");
                }
                else
                {
                    SetStatus($"Failed to share recording [{error}]");
                }
            });
        }

        private void Cleanup()
        {
            if (m_recorder != null)
            {
                if (m_recorder.IsRecording())
                {
                    m_recorder.StopRecording();
                }

                m_recorder.Flush();
            }

            m_recorder = null;
        }

        private string GetRecorderName()
        {
            if (m_recorder is VideoRecorder)
                return VideoRecorder.Name;
            else
                return GifRecorder.Name;
        }

        #endregion

        #region Recording

        public void PrepareRecording()
        {
            m_recorder.PrepareRecording(callback: (success, error) =>
            {
                if (success)
                {
                    SetStatus("Prepare recording successful.");
                }
                else
                {
                    SetStatus($"Prepare recording failed with error [{error}]");
                }
            });
        }

        public void StartRecording()
        {
            m_recorder.StartRecording();
            SetStatus("Started Recording");
        }

        public void PauseRecording()
        {
            m_recorder.PauseRecording((success, error) =>
            {
                if (success)
                {
                    SetStatus("Paused recording");
                }
                else
                {
                    SetStatus($"Failed with error [{error}]");
                }
            });
        }

        public void StopRecording(CompletionCallback callback)
        {
            m_recorder.StopRecording((success, error) =>
            {
                callback(success, error);
                if (success)
                {
                    SetStatus("Stopped recording");
                }
                else
                {
                    SetStatus($"Failed with error: {error}");
                }
            });
        }

        public void OpenRecording()
        {
            m_recorder.OpenRecording((success, error) =>
            {
                if (success)
                {
                    SetStatus($"Open recording successful");
                }
                else
                {
                    SetStatus($"Open recording failed with error [{error}]");
                }
            });
        }

        public void SaveRecording()
        {
            m_recorder.SaveRecording(null, (result, error) =>
            {
                if(error == null)
                {
                    SetStatus("Saved recording successfully :" + result.Path);
                }
                else
                {
                    SetStatus($"Failed saving recording [{error}]");
                }
            });
        }

        public void DiscardRecording()
        {
            m_recorder.DiscardRecording(callback: (success, error) =>
            {
                if (success)
                {
                    SetStatus("Discard recording successful.");
                }
                else
                {
                    SetStatus($"Discard recording failed [{error}]");
                }
            });
        }

        public void Flush()
        {
            m_recorder.Flush();
        }

        #endregion

        #region UI

        private void SetStatus(string message)
        {
            Debug.Log($"[ScreenRecorder ({GetRecorderName()})] : {message}");

			if (m_statusText != null)
            {
                m_statusText.text = $"[{GetRecorderName()}] {message}";
            }
        }

        #endregion
    }
}