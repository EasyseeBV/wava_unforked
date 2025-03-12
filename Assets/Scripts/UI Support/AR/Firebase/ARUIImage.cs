using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARUIImage : MonoBehaviour
{
    [SerializeField] private Image image;

    public void AssignSprite(Sprite sprite)
    {
        image.sprite = sprite;
        image.preserveAspect = true;
        image.enabled = false;
    }
    
    public void Show() => image.enabled = true;
}