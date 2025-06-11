using System;
using System.Collections;
using System.Collections.Generic;
using Kamgam.InvertedMask;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ARTutorialManager : MonoBehaviour
{
    private const string TUTORIAL_AR = "TutorialARCameraGeneral";

    [Header("References")]
    [SerializeField] private Image tutorialImage;
    [SerializeField] private InvertedMaskHole invertedMask;
    [SerializeField] private TMP_Text tutorialLabel;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private GameObject qrScanOutline;
    [SerializeField] private GameObject infoButton;
    
    private int index = 0;
    private int cameraIndex = 0;

    public void Awake()
    {
        if (PlayerPrefs.GetInt(TUTORIAL_AR, 0) == 0)
        {
            StartCameraTutorial();
        }
        
        tutorialButton.onClick.AddListener(() =>
        {
            invertedMask.showMaskGraphic = false;
            tutorialLabel.text = "Scan the floor in front of you.";
            tutorialButton.gameObject.SetActive(false);
            qrScanOutline.SetActive(true);
            infoButton.SetActive(true);
        });
    }

    public void StartCameraTutorial()
    {
        invertedMask.showMaskGraphic = true;
        qrScanOutline.SetActive(false);
        tutorialLabel.text = "Use your camera to scan the floor in front of you. Once the floor is scanned the artwork will appear.";
        tutorialButton.gameObject.SetActive(true);
        infoButton.SetActive(false);
    }
}