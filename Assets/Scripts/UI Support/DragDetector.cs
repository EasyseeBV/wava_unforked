using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Implements ways to check if a UI element is being dragged.
/// </summary>
public class DragDetector : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Action _DragStarted;

    public Action _DragEnded;

    public bool IsDragged
    {
        get => _isDragged;
        private set => _isDragged = value;
    }

    bool _isDragged;

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _DragStarted?.Invoke();
        _isDragged = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _DragEnded?.Invoke();
        _isDragged = false;
    }
}
