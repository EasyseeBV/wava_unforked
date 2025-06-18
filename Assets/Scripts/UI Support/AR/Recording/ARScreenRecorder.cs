using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.ScreenRecorderKit;

public class ARScreenRecorder : MonoBehaviour
{
    #region Fields

    private IScreenRecorder m_recorder;

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
        m_recorder.SetOnRecordingAvailable(OnRecordingReady);
    }

    #endregion
    
    #region Query

    public void CanRecord()
    {
        bool canRecord = m_recorder.CanRecord();
    }

    public void IsRecording()
    {
        bool isRecording = m_recorder.IsRecording();
    }

    public bool CheckIsRecording()
    {
        return m_recorder.IsRecording();
    }

    public void IsPausedOrRecording()
    {
        bool isPausedOrRecording = m_recorder.IsPausedOrRecording();
    }

    #endregion

    #region Utilities

    public void ShareRecording()
    {
        m_recorder.ShareRecording(title: "Share Video", message: "Sharing a recorded video" ,callback: (success, error) =>
        {
            if (success)
            {
            }
            else
            {
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
            }
            else
            {
            }
        });
    }

    public void StartRecording()
    {
        if(!m_recorder.IsPausedOrRecording())
        {
        }

        m_recorder.StartRecording(callback: (success, error) =>
        {
            if (success)
            {
            }
            else
            {
            }
        });
    }

    public void PauseRecording()
    {
        m_recorder.PauseRecording((success, error) =>
        {
            if (success)
            {
            }
            else
            {
            }
        });
    }

    public void StopRecording(Action onComplete)
    {
        m_recorder.StopRecording((success, error) =>
        {
            if (success)
            {
                onComplete?.Invoke();
            }
            else
            {
            }
        });
    }

    public void OpenRecording()
    {
        m_recorder.OpenRecording((success, error) =>
        {
            if (success)
            {
            }
            else
            {
            }
        });
    }

    public string SaveRecording()
    {
        string videoSavePath = null;

        m_recorder.SaveRecording(null, (result, error) =>
        {
            if (error == null)
            {
                videoSavePath = result.Path;
            }
            else
            {
            }
        });

        return videoSavePath;
    }

    public void SaveRecording(Action<string> onComplete)
    {
        m_recorder.SaveRecording(null, (result, error) =>
        {
            if (error == null)
            {
                onComplete?.Invoke(result.Path);
            }
            else
            {
            }
        });
    }

    private void OnRecordingReady(ScreenRecorderRecordingAvailableResult result)
    {
        string srcPath = result.Data as string;
        if (string.IsNullOrEmpty(srcPath) || !File.Exists(srcPath))
        {
            Debug.LogError("No recording found to process.");
            return;
        }

        // copy to persistent data folder
        string filename    = Path.GetFileName(srcPath);
        string destPath    = Path.Combine(AppCache.GalleryFolder,  filename);
        File.Copy(srcPath, destPath, overwrite: true);

        Debug.Log($"Recording copied to: {destPath}");
    }
    
    public void DiscardRecording()
    {
        m_recorder.DiscardRecording(callback: (success, error) =>
        {
            if (success)
            {
            }
            else
            {
            }
        });
    }

    public void Flush()
    {
        m_recorder.Flush();
    }

    #endregion
}
