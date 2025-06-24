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
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject scanTutorial;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private GameObject infoButton;
    
    private int index = 0;
    private int cameraIndex = 0;

    private bool forceTutorial = true;

    public void Awake()
    {
        if (PlayerPrefs.GetInt(TUTORIAL_AR, 0) == 0 || forceTutorial)
        {
            StartCameraTutorial();
        }
        else
        {
            content.SetActive(false);
            tutorialButton.gameObject.SetActive(false);
        }
        
        tutorialButton.onClick.AddListener(() =>
        {
            infoButton.SetActive(true);
            
            PlayerPrefs.SetInt(TUTORIAL_AR, 1);
            PlayerPrefs.Save();
            
            scanTutorial.SetActive(true);
            content.SetActive(false);
        });
    }

    public void StartCameraTutorial()
    {
        content.SetActive(true);
        infoButton.SetActive(false);
        scanTutorial.SetActive(false);
    }
}