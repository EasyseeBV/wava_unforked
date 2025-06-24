using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapIdleDetector : MonoBehaviour
{
    [Tooltip("Seconds of inactivity before OnIdle is invoked")]
    public float idleThreshold = 10f;

    private float _lastInteractionTime;
    private bool _hasFiredIdle;

    private void Start()
    {
        _lastInteractionTime = Time.time;
        SubscribeMapEvents();
    }

    private void Update()
    {
        if (Input.touchCount > 0 ||
            Input.GetMouseButtonDown(0) ||
            Input.GetMouseButtonDown(1) ||
            Input.mouseScrollDelta.y != 0f ||
            Input.anyKeyDown)
        {
            RegisterInteraction();
        }

        if (!_hasFiredIdle && Time.time - _lastInteractionTime >= idleThreshold)
        {
            _hasFiredIdle = true;
            Debug.Log("<color=red>Idle delayed</color>");
            //OnIdle?.Invoke();
        }
    }

    private void RegisterInteraction()
    {
        _lastInteractionTime = Time.time;
        _hasFiredIdle = false;
    }

    private void SubscribeMapEvents()
    {
        // Grab the active control (could be OnlineMapsControlBase3D or 2D, depending on your setup)
        var control = OnlineMapsControlBase.instance;
        if (control == null)
        {
            Debug.LogWarning("MapIdleDetector: No OnlineMapsControlBase instance found in scene.");
            return;
        }

        control.OnMapClick += RegisterInteraction;
        control.OnMapLongPress += RegisterInteraction;
        control.OnMapDrag += RegisterInteraction;
        control.OnMapRelease += RegisterInteraction;
        control.OnMapDoubleClick += RegisterInteraction;
    }

    private void OnDestroy()
    {
        // Clean up subscriptions
        var control = OnlineMapsControlBase.instance;
        if (control == null) return;
        
        control.OnMapClick -= RegisterInteraction;
        control.OnMapLongPress -= RegisterInteraction;
        control.OnMapDrag -= RegisterInteraction;
        control.OnMapRelease -= RegisterInteraction;
        control.OnMapDoubleClick -= RegisterInteraction;
    }
}
