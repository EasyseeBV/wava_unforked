using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserPhoto : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image photoImage;
    [SerializeField] private Button photoButton;

    private Sprite cachedSprite;

    private void Awake()
    {
        photoButton.onClick.AddListener(Open);
    }

    public void Init(Sprite sprite)
    {
        cachedSprite = sprite;
        photoImage.sprite = sprite;
    }

    private void Open()
    {
        if (ProfileUIManager.Instance == null) return;
        
        ProfileUIManager.Instance.photoDetails.gameObject.SetActive(true);
        ProfileUIManager.Instance.photoDetails.Open(cachedSprite);
    }
}
