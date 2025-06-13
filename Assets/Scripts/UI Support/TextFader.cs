using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TextFader : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _text;

    [SerializeField]
    float _fadeDuration;

    string _nextText;

    bool _fadeIn = true;

    float _fadeProgress = 1;

    bool _autoFadeInAfterFadeOut;

    private void OnValidate()
    {
        if (_text == null)
            _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        // Update the text if it is completely hidden.
        if (_fadeProgress == 0)
        {
            _text.text = _nextText;

            // Fade in immediately if auto fade in is enabled.
            if (_autoFadeInAfterFadeOut)
            {
                _fadeIn = true;
                _autoFadeInAfterFadeOut = false;
            }
        }


        // Only adjust fade progress if necessary.
        if ((_fadeIn && _fadeProgress == 1) || (!_fadeIn && _fadeProgress == 0))
            return;


        // Adjust the fade progress.
        var delta = Time.deltaTime / _fadeDuration;
        _fadeProgress += _fadeIn ? delta : -delta;
        _fadeProgress = Mathf.Clamp01(_fadeProgress);


        UpdateTextAlpha();
    }

    public void SetNextText(string nextText)
    {
        _nextText = nextText;
    }

    /// <summary>
    /// Fades out, then fades in again with the next text.
    /// </summary>
    public void UpdateText()
    {
        _autoFadeInAfterFadeOut = true;
        _fadeIn = false;
    }

    public void FadeOut()
    {
        _fadeIn = false;

        // Reset the automatic fade in.
        _autoFadeInAfterFadeOut = false;
    }

    public void FadeOutImmediately()
    {
        _fadeIn = false;
        _fadeProgress = 0;
        _autoFadeInAfterFadeOut = false;
        UpdateTextAlpha();
    }

    public void FadeIn()
    {
        _fadeIn = true;

        // Reset the automatic fade in.
        _autoFadeInAfterFadeOut = false;
    }

    public void FadeInImmediately()
    {
        _fadeIn = true;
        _fadeProgress = 1;
        _autoFadeInAfterFadeOut = false;
        UpdateTextAlpha();
    }

    void UpdateTextAlpha() => _text.alpha = LeanTween.easeInOutSine(0, 1, _fadeProgress);


#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(TextFader))]
    class TextFaderEditor : Editor
    {
        string _nextText;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");

            _nextText = EditorGUILayout.TextField("Next text", _nextText);

            if (GUILayout.Button("Set next text"))
            {
                TextFader myTarget = (TextFader)target;
                myTarget.SetNextText(_nextText);
            }

            if (GUILayout.Button("Update text"))
            {
                TextFader myTarget = (TextFader)target;
                myTarget.UpdateText();
            }

            if (GUILayout.Button("Fade in"))
            {
                TextFader myTarget = (TextFader)target;
                myTarget.FadeIn();
            }

            if (GUILayout.Button("Fade in immediately"))
            {
                TextFader myTarget = (TextFader)target;
                myTarget.FadeInImmediately();
            }

            if (GUILayout.Button("Fade out"))
            {
                TextFader myTarget = (TextFader)target;
                myTarget.FadeOut();
            }

            if (GUILayout.Button("Fade out immediately"))
            {
                TextFader myTarget = (TextFader)target;
                myTarget.FadeOutImmediately();
            }
        }
    }
#endif
}
