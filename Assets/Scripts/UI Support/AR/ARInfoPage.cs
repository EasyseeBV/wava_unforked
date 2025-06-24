using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ARInfoPage : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private GameObject content;
    [SerializeField] private Button toggleButton;
    [SerializeField] private ARStaticDetails arStaticDetails;
    [SerializeField] private RectTransform scrollRectTransform;
    [SerializeField] private ScrollRectSwipeDetector scrollRectSwipeDetector;

    private bool isOpen = false;
    private bool animating = false;
    
    protected void Awake()
    {
        toggleButton.onClick.AddListener(TogglePage);
    }

    private void OnEnable()
    {
        scrollRectSwipeDetector.OnSwipeUp += OnSwipeUp;
        scrollRectSwipeDetector.OnSwipeDown += OnSwipeDown;
        
    }

    private void TogglePage()
    {
        Animate();
    }

    private void Animate()
    {
        if (animating) return;
        
        content.SetActive(true);
        animating = true;
        
        if (isOpen)
        {
            // close
            scrollRectTransform.DOLocalMoveY(0, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                content.SetActive(false);
                animating = false;
                isOpen = false;
            });
        }
        else
        {
            // show
            scrollRectTransform.DOLocalMoveY(500, 0.6f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                animating = false;
                isOpen = true;
            });
        }
    }

    private void OnSwipeUp()
    {
        if (animating) return;
        
        content.SetActive(true);
        animating = true;
        
        // open
        scrollRectTransform.DOLocalMoveY(812, 0.6f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            animating = false;
            isOpen = true;
        });
    }

    private void OnSwipeDown()
    {
        if (animating) return;
        
        content.SetActive(true);
        animating = true;
        
        // close
        scrollRectTransform.DOLocalMoveY(0, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            content.SetActive(false);
            animating = false;
            isOpen = false;
        });
    }
}