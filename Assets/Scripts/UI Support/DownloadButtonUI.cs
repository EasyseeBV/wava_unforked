using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DownloadButtonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    TextMeshProUGUI _buttonText;

    [SerializeField]
    Image _buttonBackgroundImage;

    [SerializeField]
    Image _iconImage;

    [SerializeField]
    GameObject _loadingIndicator;

    [SerializeField, Tooltip("Fill with the layout group.")]
    RectTransform _forceRebuildAfterChange;

    [Header("Button texts per state")]
    [SerializeField]
    string _notDownloadedText;

    [SerializeField]
    string _downloadingText;

    [SerializeField]
    string _downloadedText;

    [Header("Color tints per state")]
    [SerializeField]
    Color _notDownloadedTint;

    [SerializeField]
    Color _downloadedTint;

    [Header("Sprites per state")]
    [SerializeField]
    Sprite _downloadedButtonBackground;

    [SerializeField]
    Sprite _notDownloadedButtonBackground;

    [SerializeField]
    Sprite _downloadedIcon;

    [SerializeField]
    Sprite _notDownloadedIcon;

    public void ShowAsReadyForDownload()
    {
        _buttonText.text = _notDownloadedText;

        _buttonBackgroundImage.sprite = _notDownloadedButtonBackground;
        _buttonBackgroundImage.color = _notDownloadedTint;

        _iconImage.enabled = true;
        _iconImage.sprite = _notDownloadedIcon;

        _loadingIndicator.SetActive(false);

        // Necessary for the layout to account for the changed text size.
        LayoutRebuilder.ForceRebuildLayoutImmediate(_forceRebuildAfterChange);
    }

    public void ShowAsDownloading()
    {
        _buttonText.text = _downloadingText;

        _buttonBackgroundImage.sprite = _notDownloadedButtonBackground;
        _buttonBackgroundImage.color = _notDownloadedTint;

        _iconImage.enabled = false;

        _loadingIndicator.SetActive(true);

        // Necessary for the layout to account for the changed text size.
        LayoutRebuilder.ForceRebuildLayoutImmediate(_forceRebuildAfterChange);
    }

    public void ShowAsDownloadFinished()
    {
        _buttonText.text = _downloadedText;

        _buttonBackgroundImage.sprite = _downloadedButtonBackground;
        _buttonBackgroundImage.color = _downloadedTint;

        _iconImage.enabled = true;
        _iconImage.sprite = _downloadedIcon;

        _loadingIndicator.SetActive(false);

        // Necessary for the layout to account for the changed text size.
        LayoutRebuilder.ForceRebuildLayoutImmediate(_forceRebuildAfterChange);
    }

#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(DownloadButtonUI))]
    class DownloadButtonUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector UI.
            DrawDefaultInspector();

            // Add some space.
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");

            if (GUILayout.Button("Show as ready for download"))
            {
                DownloadButtonUI myTarget = (DownloadButtonUI)target;
                myTarget.ShowAsReadyForDownload();
            }

            if (GUILayout.Button("Show as downloading"))
            {
                DownloadButtonUI myTarget = (DownloadButtonUI)target;
                myTarget.ShowAsDownloading();
            }

            if (GUILayout.Button("Show as download finished"))
            {
                DownloadButtonUI myTarget = (DownloadButtonUI)target;
                myTarget.ShowAsDownloadFinished();
            }
        }
    }
#endif
}
