using System;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperResetButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private const float DEFAULT_VISIBILITY = 0.35f;
    
    public event Action OnClick;
    
    public void SetIsSavable(bool state)
    {
        canvasGroup.interactable = state;
        canvasGroup.alpha = state ? 1 : DEFAULT_VISIBILITY;
    }

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            SetIsSavable(false);
            OnClick?.Invoke();
        });
    }
}