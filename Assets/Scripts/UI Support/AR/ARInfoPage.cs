using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARInfoPage : AnimateInfoBar
{
    [SerializeField] private Button toggleButton;
    [SerializeField] private ARStaticDetails arStaticDetails;

    public bool CanOpen;
    
    protected override void Awake()
    {
        base.Awake();
        toggleButton.onClick.AddListener(TogglePage);
    }

    private void TogglePage()
    {
        if (!CanOpen) return;
        Animate();
    }

    protected override void StartRectAnimation(bool hide)
    {
        if (!hide)
        {
            Rect.gameObject.SetActive(true);
            arStaticDetails.Open(ArTapper.ARPointToPlace);
        }
        
        base.StartRectAnimation(hide);
    }
}