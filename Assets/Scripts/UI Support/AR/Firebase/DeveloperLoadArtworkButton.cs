using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperLoadArtworkButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    public void Populate(string artworkName, Action onClick)
    {
        label.text = artworkName;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
    }
}
