using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableSelectionBar : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private RectTransform container;

    [Header("Resize Limits")]
    [SerializeField] private float minHeight = 84f;
    [SerializeField] private float maxHeight = 128f;
    
    [Header("Resize Targets")]
    [SerializeField] private float targetClosedHeight = 84f;
    [SerializeField] private float targetOpenedHeight = 243f;

    [Header("Threshold (px)")]
    [SerializeField] private float threshold = 15f;

    [Header("Smoothing")]
    [Tooltip("Higher = snappier drag smoothing")]
    [SerializeField] private float lerpSpeed = 10f;

    [Header("Expand Tween")]
    [SerializeField] private float expandDuration = 0.3f;
    [SerializeField] private Ease expandEase = Ease.OutCubic;
    [SerializeField] private Ease collapseEase = Ease.OutCubic;

    // internal state
    private float startHeight;
    private Vector2 startPointerLocalPos;
    private float targetHeight;
    private float lastRawDelta;

    public event Action<bool> OnExpanding;
    public event Action<bool> OnExpanded;

    private void Start()
    {
        targetHeight = container.rect.height;
    }

    private void Update()
    {
        float current = container.rect.height;
        
        if (!Mathf.Approximately(current, targetHeight))
        {
            float next = Mathf.Lerp(current, targetHeight, lerpSpeed * Time.deltaTime);
            container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, next);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startHeight = container.rect.height;
        targetHeight = startHeight;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            container, eventData.position, eventData.pressEventCamera,
            out startPointerLocalPos
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            container, eventData.position, eventData.pressEventCamera,
            out Vector2 currentLocalPos
        );
        
        float deltaY = currentLocalPos.y - startPointerLocalPos.y;
        lastRawDelta = deltaY;

        float rawHeight = startHeight + deltaY;
        targetHeight = Mathf.Clamp(rawHeight, minHeight, maxHeight);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Mathf.Abs(lastRawDelta) >= threshold)
        {
            bool open = lastRawDelta > 0;
            targetHeight = open ? maxHeight : minHeight;
            Expand(open);
        }
        else targetHeight = startHeight;
    }

    private void Expand(bool open)
    {
        container.DOKill();

        float endHeight = open
            ? targetOpenedHeight
            : targetClosedHeight;
        
        targetHeight = endHeight; 

        Vector2 currentSize = container.sizeDelta;
        Vector2 targetSize  = new Vector2(currentSize.x, endHeight);

        OnExpanding?.Invoke(open);
        container.DOSizeDelta(targetSize, expandDuration).SetEase(open ? expandEase : collapseEase).OnComplete(() => OnExpanded?.Invoke(open));
    }
}
