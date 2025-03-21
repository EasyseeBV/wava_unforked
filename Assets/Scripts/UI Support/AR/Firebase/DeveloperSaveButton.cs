using System;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperSaveButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private const float DEFAULT_VISIBILITY = 0.35f;

    public void SubscribeSaveClick(Action callback)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            callback?.Invoke();
            SetIsSavable(false);
        });
    }
    
    public void SetIsSavable(bool state)
    {
        canvasGroup.interactable = state;
        canvasGroup.alpha = state ? 1 : DEFAULT_VISIBILITY;
    }
}
