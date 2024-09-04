using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemContainer : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI TmpTitle;
    public TextMeshProUGUI TmpDescription;

    public enum animationType { FadeOutLeft, FadeOutRight, FadeInLeft, FadeInRight};

    public void Init (Sprite sprite, string Title, string Description) {
        image.sprite = sprite;
        TmpTitle.text = Title;
        TmpDescription.text = Description;
    }

    public void Animate(animationType type) {
        switch (type) {
            case animationType.FadeOutLeft:
                break;
            case animationType.FadeOutRight:
                break;
            case animationType.FadeInLeft:
                break;
            case animationType.FadeInRight:
                break;
        }
    }
}
