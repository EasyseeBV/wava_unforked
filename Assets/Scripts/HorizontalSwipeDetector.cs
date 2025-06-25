using System;
using UnityEngine;

public class HorizontalSwipeDetector : MonoBehaviour
{
    [SerializeField] float _minimumSwipeDistance;

    /// <summary>
    /// The parameter specifies the start position of the touch in screen coordinates.
    /// </summary>
    public Action<Vector2> SwipedLeft;

    /// <summary>
    /// The parameter specifies the start position of the touch in screen coordinates.
    /// </summary>
    public Action<Vector2> SwipedRight;

    Vector2 _touchStartPosition;

    bool _swipeCompleted;

    private void OnEnable()
    {
        _swipeCompleted = false;
        _touchStartPosition = Vector2.zero;
    }

    void Update()
    {
        if (Input.touchCount == 0)
        {
            return;
        }

        var touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            _touchStartPosition = touch.position;
            _swipeCompleted = false;
        }
        else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended) && !_swipeCompleted)
        {
            var delta = touch.position - _touchStartPosition;

            if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y) || Mathf.Abs(delta.x) < _minimumSwipeDistance)
                return;

            if (delta.x < 0)
                SwipedLeft?.Invoke(_touchStartPosition);
            else
                SwipedRight?.Invoke(_touchStartPosition);

            _swipeCompleted = true;
        }
    }
}