using UnityEngine;
using UnityEngine.UI;

public class TextSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private RectTransform _slidingTransform;

    [SerializeField, Tooltip("Indicates that there is text before the shown text.")]
    private Image _leftIndicator;

    [SerializeField, Tooltip("Indicates that there is text after the shown text.")]
    private Image _rightIndicator;

    [Header("Animation Settings")]
    [SerializeField]
    private float _startForwardDelay = 1f;

    [SerializeField]
    private float _slideBackDelay = 1f;

    [SerializeField]
    private float _slideForwardSpeed = 100f;

    [SerializeField]
    private float _slideBackSpeed = 100f;

    [Header("Indicator Settings")]
    [SerializeField, Tooltip("The distance from the edge at which indicators start fading.")]
    private float _showIndicatorThreshold = 50f;

    private float _containerWidth;
    private float _textWidth;
    private float _timeSinceAnimationStart;
    private Vector2 _originalPosition;

    private enum SlideState { WaitingToSlide, SlidingForward, WaitingToReturn, SlidingBack }
    private SlideState _state;

    private bool _shouldAnimate = true;

    void Start()
    {
        this.InvokeNextFrame(() => ResetAnimation());
    }

    void ResetAnimation()
    {
        _textWidth = _slidingTransform.rect.width;
        _containerWidth = (_slidingTransform.parent as RectTransform).rect.width;

        _shouldAnimate = _textWidth > _containerWidth;

        if (!_shouldAnimate)
            return;

        _originalPosition = _slidingTransform.anchoredPosition;

        _timeSinceAnimationStart = 0f;
        _state = SlideState.WaitingToSlide;

        UpdateIndicators();
    }

    void Update()
    {
        if (!_shouldAnimate)
        {
            SetImageAlpha(_leftIndicator, 0f);
            SetImageAlpha(_rightIndicator, 0f);
            return;
        }

        _timeSinceAnimationStart += Time.deltaTime;

        Vector2 pos = _slidingTransform.anchoredPosition;

        switch (_state)
        {
            case SlideState.WaitingToSlide:
                if (_timeSinceAnimationStart >= _startForwardDelay)
                {
                    _state = SlideState.SlidingForward;
                }
                break;

            case SlideState.SlidingForward:
                pos.x -= _slideForwardSpeed * Time.deltaTime;

                if (Mathf.Abs(pos.x) >= _textWidth - _containerWidth)
                {
                    pos.x = -(_textWidth - _containerWidth);
                    _timeSinceAnimationStart = 0f;
                    _state = SlideState.WaitingToReturn;
                }
                _slidingTransform.anchoredPosition = pos;
                break;

            case SlideState.WaitingToReturn:
                if (_timeSinceAnimationStart >= _slideBackDelay)
                {
                    _state = SlideState.SlidingBack;
                }
                break;

            case SlideState.SlidingBack:
                pos.x += _slideBackSpeed * Time.deltaTime;

                if (pos.x >= _originalPosition.x)
                {
                    pos.x = _originalPosition.x;
                    _timeSinceAnimationStart = 0f;
                    _state = SlideState.WaitingToSlide;
                }
                _slidingTransform.anchoredPosition = pos;
                break;
        }

        UpdateIndicators();
    }

    void UpdateIndicators()
    {
        float distanceFromStart = Mathf.Abs(_slidingTransform.anchoredPosition.x);
        float distanceToEnd = Mathf.Abs((_textWidth - _containerWidth) - distanceFromStart);

        float beforeAlpha = Mathf.Clamp01(distanceFromStart / _showIndicatorThreshold);
        SetImageAlpha(_leftIndicator, beforeAlpha);

        float afterAlpha = Mathf.Clamp01(distanceToEnd / _showIndicatorThreshold);
        SetImageAlpha(_rightIndicator, afterAlpha);
    }

    void SetImageAlpha(Image img, float alpha)
    {
        if (img == null)
            return;

        var color = img.color;
        color.a = alpha;
        img.color = color;
    }
}