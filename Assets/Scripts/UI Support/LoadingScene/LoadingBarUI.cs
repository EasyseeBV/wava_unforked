using UnityEngine;
using UnityEngine.UI;

public class LoadingBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Image _loadingBarImage;

    [SerializeField]
    LoadingProgressTracker _loadingTracker;

    [Header("Settings")]
    [SerializeField]
    float _animationSmoothTime;

    // Animation parameters
    float _velocity;

    float _targetValue;

    private void OnEnable()
    {
        _loadingTracker._ProgressChanged += OnProgressChanged;
        _loadingTracker._OnLoadingFinished += OnLoadingFinished;
    }

    private void OnDisable()
    {
        _loadingTracker._ProgressChanged -= OnProgressChanged;
        _loadingTracker._OnLoadingFinished -= OnLoadingFinished;
    }

    private void Update()
    {
        var current = _loadingBarImage.fillAmount;

        _loadingBarImage.fillAmount = Mathf.SmoothDamp(current, _targetValue, ref _velocity, _animationSmoothTime);
    }

    void OnProgressChanged(float progressPercentage)
    {
        _targetValue = progressPercentage;
    }

    void OnLoadingFinished()
    {

    }
}
