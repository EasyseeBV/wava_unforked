using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Implements a video player with a play/pause button and a slider to seek through the video.
/// Use the SetVideoClip function to set the video clip to show.
/// </summary>
public class AdvancedVideoPlayer : MonoBehaviour
{
    [SerializeField]
    VideoPlayer _videoPlayer;

    [SerializeField]
    RawImage _videoRawImage;

    [SerializeField]
    AspectRatioFitter _videoRawImageAspect;

    [SerializeField]
    Button _playAndResumeButton;

    [SerializeField]
    Image _playAndResumeButtonImage;

    [SerializeField]
    Sprite _playIcon;

    [SerializeField]
    Sprite _pauseIcon;

    [SerializeField]
    Button _toggleControlsButton;

    [SerializeField]
    Slider _seekVideoSlider;

    [SerializeField]
    TextMeshProUGUI _timeText;

    [SerializeField]
    Image _blackOverlayImage;

    float _initialBlackOverlayAlpha;

    [SerializeField]
    DragDetector _sliderDragDetector;

    bool _videoWasPlayingWhenDragBegan;

    [SerializeField, Range(0, 3f)]
    float _hideControlsDelay;

    [SerializeField, Range(0, 1f)]
    float _blackOverlayAnimationDuration;

    float _hideControlsTime = Mathf.Infinity;

    RenderTexture _placeholderRenderTexture;

    RenderTexture _targetTexture;

    private void Awake()
    {
        // Store the initial alpha value of the black overlay. This alpha value is changed in animations.
        _initialBlackOverlayAlpha = _blackOverlayImage.color.a;

        // Create a placeholder render texture. The video player needs a target texture to prepare in the VideoRenderMode.RenderTexture
        _placeholderRenderTexture = new RenderTexture(4, 4, 0);
    }

    private void OnDestroy()
    {
        _placeholderRenderTexture.Release();
        Destroy(_placeholderRenderTexture);
    }

    void OnEnable()
    {
        // Add functionality to play/resume button.
        _playAndResumeButton.onClick.AddListener(OnPlayAndResumeButtonClicked);

        // Show or hide the controls when the player clicks anywhere on the image.
        _toggleControlsButton.onClick.AddListener(OnToggleControlsButtonClicked);

        // Tell the video player to show the controls once the loop point has been reached.
        _videoPlayer.loopPointReached += OnLoopPointReached;

        // As soon as the video player has prepared a video show the first frame.
        _videoPlayer.prepareCompleted += OnVideoPrepareCompleted;

        // Change the progress of the video when the slider value is changed.
        _seekVideoSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // Stop the video while the slider is being dragged.
        _sliderDragDetector._DragStarted += OnSliderDragStarted;
        _sliderDragDetector._DragEnded += OnSliderDragEnded;

    }

    private void OnDisable()
    {
        _playAndResumeButton.onClick.RemoveListener(OnPlayAndResumeButtonClicked);

        _toggleControlsButton.onClick.RemoveListener(OnToggleControlsButtonClicked);

        _videoPlayer.loopPointReached -= OnLoopPointReached;

        _videoPlayer.prepareCompleted -= OnVideoPrepareCompleted;

        _seekVideoSlider.onValueChanged.RemoveListener(OnSliderValueChanged);

        _sliderDragDetector._DragStarted -= OnSliderDragStarted;
        _sliderDragDetector._DragEnded -= OnSliderDragEnded;
    }

    void OnPlayAndResumeButtonClicked()
    {
        // If the video is playing then pause it. If the video is paused then resume playing.

        if (_videoPlayer.isPlaying)
        {
            _videoPlayer.Pause();

            // Update the play/resume button visuals.
            _playAndResumeButtonImage.sprite = _playIcon;

            // The video is not playing. Set the controls to not hide.
            _hideControlsTime = Mathf.Infinity;
        }
        else
        {
            _videoPlayer.Play();

            // Update the play/resume button visuals.
            _playAndResumeButtonImage.sprite = _pauseIcon;

            // The video is playing. Set to hide the controls after the specified delay.
            _hideControlsTime = Time.time + _hideControlsDelay;
        }
    }

    void OnToggleControlsButtonClicked()
    {
        // Show the controls if they are hidden, and hide them if they are shown.
        if (ControlsAreVisible())
        {
            HideControls();
        }
        else
        {
            ShowControls();

            // If the video is playing then hide the controls after some delay.
            if (_videoPlayer.isPlaying)
                _hideControlsTime = Time.time + _hideControlsDelay;
        }
    }

    void OnLoopPointReached(VideoPlayer source)
    {
        // Show the controls when the video finishes playing.
        ShowControls();

        // Set the controls to not hide automatically.
        _hideControlsTime = Mathf.Infinity;

        // Show the play icon.
        _playAndResumeButtonImage.sprite = _playIcon;
    }

    void OnVideoPrepareCompleted(VideoPlayer source)
    {
        // Hoist video dimensions for easy use.
        var videoWidth = (int)_videoPlayer.width;
        var videoHeight = (int)_videoPlayer.height;

        // Setup a render texture with the dimensions of the prepared video.
        // - Check if a previous render texture can be reused.
        if (_targetTexture != null && (_targetTexture.width != videoWidth || _targetTexture.height != videoHeight))
        {
            // Delete the previous render texture as its dimensions do not match.
            _targetTexture.Release();
            Destroy(_targetTexture);
            _targetTexture = null;
        }

        // - If no suitable render texture exists then create one.
        if (_targetTexture == null)
        {
            _targetTexture = new RenderTexture(videoWidth, videoHeight, 0);
        }

        // - Set the render texture to be used by the video player and the raw image that shows it.
        _videoPlayer.targetTexture = _targetTexture;
        _videoRawImage.texture = _targetTexture;


        // Adjust the aspect ratio of the raw image that shows the video to avoid stretching.
        _videoRawImageAspect.aspectRatio = (float)videoWidth / videoHeight;


        // The button should show a 'play' icon.
        _playAndResumeButtonImage.sprite = _playIcon;


        // Update the time text.
        UpdateTimeText();


        // Play and pause the player immediately to put the first frame of the video into the render texture.
        _videoPlayer.Play();
        _videoPlayer.Pause();
    }

    void OnSliderValueChanged(float value)
    {
        Debug.Log("Slider value changed!");

        // Set the video progress to the new slider value.
        _videoPlayer.time = _seekVideoSlider.value * _videoPlayer.length;

        // Because the player interacted with the controls: set a new time to hide them.
        _hideControlsTime = Time.time + _hideControlsDelay;
    }

    void OnSliderDragStarted()
    {
        // Remember if the video was playing.
        _videoWasPlayingWhenDragBegan = _videoPlayer.isPlaying;

        // Pause the video.
        _videoPlayer.Pause();
    }

    void OnSliderDragEnded()
    {
        // Play the video if the video was playing when the slider was dragged.
        if (_videoWasPlayingWhenDragBegan)
            _videoPlayer.Play();
    }

    /*
    public void SetVideoClip(VideoClip videoClip)
    {
        // Set the video of the videoplayer component.
        _videoPlayer.source = VideoSource.VideoClip;
        _videoPlayer.clip = videoClip;


        // Prepare the video for instant access.
        _videoPlayer.Prepare();
    }
    */

    public void SetVideoPath(string path)
    {
        // Set url for video player.
        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.url = path;

        // Set a placeholder render texture for the video.
        _videoPlayer.targetTexture = _placeholderRenderTexture;

        // Prepare the video as it is necessary when using a url.
        _videoPlayer.Prepare();
    }


    private void Update()
    {
        // Check if it is time to hide the controls.
        if (Time.time >= _hideControlsTime)
        {
            HideControls();

            _hideControlsTime = Mathf.Infinity;
        }


        // Update the transparency of the black overlay image.
        // - Calculate the change in alpha.
        var alphaDelta = _initialBlackOverlayAlpha * Time.deltaTime / _blackOverlayAnimationDuration;

        // - If the controls are visible then make the overlay more visible. Otherwise make it less visible.
        if (!ControlsAreVisible())
            alphaDelta = - alphaDelta;

        // - Set the new alpha value while clamping it.
        var overlayColor = _blackOverlayImage.color;
        overlayColor.a = Mathf.Clamp(overlayColor.a + alphaDelta, 0, _initialBlackOverlayAlpha);
        _blackOverlayImage.color = overlayColor;


        // Update the slider and the time stamp if the video is playing.
        if (_videoPlayer.isPlaying)
        {
            // Update the slider value.
            // - Calculate the relative progress of the played video.
            var videoProgress = 0f;

            if (_videoPlayer.length != 0)
                videoProgress = (float)_videoPlayer.time / (float)_videoPlayer.length;

            _seekVideoSlider.SetValueWithoutNotify(videoProgress);


            // Update the time text.
            UpdateTimeText();
        }
    }

    bool ControlsAreVisible()
    {
        // Any of the controls could be chosen here since they are all activated/deactivated at the same time.
        return _playAndResumeButton.gameObject.activeSelf;
    }

    void ShowControls()
    {
        _timeText.gameObject.SetActive(true);
        _playAndResumeButton.gameObject.SetActive(true);
        _seekVideoSlider.gameObject.SetActive(true);
    }

    void HideControls()
    {
        _timeText.gameObject.SetActive(false);
        _playAndResumeButton.gameObject.SetActive(false);
        _seekVideoSlider.gameObject.SetActive(false);
    }

    void UpdateTimeText() => _timeText.text = $"<b>{FormatTime(_videoPlayer.time)}</b> / {FormatTime(_videoPlayer.length)}";

    public static string FormatTime(double seconds)
    {
        // Calculate the number of minutes.
        var minutes = (int)(seconds / 60);

        // Calculate the number of leftover seconds.
        var secondsLeft = seconds - minutes * 60;

        // Format the string.
        var secondsText = $"{secondsLeft:0}";
        if (seconds < 10)
            secondsText = "0" + secondsText;

        return $"{minutes}:{secondsText}";
    }

#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(AdvancedVideoPlayer))]
    class AdvancedVideoPlayerEditor : Editor
    {
        VideoClip _videoClip;

        public override void OnInspectorGUI()
        {
            // Draw the default inspector UI.
            DrawDefaultInspector();

            // Add some space.
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");

            /*
            // Add a VideoClip field for testing.
            _videoClip = (VideoClip)EditorGUILayout.ObjectField(
                "Video Clip",
                _videoClip,
                typeof(VideoClip),
                false
            );

            // Add a button.
            if (GUILayout.Button("Set specified videoclip"))
            {
                AdvancedVideoPlayer myTarget = (AdvancedVideoPlayer)target;
                myTarget.SetVideoClip(_videoClip);
            }
            */
            
        }
    }
#endif
}
