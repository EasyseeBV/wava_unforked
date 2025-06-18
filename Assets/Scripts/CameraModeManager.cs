using System;
using AlmostEngine.Screenshot;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AlmostEngine.Examples;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.ScreenRecorderKit.Demo;

public class CameraModeManager : MonoBehaviour
{
    [SerializeField] private ScreenRecorderDemo screenRecorder;
    [SerializeField] private ScreenshotManager screenshotManager;
    [SerializeField] private ScreenshotAR screenshotAR;
    [SerializeField] private ScreenshotHandler screenshotHandler;

    [SerializeField] private CameraMode mode = CameraMode.Photo;

    [SerializeField] private Image button;
    [SerializeField] private Color recordingColor;
    [SerializeField] private List<GameObject> ObjectToTurnOff;
    [SerializeField] private List<GameObject> ObjectToTurnOn;
    
    private bool videoInitialized = false;

    public void Action()
    {
        switch (mode)
        {
            case CameraMode.Photo:
                Handheld.Vibrate();
                screenshotHandler.Capture();
                //screenshotAR.Capture();
                //screenshotManager.Capture();
                break;
            case CameraMode.Video:
                if (screenRecorder.CheckIsRecording())
                {
                    screenRecorder.StopRecording(() => {
                        string videoSavePath = screenRecorder.SaveRecording();

                        Debug.Log("videoSavePath: " +  videoSavePath);
                        
                        /*string path = Path.Combine(Application.persistentDataPath, "Gallery");
            
                        string directory = Path.GetDirectoryName(path);
                        if (!Directory.Exists(directory))
                        {
                            if (directory != null)
                            {
                                Directory.CreateDirectory(directory);
                                Debug.Log("Created new directory: " + directory);
                            }
                        }
                        
                        // copy videoSavePath to the path above
                        string fileName = Path.GetFileName(videoSavePath);
                        string destinationPath = Path.Combine(path, fileName);
                        
                        try
                        {
                            File.Copy(videoSavePath, destinationPath, true); // true to overwrite if file exists
                            Debug.Log("Video copied to: " + destinationPath);
                            
                            // If the video has been saved then persistently store its path.
                            VideoPathStore.StorePath(destinationPath);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Failed to copy video: " + e.Message);
                        }*/

                        if (!string.IsNullOrEmpty(videoSavePath)) VideoPathStore.StorePath(videoSavePath);
                        button.color = Color.white;
                        ObjectToTurnOff.ForEach(o => o.SetActive(true));
                        ObjectToTurnOn.ForEach(o => o.SetActive(false));

                    });
                }
                else
                {
                    try
                    {
                        screenRecorder.StartRecording();
                        if (button) button.color = recordingColor;
                        ObjectToTurnOff?.ForEach(o => o.SetActive(false));
                        ObjectToTurnOn?.ForEach(o => o.SetActive(true));
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Failed to start recording: " + e);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void SetPhoto()
    {
        mode = CameraMode.Photo;
    }

    public void SetVideo()
    {
        mode = CameraMode.Video;
        if (videoInitialized) return;
        screenRecorder.CreateVideoRecorder();
        screenRecorder.PrepareRecording();
        videoInitialized = true;
    }

    public enum CameraMode
    {
        Photo, Video
    }
}
