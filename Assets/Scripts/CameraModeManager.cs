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

    [SerializeField] private CameraMode mode;

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
                screenshotManager.Capture();
                break;
            case CameraMode.Video:
                if (screenRecorder.IsRecording())
                {
                    screenRecorder.StopRecording((success, error) => {
                        screenRecorder.SaveRecording();
                        button.color = Color.white;
                        ObjectToTurnOff.ForEach(o => o.SetActive(true));
                        ObjectToTurnOn.ForEach(o => o.SetActive(false));
                    });
                }
                else
                {
                    screenRecorder.StartRecording();
                    button.color = recordingColor;
                    ObjectToTurnOff.ForEach(o => o.SetActive(false));
                    ObjectToTurnOn.ForEach(o => o.SetActive(true));
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
