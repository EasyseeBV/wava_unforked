using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEnd : MonoBehaviour
{
    public void EndTutorial()
    {
        PlayerPrefs.SetInt("OpeningTutorial", 1);
        PlayerPrefs.Save();
    }
}
