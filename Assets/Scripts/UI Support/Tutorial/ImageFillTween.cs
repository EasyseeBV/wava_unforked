using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFillTween : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image;

    [Header("Settings")]
    [SerializeField] [Range(0,1)] private float targetFillAmount;
    
    private const float fillSpeed = 0.35f;
    private float initialFillAmount;
    private float timer;

    private void Start()
    {
        FrameRateManager.SetHighFrameRate();

        initialFillAmount = image.fillAmount;
        timer = 0f;
        StartCoroutine(TweenFill());
    }
    
    private IEnumerator TweenFill()
    {
        while (timer < fillSpeed)
        {
            float t = timer / fillSpeed;
            float currentFillAmount = Mathf.Lerp(initialFillAmount, targetFillAmount, t);
            image.fillAmount = currentFillAmount;
            timer += Time.deltaTime;
            yield return null;
        }
        
        image.fillAmount = targetFillAmount;
    }

    private void OnValidate()
    {
        if (!image) image = GetComponent<Image>();
    }
}
