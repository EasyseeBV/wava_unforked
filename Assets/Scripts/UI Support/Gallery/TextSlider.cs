using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TextSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    TextMeshProUGUI _slidingText;

    [SerializeField, Tooltip("Indicates that there is text before the shown text.")]
    Image _leftIndicator;

    [SerializeField, Tooltip("Indicates that there is text after the shown text.")]
    Image _rightIndicator;

    [SerializeField]
    LayoutElement _textContainerLayoutElement;

    [Header("Animation Settings")]
    [SerializeField]
    float _startForwardDelay = 1f;

    [SerializeField]
    float _slideBackDelay = 1f;

    [SerializeField]
    float _slideForwardSpeed = 100f;

    [SerializeField]
    float _slideBackSpeed = 100f;

    [Header("Other Settings")]
    [SerializeField, Tooltip("The distance from the edge at which indicators start fading.")]
    float _showIndicatorThreshold = 50f;

    [SerializeField]
    int _maxWidth = 100;

    RectTransform slidingTransform {
        get => _slidingText.transform as RectTransform;
    }

    float _containerWidth;
    float _textWidth;
    float _timeSinceAnimationStart;
    Vector2 _originalPosition;

    enum SlideState { WaitingToSlide, SlidingForward, WaitingToReturn, SlidingBack }
    SlideState _state;

    bool _shouldAnimate = true;

    void Awake()
    {
        _originalPosition = slidingTransform.anchoredPosition;
    }

    public void SetTextAndResetAnimation(string text)
    {
        _slidingText.text = text;


        // Update layout.
        LayoutRebuilder.ForceRebuildLayoutImmediate(slidingTransform);

        // - If a text container layout element is given then adjust its size.
        if (_textContainerLayoutElement != null)
        {
            // - Wait until content size fitter on text updated.
            this.InvokeNextFrame(() =>
            {
                var preferredWidth = LayoutUtility.GetPreferredWidth(slidingTransform);

                _textContainerLayoutElement.preferredWidth = Mathf.Min(preferredWidth, _maxWidth);
            });

            // Wait for container layout update, then reset animation.
            this.InvokeAfterDelay(2, ResetAnimation);
        }
        else
        {
            this.InvokeNextFrame(ResetAnimation);
        }
    }

    public void ResetAnimation()
    {
        // Reset sliding transform position.
        slidingTransform.anchoredPosition = _originalPosition;


        // Get element dimensions.
        _textWidth = slidingTransform.rect.width;
        _containerWidth = (slidingTransform.parent as RectTransform).rect.width;


        // Check if animation necessary.
        _shouldAnimate = _textWidth > _containerWidth;

        if (_shouldAnimate)
        {
            // Reset animation parameters.
            _timeSinceAnimationStart = 0f;
            _state = SlideState.WaitingToSlide;
        }
        else
        {
            // Hide indicators.
            SetImageAlpha(_leftIndicator, 0f);
            SetImageAlpha(_rightIndicator, 0f);
        }
    }

    void Update()
    {
        if (!_shouldAnimate)
            return;

        _timeSinceAnimationStart += Time.deltaTime;

        Vector2 pos = slidingTransform.anchoredPosition;

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
                slidingTransform.anchoredPosition = pos;
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
                slidingTransform.anchoredPosition = pos;
                break;
        }

        UpdateIndicators();
    }

    void UpdateIndicators()
    {
        float distanceFromStart = Mathf.Abs(slidingTransform.anchoredPosition.x);
        float distanceToEnd = Mathf.Abs((_textWidth - _containerWidth) - distanceFromStart);

        float leftAlpha = Mathf.Clamp01(distanceFromStart / _showIndicatorThreshold);
        SetImageAlpha(_leftIndicator, leftAlpha);

        float rightAlpha = Mathf.Clamp01(distanceToEnd / _showIndicatorThreshold);
        SetImageAlpha(_rightIndicator, rightAlpha);
    }

    void SetImageAlpha(Image img, float alpha)
    {
        if (img == null)
            return;

        var color = img.color;
        color.a = alpha;
        img.color = color;
    }

#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(TextSlider))]
    class TextSliderEditor : Editor
    {
        string _textToSet;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");


            _textToSet = EditorGUILayout.TextField("Text to set", _textToSet);

            if (GUILayout.Button("Set text and reset animation"))
            {
                var myTarget = (TextSlider)target;
                myTarget.SetTextAndResetAnimation(_textToSet);
            }

            if (GUILayout.Button("Reset animation"))
            {
                var myTarget = (TextSlider)target;
                myTarget.ResetAnimation();
            }
        }
    }
#endif
}