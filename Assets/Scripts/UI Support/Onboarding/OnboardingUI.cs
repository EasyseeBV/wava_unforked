using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnboardingUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Button _forwardButton;

    [SerializeField]
    Button _backwardButton;

    [SerializeField]
    ForwardBackwardButtonCombinationUI _buttonCombination;

    [SerializeField]
    PointsAndLineUI _pointsAndLineUI;

    [SerializeField]
    SlidingImageGalleryUI _slidingImagesGallery;

    [SerializeField]
    TextFader _headingTextFader;

    [SerializeField]
    TextFader _descriptionTextFader;

    [SerializeField]
    TextFader _hintTextFader;

    [SerializeField]
    GpsPermission _gpsPermission;

    [Header("Settings")]
    [SerializeField]
    List<string> _headingTexts;

    [SerializeField]
    List<string> _descriptionTexts;

    [SerializeField]
    int _followingSceneIndex;

    int _visibleSlideIndex;

    private void Awake()
    {
        _pointsAndLineUI.SetPointCount(GetSlideCount());
        _pointsAndLineUI.FinishAnimationsImmediately();

        _hintTextFader.FadeOutImmediately();
    }

    private void OnEnable()
    {
        // Subscribe to buttons.
        _forwardButton.onClick.AddListener(OnForwardButtonClicked);
        _backwardButton.onClick.AddListener(OnBackwardButtonClicked);


        _gpsPermission.EventWhenGotPermission.AddListener(OnGpsAccessGranted);
    }

    private void OnDisable()
    {
        _forwardButton.onClick.RemoveListener(OnForwardButtonClicked);
        _backwardButton.onClick.RemoveListener(OnBackwardButtonClicked);


        _gpsPermission.EventWhenGotPermission.RemoveListener(OnGpsAccessGranted);
    }

    void OnForwardButtonClicked()
    {
        // If it's the last slide: load another scene.
        if (_visibleSlideIndex == GetSlideCount() - 1)
        {
            SceneManager.LoadScene(_followingSceneIndex);
        }

        // If it's the first slide: ask for gps permission
        if (_visibleSlideIndex == 0)
        {
            _gpsPermission.AskForGps();
            return;
        }

        if (_visibleSlideIndex == 0)
            _buttonCombination.ShowBackwardButton();

        _visibleSlideIndex = Mathf.Clamp(_visibleSlideIndex + 1, 0, GetSlideCount() - 1);

        UpdateUI();
    }

    void OnBackwardButtonClicked()
    {
        if (_visibleSlideIndex == 1)
            _buttonCombination.HideBackwardButton();

        _visibleSlideIndex = Mathf.Clamp(_visibleSlideIndex - 1, 0, GetSlideCount() - 1);

        UpdateUI();
    }

    void OnGpsAccessGranted()
    {
        Debug.Assert(_visibleSlideIndex == 0);

        _buttonCombination.ShowBackwardButton();

        _visibleSlideIndex = 1;

        UpdateUI();
    }

    void UpdateUI()
    {
        _pointsAndLineUI.SetSelectedPointIndex(_visibleSlideIndex);

        _slidingImagesGallery.SetTargetImageIndex(_visibleSlideIndex);

        _headingTextFader.SetNextText(_headingTexts[_visibleSlideIndex]);
        _headingTextFader.UpdateText();

        _descriptionTextFader.SetNextText(_descriptionTexts[_visibleSlideIndex]);
        _descriptionTextFader.UpdateText();


        // Adjust text of next button.
        if (_visibleSlideIndex == GetSlideCount() - 1)
        {
            _buttonCombination.ShowForwardButtonAlternateText();
            _hintTextFader.FadeIn();
        }
        else
        {
            _buttonCombination.ShowForwardButtonDefaultText();
            _hintTextFader.FadeOut();
        }

    }

    int GetSlideCount() => _headingTexts.Count;
}
