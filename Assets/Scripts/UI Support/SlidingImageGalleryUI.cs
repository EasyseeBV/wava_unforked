using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SlidingImageGalleryUI : MonoBehaviour
{
    [SerializeField]
    HorizontalLayoutGroup _horizontalLayoutGroup;

    [SerializeField]
    float _elementWidth;

    [SerializeField, Range(0, 1)]
    float _smoothTime;

    int _targetChildIndex;

    // For smooth damp.
    float _targetPadding;
    float _velocity;

    private void Update()
    {
        // Adjust padding left.
        var currentPadding = _horizontalLayoutGroup.padding.left;

        var nextPadding = Mathf.SmoothDamp(currentPadding, _targetPadding, ref _velocity, _smoothTime);

        _horizontalLayoutGroup.padding.left = Mathf.RoundToInt(nextPadding);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void GoForwards()
    {
        _targetChildIndex = Mathf.Clamp(_targetChildIndex + 1, 0, transform.childCount - 1);

        _targetPadding = - _targetChildIndex * _elementWidth;

        Debug.Log(_targetChildIndex);
    }

    public void GoBackwards()
    {
        _targetChildIndex = Mathf.Clamp(_targetChildIndex - 1, 0, transform.childCount - 1);

        _targetPadding = - _targetChildIndex * _elementWidth;

        Debug.Log(_targetChildIndex);
    }

#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(SlidingImageGalleryUI))]
    class SlidingImageGalleryUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");

            if (GUILayout.Button("Go forwards"))
            {
                var myTarget = (SlidingImageGalleryUI)target;
                myTarget.GoForwards();
            }

            if (GUILayout.Button("Go backwards"))
            {
                var myTarget = (SlidingImageGalleryUI)target;
                myTarget.GoBackwards();
            }
        }
    }
#endif
}
