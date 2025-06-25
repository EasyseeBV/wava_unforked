using UnityEditor;
using UnityEngine;

public class FullscreenViewer : MonoBehaviour
{
    [SerializeField]
    RectTransform _defaultContainer;

    [SerializeField]
    RectTransform _fullscreenContainer;

    [SerializeField]
    GameObject _fullscreenBackground;

    [SerializeField]
    RectTransform _content;

    [SerializeField]
    CanvasGroup _blackOverlay;

    [SerializeField]
    float _blackOverlayAnimationDuration;

    public bool IsInFullscreen => _content.parent == _fullscreenContainer;

    bool _enterFullscreen;

    void Update()
    {
        // Update black overlay animation.
        var alphaDelta = Time.deltaTime / _blackOverlayAnimationDuration;

        if (IsInFullscreen == _enterFullscreen)
            alphaDelta = -alphaDelta;

        _blackOverlay.alpha += alphaDelta;

        if (_blackOverlay.alpha == 1 && _enterFullscreen != IsInFullscreen)
        {
            _content.SetParent(_enterFullscreen ? _fullscreenContainer : _defaultContainer, false);

            _fullscreenBackground.SetActive(_enterFullscreen);
        }
    }

    public void EnterFullscreen()
    {
        _enterFullscreen = true;
    }

    public void ExitFullscreen()
    {
        _enterFullscreen = false;
    }

#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(FullscreenViewer))]
    class FullscreenViewerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector UI.
            DrawDefaultInspector();

            // Add some space.
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");

            if (GUILayout.Button("Enter fullscreen"))
            {
                var myTarget = (FullscreenViewer)target;
                myTarget.EnterFullscreen();
            }

            if (GUILayout.Button("Exit fullscreen"))
            {
                var myTarget = (FullscreenViewer)target;
                myTarget.ExitFullscreen();
            }
        }
    }
#endif
}
