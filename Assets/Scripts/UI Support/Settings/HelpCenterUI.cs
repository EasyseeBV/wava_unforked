using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpCenterUI : MonoBehaviour
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
    CanvasGroup _canvasGroup;

    [Header("Settings")]
    [SerializeField]
    List<string> _headingTexts;

    [SerializeField]
    List<string> _descriptionTexts;

    int _visibleSlideIndex;

    private void Awake()
    {
        _pointsAndLineUI.SetPointCount(GetSlideCount());
    }

    private void OnEnable()
    {
        // Subscribe to buttons.
        _forwardButton.onClick.AddListener(OnForwardButtonClicked);
        _backwardButton.onClick.AddListener(OnBackwardButtonClicked);
    }

    public void ShowPage()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void HidePage()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void ResetUI()
    {
        _visibleSlideIndex = 0;

        UpdateUI();

        _pointsAndLineUI.FinishAnimationsImmediately();
        _slidingImagesGallery.FinishAnimationsImmediately();
        _headingTextFader.UpdateTextImmediately();
        _descriptionTextFader.UpdateTextImmediately();
        _buttonCombination.HideBackwardButtonImmediately();
    }

    private void OnDisable()
    {
        _forwardButton.onClick.RemoveListener(OnForwardButtonClicked);
        _backwardButton.onClick.RemoveListener(OnBackwardButtonClicked);
    }

    void OnForwardButtonClicked()
    {
        // If it's the last slide: hide this page.
        if (_visibleSlideIndex == GetSlideCount() - 1)
        {
            HidePage();
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
            _buttonCombination.ShowForwardButtonAlternateText();
        else
            _buttonCombination.ShowForwardButtonDefaultText();
    }

    int GetSlideCount() => _headingTexts.Count;
}
