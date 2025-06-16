using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;

public class ARScanTutorialHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image exitButtonImage;
    [SerializeField] private Image infoButtonImage;
    [SerializeField] private Image qrScannerImage;
    [SerializeField] private Image scanBackgroundImage;
    [SerializeField] private GameObject scanObject;
    [SerializeField] private CanvasGroup buttonGroups;
    [SerializeField] private TMP_Text tutorialLabel;
    [SerializeField] private TMP_Text scanFloorLabel;
    
    [Header("Dependencies")]
    [SerializeField] private Material blurMaterial;

    [Header("Settings")]
    [SerializeField] private float waitToHideTime = 1f;
    [SerializeField] private float screenFadeTime = 0.5f;
    [SerializeField] private float qrScanFadeTime = 0.1f;
    [SerializeField] private float buttonGroupFadeTime = 0.5f;

    private bool onTouch = false;
    
    private void OnEnable()
    {
        ArTapper.OnPlacementAccepted += OnTouch;
    }

    private void OnDisable()
    {
        ArTapper.OnPlacementAccepted -= OnTouch;
    }

    private void OnTouch()
    {
        if (onTouch) return;
        onTouch = true;
        
        string hex = "#00FFC5";
        ColorUtility.TryParseHtmlString(hex, out Color color);
        tutorialLabel.text = "Scan completed.";
        scanBackgroundImage.raycastTarget = false;
        scanFloorLabel.text = string.Empty;
        qrScannerImage.DOColor(color, qrScanFadeTime).OnComplete(() =>
        {
            StartCoroutine(ShowDefaultView());
        });
    }

    private IEnumerator ShowDefaultView()
    {
        yield return new WaitForSecondsRealtime(waitToHideTime);
        
        scanBackgroundImage.DOFade(0.5f, screenFadeTime).OnComplete(() =>
        {
            scanObject.gameObject.SetActive(false);
        });

        exitButtonImage.material = blurMaterial;
        infoButtonImage.material = blurMaterial;
        buttonGroups.DOFade(1f, buttonGroupFadeTime);
        buttonGroups.interactable = true;
    }
}
