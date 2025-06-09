using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamically adjusts the cell size of a grid layout to take up all space while keeping a constant column count.
/// </summary>
[RequireComponent(typeof(GridLayoutGroup))]
public class AdaptiveGridLayout : MonoBehaviour
{
    [SerializeField]
    int _columnCount = 3;

    [SerializeField]
    GridLayoutGroup _gridLayout;

    [SerializeField]
    RectTransform _rectTransform;

    void OnRectTransformDimensionsChange()
    {
        UpdateCellSize();
    }

    void UpdateCellSize()
    {
        float totalWidth = _rectTransform.rect.width;

        // Calculate the spacing and width to subtract from the available width.
        float spacing = _gridLayout.spacing.x * (_columnCount - 1);
        float padding = _gridLayout.padding.left + _gridLayout.padding.right;

        float availableWidth = totalWidth - spacing - padding;
        float cellWidth = availableWidth / _columnCount;

        _gridLayout.cellSize = new Vector2(cellWidth, cellWidth);
    }
}