using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.IO;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectSwipeDetector : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Tooltip("Minimum swipe speed (in pixels/sec) to be considered a quick swipe.")]
    public float swipeSpeedThreshold = 1000f;

    private Vector2 _startPointerPos;
    private float _startTime;
    private ScrollRect _scrollRect;

    public event Action OnSwipeUp;
    public event Action OnSwipeDown;

    void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Record where and when the drag started
        _startPointerPos = eventData.position;
        _startTime = Time.unscaledTime;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Calculate delta position and time
        Vector2 endPointerPos = eventData.position;
        float endTime = Time.unscaledTime;

        float deltaY = endPointerPos.y - _startPointerPos.y;
        float deltaTime = endTime - _startTime;
        if (deltaTime <= 0) return;

        // Compute swipe speed (abs)
        float swipeSpeed = Mathf.Abs(deltaY) / deltaTime;

        // If it exceeds our threshold, determine direction
        if (swipeSpeed >= swipeSpeedThreshold)
        {
            if (deltaY > 0)
                OnSwipeUp?.Invoke();
            else
                OnSwipeDown?.Invoke();
        }
    }
}
