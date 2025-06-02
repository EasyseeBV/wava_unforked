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
        path = Path;
        videoPlayer.url = path;
    }

    public void Open()
    {
        if (!IsARView)
        {
            if (ProfileUIManager.Instance == null) return;
            
            ProfileUIManager.Instance.photoDetails.gameObject.SetActive(true);
            ProfileUIManager.Instance.photoDetails.Open(this);
        }
        else
        {
            if (ARPhotoViewer.Instance == null) return;
            
            ARPhotoViewer.Instance.Open(this);
        }
    }
}
