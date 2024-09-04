using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFillAndScaleTween : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image;
    [SerializeField] private RectTransform transformToScale;

    [Header("Settings")]
    [SerializeField] [Range(0,1)] private float targetFillAmount;
    
    private Vector4 targetPadding = new Vector4(0,0,0,0);
    private Vector4 initialPadding;
    
    private const float fillSpeed = 0.1f;
    private const float scaleSpeed = 0.3f;
    
    private float initialFillAmount;
    private float timer;

    private void Start()
    {
        initialPadding = new Vector4(
            transformToScale.offsetMin.x,
            transformToScale.offsetMax.x,
            -transformToScale.offsetMax.y,
            -transformToScale.offsetMin.y
        );
        
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
        StartCoroutine(TweenRectTransform());
    }
    
    private IEnumerator TweenRectTransform()
    {
        timer = 0;
        
        while (timer < scaleSpeed)
        {
            float t = timer / scaleSpeed;
            Vector4 currentPadding = Vector4.Lerp(initialPadding, targetPadding, t);

            // Calculate new offset values
            float newOffsetMinX = Mathf.Lerp(initialPadding.x, targetPadding.x, t);
            float newOffsetMaxX = Mathf.Lerp(initialPadding.y, targetPadding.y, t);
            float newOffsetMinY = Mathf.Lerp(initialPadding.z, targetPadding.z, t);
            float newOffsetMaxY = Mathf.Lerp(initialPadding.w, targetPadding.w, t);

            // Apply the new offset values
            transformToScale.offsetMin = new Vector2(newOffsetMinX, newOffsetMinY);
            transformToScale.offsetMax = new Vector2(newOffsetMaxX, newOffsetMaxY);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure the target padding is set accurately
        transformToScale.offsetMin = new Vector2(targetPadding.x, targetPadding.z);
        transformToScale.offsetMax = new Vector2(targetPadding.y, targetPadding.w);
        
        // Set the application framerate back to normal
        FrameRateManager.SetDefaultFrameRate();
    }

    private void OnValidate()
    {
        if (!image) image = GetComponent<Image>();
    }
}
