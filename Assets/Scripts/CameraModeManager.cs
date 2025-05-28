using System;
using AlmostEngine.Screenshot;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.ScreenRecorderKit.Demo;

public class CameraModeManager : MonoBehaviour
{
    [SerializeField] private ScreenRecorderDemo screenRecorder;
    [SerializeField] private ScreenshotManager screenshotManager;
    [SerializeField] private ScreenshotAR screenshotAR;

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
                //screenshotAR.Capture();
                screenshotManager.Capture();
                break;
            case CameraMode.Video:
                if (screenRecorder.CheckIsRecording())
                {
                    screenRecorder.StopRecording(() => {
                        screenRecorder.SaveRecording();
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
