using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARTutorialManager : MonoBehaviour
{
    
    private const string TUTORIAL_AR = "TutorialARCamera";
    private const string TUTORIAL_AR_PLACE = "TutorialARCameraPlace";
    private const string TUTORIAL_AR_CAMERA = "TutorialARCameraUsage";

    [Header("References")] 
    [SerializeField] private List<Button> tutorialImages;
    [SerializeField] private List<Button> tutorialImagesCamera;
    [SerializeField] private Button placeObjectTutorialHint;

    private int index = 0;
    private int cameraIndex = 0;

    public void Awake()
    {
        placeObjectTutorialHint.onClick.AddListener(() => placeObjectTutorialHint.gameObject.SetActive(false));
        placeObjectTutorialHint.gameObject.SetActive(false);
        
        foreach (var b in tutorialImages)
        {
            b.gameObject.SetActive(false);
            b.onClick.AddListener(NextTutorial);
        }
        
        foreach (var b in tutorialImagesCamera)
        {
            b.gameObject.SetActive(false);
            b.onClick.AddListener(NextTutorialCamera);
        }

        if (PlayerPrefs.GetInt(TUTORIAL_AR, 0) == 0)
        {
            index = 0;
            tutorialImages[index].gameObject.SetActive(true);
        }
    }

    public void ShowPlaceHint()
    {
        if (PlayerPrefs.GetInt(TUTORIAL_AR_PLACE, 0) == 0)
        {
            placeObjectTutorialHint.gameObject.SetActive(true);
            PlayerPrefs.SetInt(TUTORIAL_AR_PLACE, 1);
            PlayerPrefs.Save();
        }
    }

    private void NextTutorial()
    {
        tutorialImages[index].gameObject.SetActive(false);
        index++;

        if (tutorialImages.Count <= index)
        {
            PlayerPrefs.SetInt(TUTORIAL_AR, 1);
            PlayerPrefs.Save();
            index = 0;
            return;
        }
        
        tutorialImages[index].gameObject.SetActive(true);
    }

    private void NextTutorialCamera()
    {
        tutorialImagesCamera[cameraIndex].gameObject.SetActive(false);
        cameraIndex++;

        if (tutorialImagesCamera.Count <= cameraIndex)
        {
            PlayerPrefs.SetInt(TUTORIAL_AR_CAMERA, 1);
            PlayerPrefs.Save();
            cameraIndex = 0;
            return;
        }
        
        tutorialImagesCamera[cameraIndex].gameObject.SetActive(true);
    }

    public void StartCameraTutorial()
    {
        if (PlayerPrefs.GetInt(TUTORIAL_AR_CAMERA, 0) == 0)
        {
            cameraIndex = 0;
            tutorialImagesCamera[0].gameObject.SetActive(true);
        }
    }
}