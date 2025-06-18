using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnderlinedSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    List<TextMeshProUGUI> _selectionTexts;

    [SerializeField]
    List<Button> _selectionButtons;

    [SerializeField]
    RectTransform _grayLine;

    [SerializeField]
    RectTransform _blackLine;

    [Header("Animation parameters")]
    [SerializeField]
    Color _defaultTextColor;

    [SerializeField]
    Color _selectedTextColor;

    [SerializeField]
    float _animationDuration;

    float _lineWidth;

    public Action<int> _SelectedIndex;

    bool _subscribedToButtons;

    private void Start()
    {
        Setup();
    }

    void Setup()
    {
        // Set black line width.
        var totalWidth = _grayLine.rect.width;
        _lineWidth = totalWidth / SelectionOptionsCount();

        var size = _blackLine.sizeDelta;
        size.x = _lineWidth;
        _blackLine.sizeDelta = size;


        // Initialize black line position.
        var position = _blackLine.anchoredPosition;
        position.x = 0;
        _blackLine.anchoredPosition = position;


        // Subscribe to button presses.
        if (_subscribedToButtons)
            return;

        _subscribedToButtons = true;

        for (int i = 0; i < _selectionButtons.Count; i++)
        {
            var button = _selectionButtons[i];

            // Circumvent loop variable closure problem.
            var index = i;

            button.onClick.AddListener(() => { ShowAsSelected(index); });
        }
    }

    int SelectionOptionsCount() => _selectionTexts.Count;

    void ShowAsSelected(int index)
    {
        index = Mathf.Clamp(index, 0, SelectionOptionsCount() - 1);

        LeanTween.cancel(gameObject);


        // Trigger black bar movement animation.
        var currentX = _blackLine.anchoredPosition.x;
        var targetX = index * _lineWidth;

        if (currentX != targetX)
        {
            LeanTween.value(gameObject, currentX, targetX, _animationDuration)
                .setOnUpdate((float val) =>
                {
                    var position = _blackLine.anchoredPosition;
                    position.x = val;
                    _blackLine.anchoredPosition = position;
                })
                .setEase(LeanTweenType.easeInOutQuad);
        }


        // Trigger text color animation.
        for (int i = 0; i < _selectionTexts.Count; i++)
        {
            var text = _selectionTexts[i];

            var currentColor = text.color;
            var targetColor = i == index ? _selectedTextColor : _defaultTextColor;

            if (currentColor == targetColor)
                continue;

            LeanTween.value(gameObject, 0, 1, _animationDuration)
                .setOnUpdate((float val) =>
                {
                    text.color = Color.Lerp(currentColor, targetColor, val);
                })
                .setEase(LeanTweenType.easeInOutQuad);
        }
    }

#if UNITY_EDITOR
    // Custom editor for easy testing.
    [CustomEditor(typeof(UnderlinedSelectionUI))]
    class UnderlinedSelectionUIEditor : Editor
    {
        int _pointIndexToSelect;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("DEBUG");


            _pointIndexToSelect = EditorGUILayout.IntField("Show index as selected", _pointIndexToSelect);

            if (GUILayout.Button("Select point with index"))
            {
                var myTarget = (UnderlinedSelectionUI)target;
                myTarget.ShowAsSelected(_pointIndexToSelect);
            }
        }
    }
#endif
}
