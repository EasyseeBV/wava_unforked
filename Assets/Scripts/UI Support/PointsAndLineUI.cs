using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PointsAndLineUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Image _pointsImagePrefab;

    [SerializeField]
    Transform _pointsContainer;

    [Header("Settings")]
    [SerializeField]
    float _animationDuration;

    [SerializeField]
    Color _selectedColor;

    Color _defaultPointColor;

    [SerializeField]
    float _selectedWidth;

    float _defaultWidth;

    List<Image> _points;

    List<float> _pointsAnimationProgress;

    int _selectedPointIndex;

    private void Awake()
    {
        // Store values for the unselected points.
        _defaultPointColor = _pointsImagePrefab.color;
        _defaultWidth = _pointsImagePrefab.rectTransform.sizeDelta.x;
    }

    public void SetSelectedPointIndex(int pointIndex)
    {
        _selectedPointIndex = pointIndex;
    }

    public void SetPointCount(int count)
    {
        if (_points == null)
        {
            _points = new();
            _pointsAnimationProgress = new();
        }


        // Instantiate points up to the point count.
        while (_points.Count < count)
        {
            var point = Instantiate(_pointsImagePrefab, _pointsContainer);
            _points.Add(point);
            _pointsAnimationProgress.Add(0);
        }

        // Remove the points that are too many.
        while (_points.Count > count)
        {
            var pointToRemove = _points[_points.Count - 1];

            Destroy(pointToRemove.gameObject);

            _points.RemoveAt(_points.Count - 1);
            _pointsAnimationProgress.RemoveAt(_points.Count - 1);
        }
    }

    public int GetPointCount() => _points != null ? _points.Count : 0;

    public void FinishAnimationsImmediately()
    {
        for (int i = 0; i < _points.Count; i++)
        {
            _pointsAnimationProgress[i] = i == _selectedPointIndex ? 1 : 0;
            UpdateAnimationForPoint(i);
        }
    }

    void Update()
    {
        if (_points == null)
            return;

        // Increase/decrease the animation progress of each point.
        for (int i = 0; i < _points.Count; i++)
        {
            var delta = Time.deltaTime / _animationDuration;

            if (i == _selectedPointIndex)
            {
                // Increase the animation progress.
                _pointsAnimationProgress[i] = Mathf.Clamp01(_pointsAnimationProgress[i] + delta);
            }
            else
            {
                // Decrease the animation progress.
                _pointsAnimationProgress[i] = Mathf.Clamp01(_pointsAnimationProgress[i] - delta);
            }


            // Animate the point.
            UpdateAnimationForPoint(i);
        }
    }

    void UpdateAnimationForPoint(int pointIndex)
    {
        var point = _points[pointIndex];
        var progress = LeanTween.easeInOutSine(0, 1, _pointsAnimationProgress[pointIndex]);
        point.color = Color.Lerp(_defaultPointColor, _selectedColor, progress);
        var width = Mathf.Lerp(_defaultWidth, _selectedWidth, progress);
        point.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(PointsAndLineUI))]
    class PointsAndLineUIEditor : Editor
    {
        int _pointIndexToSelect;

        int _pointCount;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");


            _pointIndexToSelect = EditorGUILayout.IntField("Point index to select", _pointIndexToSelect);

            if (GUILayout.Button("Select point with index"))
            {
                PointsAndLineUI myTarget = (PointsAndLineUI)target;
                myTarget.SetSelectedPointIndex(_pointIndexToSelect);
            }


            _pointCount = EditorGUILayout.IntField("Point count", _pointCount);

            if (GUILayout.Button("Set point count"))
            {
                PointsAndLineUI myTarget = (PointsAndLineUI)target;
                myTarget.SetPointCount(_pointCount);
            }
        }
    }
#endif
}
