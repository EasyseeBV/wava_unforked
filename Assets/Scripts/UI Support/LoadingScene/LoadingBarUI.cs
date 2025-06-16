using UnityEngine;
using UnityEngine.UI;

public class LoadingBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Image _loadingBarImage;

    [SerializeField]
    DocumentLoadingTracker _loadingTracker;

    [Header("Settings")]
    [SerializeField]
    float _animationSmoothTime;

    // Animation parameters
    float _velocity;

    float _targetValue;

    private void Start()
    {
        _loadingTracker.ResetTracking();
    }

    private void OnEnable()
    {
        _loadingTracker._OnProgressChanged += OnProgressChanged;
        _loadingTracker._OnAllDocumentsLoaded += OnAllDocumentsLoaded;
    }

    private void OnDisable()
    {
        _loadingTracker._OnProgressChanged -= OnProgressChanged;
        _loadingTracker._OnAllDocumentsLoaded -= OnAllDocumentsLoaded;
    }

    private void Update()
    {
        // Debug
        //OnProgressChanged(Time.time * 0.1f);

        var current = _loadingBarImage.fillAmount;

        _loadingBarImage.fillAmount = Mathf.SmoothDamp(current, _targetValue, ref _velocity, _animationSmoothTime);
    }

    void OnProgressChanged(float progressPercentage)
    {
        _targetValue = progressPercentage;

        Debug.Log(progressPercentage);
    }

    void OnAllDocumentsLoaded()
    {

    }
}
