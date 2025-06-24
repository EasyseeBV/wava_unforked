using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ARDownloadBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject content;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text statusLabel;

    public bool hasPresets { get; set; } = false;
    private int size = 0;

    private void Awake()
    {
        statusLabel.text = "";
        content.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Show()
    {
        content.gameObject.SetActive(true);
    }

    public void SetSize(int size)
    {
        this.size = size;
        slider.maxValue = size * 100 + (hasPresets ? 100 : 0);
        slider.value = 0;
    }
    
    public void UpdateProgress(int index, float value)
    {
        statusLabel.color = Color.black;
        statusLabel.text = "Artwork is loading...";
        float currentProgress = (hasPresets ? index + 1 : index) * 100;
        slider.value = (currentProgress + value);
    }

    public void FailedDownload()
    {
        statusLabel.color = Color.red;
        statusLabel.text = "Artwork download failed";
        Debug.LogError("Failed download.. UI Update needed");
    }

    public void FailedLoad()
    {
        statusLabel.color = Color.red;
        statusLabel.text = "Artwork loading failed.. redownloading..";
        Debug.LogError("Failed download.. UI Update needed");
    }
}