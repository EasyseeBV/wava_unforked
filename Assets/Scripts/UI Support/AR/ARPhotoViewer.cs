using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityNative.Sharing;

public class ARPhotoViewer : MonoBehaviour
{
    public static ARPhotoViewer Instance;

    [SerializeField] private GameObject content;
    [SerializeField] private Image image;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button shareButton;

    private UserPhoto userPhoto;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        returnButton.onClick.AddListener(Return);
        shareButton.onClick.AddListener(Share);
        content.SetActive(false);
    }

    public void Open(UserPhoto photo)
    {
        content.SetActive(true);
        
        userPhoto = photo;
        image.sprite = photo.CachedSprite;
    }
    
    private void Return() => content.SetActive(false);

    private void Share()
    {
        if (!userPhoto) return;
        var share = UnityNativeSharing.Create();
        share.ShareScreenshotAndText("WAVA", userPhoto.Path);
    }
}
