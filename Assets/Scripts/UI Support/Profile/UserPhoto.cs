using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserPhoto : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image photoImage;
    [SerializeField] private Button photoButton;

    public Sprite CachedSprite { get; private set; }
    public string Path { get; private set; }
    public bool IsARView { get; set; } = false;

    private void Awake()
    {
        photoButton.onClick.AddListener(Open);
    }

    public void Init(Sprite sprite, string path)
    {
        Path = path;
        CachedSprite = sprite;
        photoImage.sprite = sprite;
    }

    private void Open()
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
