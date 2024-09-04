using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipTutorial : MonoBehaviour
{
    public bool ResetTutorialSkip = false;

    public GameObject TutorialCanvas;
    public GameObject LoadingCanvas;
    public SimpleSceneLoader sceneLoader;

    // Start is called before the first frame update
    void Awake()
    {
#if UNITY_EDITOR
        if (ResetTutorialSkip)
            PlayerPrefs.SetInt("Tutorial", 0);
#endif
        if (PlayerPrefs.GetInt("Tutorial") == 1) {
            Skip();
        } else {
            TutorialCanvas.SetActive(true);
        }
    }

    public void Skip() {
        TutorialCanvas.SetActive(false);
        LoadingCanvas.SetActive(true);
        PlayerPrefs.SetInt("Tutorial", 1);
        sceneLoader.LoadSceneAsync("Map");
    }
}
