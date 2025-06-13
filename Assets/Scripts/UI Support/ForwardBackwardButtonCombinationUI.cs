using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ForwardBackwardButtonCombinationUI : MonoBehaviour
{
    [SerializeField]
    RectTransform _forwardButton;

    [SerializeField]
    RectTransform _backwardButton;

    // Button text related
    [SerializeField]
    TextFader _forwardButtonTextFader;

    [SerializeField]
    TextFader _backwardButtonTextFader;

    [SerializeField]
    string _forwardButtonAlternateText;

    string _forwardButtonDefaultText;

    // Button animation related
    [SerializeField, Range(0, 1)]
    float _animationDuration;

    float _forwardButtonLeftOffset;

    float _backwardButtonWidth;


    void Start()
    {
        // Store initial next button text.
        _forwardButtonDefaultText = _forwardButtonTextFader.GetComponent<TextMeshProUGUI>().text;


        // Store initial button layout values.
        _forwardButtonLeftOffset = _forwardButton.offsetMin.x;
        _backwardButtonWidth = _backwardButton.sizeDelta.x;


        HideBackwardButtonImmediately();
    }

    public void ShowBackwardButton()
    {
        LeanTween.cancel(gameObject);

        var from = _backwardButton.sizeDelta.x;
        var to = _backwardButtonWidth;

        LeanTween.value(gameObject, from, to, _animationDuration)
            .setOnUpdate((float val) =>
            {
                var sizeDelta = _backwardButton.sizeDelta;
                sizeDelta.x = val;
                _backwardButton.sizeDelta = sizeDelta;
            })
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                _backwardButtonTextFader.FadeIn();
            });


        from = _forwardButton.offsetMin.x;
        to = _forwardButtonLeftOffset;

        LeanTween.value(gameObject, from, to, _animationDuration)
            .setOnUpdate((float val) =>
            {
                var offsetMin = _forwardButton.offsetMin;
                offsetMin.x = val;
                _forwardButton.offsetMin = offsetMin;
            })
            .setEase(LeanTweenType.easeInOutQuad);
    }

    public void HideBackwardButton()
    {
        LeanTween.cancel(gameObject);

        _backwardButtonTextFader.FadeOut();

        var from = _backwardButton.sizeDelta.x;
        var to = 0;

        LeanTween.value(gameObject, from, to, _animationDuration)
            .setOnUpdate((float val) =>
            {
                var sizeDelta = _backwardButton.sizeDelta;
                sizeDelta.x = val;
                _backwardButton.sizeDelta = sizeDelta;
            })
            .setEase(LeanTweenType.easeInOutQuad);


        from = _forwardButton.offsetMin.x;
        to = 0;

        LeanTween.value(gameObject, from, to, _animationDuration)
            .setOnUpdate((float val) =>
            {
                var offsetMin = _forwardButton.offsetMin;
                offsetMin.x = val;
                _forwardButton.offsetMin = offsetMin;
            })
            .setEase(LeanTweenType.easeInOutQuad);
    }

    public void HideBackwardButtonImmediately()
    {
        LeanTween.cancel(gameObject);

        _backwardButtonTextFader.FadeOutImmediately();

        var offsetMin = _forwardButton.offsetMin;
        offsetMin.x = 0;
        _forwardButton.offsetMin = offsetMin;

        var sizeDelta = _backwardButton.sizeDelta;
        sizeDelta.x = 0;
        _backwardButton.sizeDelta = sizeDelta;
    }

    public void ShowForwardButtonAlternateText()
    {
        _forwardButtonTextFader.SetNextText(_forwardButtonAlternateText);
        _forwardButtonTextFader.UpdateText();
    }

    public void ShowForwardButtonDefaultText()
    {
        _forwardButtonTextFader.SetNextText(_forwardButtonDefaultText);
        _forwardButtonTextFader.UpdateText();
    }

#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(ForwardBackwardButtonCombinationUI))]
    class ForwardBackwardButtonCombinationUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");

            if (GUILayout.Button("Show backward button"))
            {
                var myTarget = (ForwardBackwardButtonCombinationUI)target;
                myTarget.ShowBackwardButton();
            }

            if (GUILayout.Button("Hide backward button"))
            {
                var myTarget = (ForwardBackwardButtonCombinationUI)target;
                myTarget.HideBackwardButton();
            }

            if (GUILayout.Button("Show forward button alternate text"))
            {
                var myTarget = (ForwardBackwardButtonCombinationUI)target;
                myTarget.ShowForwardButtonAlternateText();
            }

            if (GUILayout.Button("Show forward button default text"))
            {
                var myTarget = (ForwardBackwardButtonCombinationUI)target;
                myTarget.ShowForwardButtonDefaultText();
            }
        }
    }
#endif
}
