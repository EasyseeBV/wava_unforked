using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadingProgressTracker : MonoBehaviour
{
    [SerializeField]
    List<float> _approximateLoadingStepDurations;

    float _approximateTotalLoadingTime;

    float _accumulatedLoadingTimes;

    int _currentLoadingStepIndex;

    int StepCount => _approximateLoadingStepDurations.Count;

    public Action<float> _ProgressChanged;

    public Action _OnLoadingFinished;

    bool _finishedloading;

    private void OnEnable()
    {
        FirebaseLoader.OnStartedLoading += OnStartedLoading;
        FirebaseLoader.OnCompletedLoadingStep += OnCompletedLoadingStep;
    }

    private void OnDisable()
    {
        FirebaseLoader.OnStartedLoading -= OnStartedLoading;
        FirebaseLoader.OnCompletedLoadingStep -= OnCompletedLoadingStep;
    }

    void OnStartedLoading()
    {
        // Reset variables.
        _approximateTotalLoadingTime = _approximateLoadingStepDurations.Sum();
        _accumulatedLoadingTimes = 0;
        _currentLoadingStepIndex = 0;
        _finishedloading = false;

        // Start loading the first step.
        SimulateLoadingStep(0);
    }

    void OnCompletedLoadingStep()
    {
        if (_finishedloading)
        {
            Debug.LogError("A loading step does not have an associated approximate loading time! Remove the loading step in FirebaseLoader or add its approximate duration.");
            return;
        }


        // Finish simulating the loading step.
        // - Cancel tween.
        LeanTween.cancel(gameObject);

        // - Update accumulated loading times.
        var duration = _approximateLoadingStepDurations[_currentLoadingStepIndex];
        _accumulatedLoadingTimes += duration;

        // - Snap progress to target value.
        _ProgressChanged?.Invoke(_accumulatedLoadingTimes / _approximateTotalLoadingTime);


        // Check if all loading steps have been simulated.
        if (_currentLoadingStepIndex == StepCount - 1)
        {
            _ProgressChanged?.Invoke(1);
            _OnLoadingFinished?.Invoke();
            _finishedloading = true;
        }
        else
        {
            // Simulate next loading step.
            _currentLoadingStepIndex++;

            SimulateLoadingStep(_currentLoadingStepIndex);
        }
    }

    void SimulateLoadingStep(int stepindex)
    {
        var duration = _approximateLoadingStepDurations[stepindex];

        var from = _accumulatedLoadingTimes / _approximateTotalLoadingTime;
        var to = (_accumulatedLoadingTimes + duration) / _approximateTotalLoadingTime;

        LeanTween.value(gameObject, from, to, duration)
            .setOnUpdate((float val) =>
            {
                _ProgressChanged?.Invoke(val);
            })
            .setEase(LeanTweenType.linear);
    }
}
