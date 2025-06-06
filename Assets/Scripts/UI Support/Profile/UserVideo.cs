using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UserVideo : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Button openButton;

    public string Path { get;private set; }
    public bool IsARView { get; set; } = false;
    
    public void Init(string path)
    {
        Path = path;
        gameObject.SetActive(true);
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Path;

        // Ensure it doesn't play automatically
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;

        // Begin preparing (decoding) the video
        videoPlayer.Prepare();
        
        VideoPlayer.EventHandler handler = null;
        handler = (VideoPlayer vp) =>
        {
            vp.prepareCompleted -= handler;
            rawImage.texture = videoPlayer.texture;
            videoPlayer.Play();
            // Pause the player so it never auto-plays
            // videoPlayer.Pause();
        };

        videoPlayer.prepareCompleted += handler;

        // Start coroutine to wait until it's ready and then grab frame 0
        StartCoroutine(ApplyFirstFrameWhenReady());

        // Set the video to open when the button is pressed.
        openButton.onClick.RemoveAllListeners();
        openButton.onClick.AddListener(Open);
    }

    private IEnumerator ApplyFirstFrameWhenReady()
    {
        // Wait until the VideoPlayer has finished preparing
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // At this point, videoPlayer.texture contains the first frame
        // Assign it to the RawImage so you see the very first frame
        rawImage.texture = videoPlayer.texture;

        // Pause the player so it never auto-plays
        videoPlayer.Pause();
    }

    void Open()
    {
        if (!IsARView)
        {
            if (ProfileUIManager.Instance == null) return;
            
            ProfileUIManager.Instance.photoDetails.gameObject.SetActive(true);
            ProfileUIManager.Instance.photoDetails.SetVideoPlayerVideo(this);
        }
        else
        {
            if (ARPhotoViewer.Instance == null) return;
            
            ARPhotoViewer.Instance.Open(this);
        }
    }
}
