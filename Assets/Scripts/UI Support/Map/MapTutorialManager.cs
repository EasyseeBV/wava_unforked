using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapTutorialManager : MonoBehaviour
{
    private const string TUTORIAL_FIRST_VISIT = "TutorialEnabled";
    private const string TUTORIAL_AR = "TutorialARCamera";
    
    [Header("References")]
    [SerializeField] private List<Sprite> tutorialSprites;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private GameObject tutorialArea;
    [Space]
    [SerializeField] private GameObject finalObjectToShow;

    [Header("Debugging")]
    [SerializeField] private bool alwaysStartTutorial;
    [SerializeField] private bool dontPromptTutorial;

    public static event Action OnTutorialEnded;
    public static bool TutorialActive = false;

    private int index = 0;

    private void Awake()
    {
        tutorialButton.onClick.AddListener(NextTutorial);
    }

    public void StartTutorial()
    {
        if (alwaysStartTutorial)
        {
            PlayerPrefs.SetInt(TUTORIAL_FIRST_VISIT, 0);
            PlayerPrefs.SetInt(TUTORIAL_AR, 0);
            PlayerPrefs.Save();
        }

        if (dontPromptTutorial) return;
        
        if (PlayerPrefs.GetInt(TUTORIAL_FIRST_VISIT, 0) == 0)
        {
            TutorialActive = true;
            tutorialArea.SetActive(true);
            index = 0;
            NextTutorial();
        }
        else
        {
            TutorialActive = false;
            tutorialArea.SetActive(false);
        }
    }

    public void ResetTutorialPrefs()
    {
        PlayerPrefs.SetInt(TUTORIAL_FIRST_VISIT, 0);
        PlayerPrefs.SetInt(TUTORIAL_AR, 0);
        PlayerPrefs.Save();
    }

    public void ForceStartTutorial()
    {
        ResetTutorialPrefs();
        
        tutorialArea.SetActive(true);
        index = 0;
        NextTutorial();
    }

    private void CloseAll()
    {
        //finalObjectToShow.SetActive(false);
    }

    private void NextTutorial()
    {
        TutorialActive = true;
        
        if (tutorialSprites.Count <= index)
        {
            EndTutorial();
            return;
        }
        
        tutorialImage.sprite = tutorialSprites[index];
        index++;
    }

    private void EndTutorial()
    {
        PlayerPrefs.SetInt(TUTORIAL_FIRST_VISIT, 1);
        PlayerPrefs.Save();
        
        finalObjectToShow.SetActive(true);
        Invoke(nameof(CloseAll), 3f);
        tutorialArea.SetActive(false);

        TutorialActive = false;
        OnTutorialEnded?.Invoke();
    }
}
