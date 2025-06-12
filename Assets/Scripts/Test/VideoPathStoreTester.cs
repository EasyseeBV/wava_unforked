using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VideoPathStoreTester : MonoBehaviour
{
    [SerializeField]
    TMP_InputField _pathInputField;

    [SerializeField]
    Button _storePathButton;

    [SerializeField]
    Button _loadAllPathsButton;

    private void Awake()
    {
        _storePathButton.onClick.AddListener(OnStorePathButtonClicked);

        _loadAllPathsButton.onClick.AddListener(OnLoadAllPathsButtonClicked);

        Debug.Log($"File is stored at {VideoPathStore.FilePath}");
    }

    void OnStorePathButtonClicked()
    {
        var path = _pathInputField.text;
        _pathInputField.text = string.Empty;
        VideoPathStore.StorePath(path);
        Debug.Log($"Stored path: {path}");
    }

    void OnLoadAllPathsButtonClicked()
    {
        var paths = VideoPathStore.ReadPaths();

        foreach (var path in paths)
        {
            Debug.Log($"Retrieved path: {path}");
        }
    }
}
