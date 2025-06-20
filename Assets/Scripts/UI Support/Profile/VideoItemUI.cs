using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Each item in the gallery that is a video has this script attached.
/// It handles creating the preview image of the video for the gallery.
/// It also adds functionality to when the item is clicked.
/// </summary>
public class VideoItemUI : MonoBehaviour
{
    [SerializeField]
    Button openVideoButton;

    [SerializeField]
    RawImage _videoPreviewImage;

    [SerializeField]
    AspectRatioFitter _previewImageAspect;

    [SerializeField]
    TextMeshProUGUI _videoDurationText;

    VideoPlayer _videoPlayer;

    RenderTexture _videoPreviewRenderTexture;

    string _videoPath = null;

    private void Awake()
    {
        // Setup everything for when a video is set.
        // - First, create a video player component to copy the first frame of the video into a render texture.
        _videoPlayer = gameObject.AddComponent<VideoPlayer>();

        // - Setup the video player for our purposes.
        _videoPlayer.playOnAwake = false;
        _videoPlayer.waitForFirstFrame = true;

        // - Next, create a render texture to use as a target for the video player.
        // - - 400x225 preserves a 16:9 ratio and limits scaling of the video.
        _videoPreviewRenderTexture = new RenderTexture(400, 225, 0);

        // - Set the preview image to show the render texture.
        _videoPreviewImage.texture = _videoPreviewRenderTexture;

        // - Set the render texture as a target for the video player.
        _videoPlayer.targetTexture = _videoPreviewRenderTexture;

        // - Add functionality to open video button.
        openVideoButton.onClick.AddListener(OnOpenVideoButtonClicked);
    }

    private void OnDestroy()
    {
        // Delete the render texture that has been created.
        _videoPreviewRenderTexture.Release();
        Destroy(_videoPreviewRenderTexture);
    }

    public void SetVideoToShow(string videoPath)
    {
        // Prepare the preview image.
        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.url = videoPath;

        // - When the video is prepared then render the first frame.
        _videoPlayer.prepareCompleted += (_) =>
        {
            // Play and pause the video immediately to render the first frame to the render texture.
            _videoPlayer.Play();
            _videoPlayer.Pause();

            //Debug.Log($"Video width: {_videoPlayer.width}, height: {_videoPlayer.height}");

            // Set the aspect ratio of the preview image according to the aspect ratio of the video.
            _previewImageAspect.aspectRatio = (float)_videoPlayer.width / _videoPlayer.height;

            //StartCoroutine(ForceRebuildLayoutNextFrame(_videoPreviewImage.rectTransform));

            // Set the duration text.
            _videoDurationText.text = AdvancedVideoPlayer.FormatTime(_videoPlayer.length);

            // Don't destroy the video player here; that's too early. The frame will not be rendered.
        };

        // - Prepare the video.
        _videoPlayer.Prepare();


        // Store the video path.
        _videoPath = videoPath;
    }

    /*
    IEnumerator ForceRebuildLayoutNextFrame(RectTransform transform)
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
    }
    */

    void OnOpenVideoButtonClicked()
    {
        // Check if there is a video to play.
        if (_videoPath == null)
            return;


        // Tell the manager to open the video details page.
        if (ProfileUIManager.Instance == null)
            return;

        ProfileUIManager.Instance.galleryItemDetailsUI.OpenDetailsPage();
        ProfileUIManager.Instance.galleryItemDetailsUI.SetPathOfVideoToShow(_videoPath);
    }

#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(VideoItemUI))]
    class VideoItemUIEditor : Editor
    {
        string _videoPath;

        public override void OnInspectorGUI()
        {
            // Draw the default inspector UI.
            DrawDefaultInspector();

            // Add some space.
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");

            _videoPath = EditorGUILayout.TextField("Video path", _videoPath);

            // Add a button.
            if (GUILayout.Button("Set video path"))
            {
                Debug.Log($"Setting path: {_videoPath}");

                VideoItemUI myTarget = (VideoItemUI)target;
                myTarget.SetVideoToShow(_videoPath);
            }

        }
    }
#endif
}