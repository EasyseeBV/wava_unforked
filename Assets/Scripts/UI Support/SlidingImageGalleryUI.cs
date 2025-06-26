using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SlidingImageGalleryUI : MonoBehaviour
{
    public float _ElementWidth;

    [SerializeField]
    HorizontalLayoutGroup _horizontalLayoutGroup;

    [SerializeField, Range(0, 1)]
    float _animationDuration;

    RectTransform _layoutGroupTransform;

    int _tweenId;

    int _targetChildIndex;

    private void Awake()
    {
        _layoutGroupTransform = _horizontalLayoutGroup.transform as RectTransform;
    }

    public void GoForwards() => SetTargetImageIndex(_targetChildIndex + 1);

    public void GoBackwards() => SetTargetImageIndex(_targetChildIndex - 1);

    public void SetTargetImageIndex(int targetIndex)
    {
        // Keep index within bounds.
        _targetChildIndex = Mathf.Clamp(targetIndex, 0, transform.childCount - 1);

        LeanTween.cancel(_tweenId);

        var currentPadding = _horizontalLayoutGroup.padding.left;
        var targetPadding = -_targetChildIndex * _ElementWidth;

        _tweenId = LeanTween.value(gameObject, currentPadding, targetPadding, _animationDuration)
            .setOnUpdate((float val) =>
            {
                _horizontalLayoutGroup.padding.left = Mathf.RoundToInt(val);

                LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroupTransform);
            })
            .setEase(LeanTweenType.easeInOutQuad)
            .uniqueId;
    }

    public void FinishAnimationsImmediately()
    {
        LeanTween.cancel(_tweenId);

        var targetPadding = -_targetChildIndex * _ElementWidth;

        _horizontalLayoutGroup.padding.left = Mathf.RoundToInt(targetPadding);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroupTransform);
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
