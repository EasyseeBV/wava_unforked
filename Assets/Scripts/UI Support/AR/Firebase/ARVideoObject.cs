using System;
using UnityEngine;
using UnityEngine.Video;

public class ARVideoObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject content;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private VideoPlayer videoPlayer;

    public void PrepareVideo(string url, Action<VideoPlayer> onComplete)
    {
        content.SetActive(true);
        videoPlayer.url = url;
        videoPlayer.Prepare();
        
        VideoPlayer.EventHandler handler = null;
        handler = (VideoPlayer vp) =>
        {
            videoPlayer.prepareCompleted -= handler;
            onComplete.Invoke(vp);
        };

        videoPlayer.prepareCompleted += handler;
    }

    public void Play()
    {
        meshRenderer.enabled = true;
        videoPlayer.Play();
    }
}
