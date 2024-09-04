using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARGalleryPage : AnimateInfoBar
{
    [SerializeField] private PhotosPage photosPage;
    [SerializeField] private Button toggleButton;

    protected override void Awake()
    {
        base.Awake();
        toggleButton.onClick.AddListener(TogglePage);
    }

    private void TogglePage()
    {
        Animate();
    }

    protected override void StartRectAnimation(bool hide)
    {
        base.StartRectAnimation(hide);
        if(!hide) photosPage.Open();
    }

    protected override void StopRectAnmation(bool hidden)
    {
        base.StopRectAnmation(hidden);
        if(hidden) photosPage.Close();
    }
}
