using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DeveloperButton : MonoBehaviour
{
    public Button ButtonRef => button;
    
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image outsideRingImage;

    [Header("Settings")]
    [SerializeField] private DeveloperModeARView.ARTransformView view;
    [SerializeField] private Color selectedColor = Color.green;
    
    private Action<DeveloperModeARView.ARTransformView, DeveloperButton> OnClickAction;

    public bool ToggledOn { get; set; } = false;
    
    private void Awake()
    {
        button.onClick.AddListener(OnClick);
    }

    public void AddOnClickListener(Action<DeveloperModeARView.ARTransformView, DeveloperButton> OnClickAction)
    {
        this.OnClickAction = OnClickAction;
    }

    private void OnClick()
    {
        ToggledOn = !ToggledOn;
        outsideRingImage.color = ToggledOn ? selectedColor : Color.white;
        OnClickAction?.Invoke(view, this);
    }

    public void Untoggle()
    {
        ToggledOn = false;
        outsideRingImage.color = Color.white;
    }
}