using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmoothScrollRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private ScrollRect scrollRect;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        FrameRateManager.SetHighFrameRate();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Call StartCoroutine to check if scrolling has stopped
        StartCoroutine(WaitUntilScrollingStops());
    }

    private IEnumerator WaitUntilScrollingStops()
    {
        // Wait until velocity of the ScrollRect is nearly zero
        while (scrollRect.velocity.magnitude > 0.1f)
        {
            yield return new WaitForEndOfFrame();
        }

        FrameRateManager.SetDefaultFrameRate();
    }
}
