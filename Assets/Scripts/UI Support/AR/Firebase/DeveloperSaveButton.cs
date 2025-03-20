using UnityEngine;
using UnityEngine.UI;

public class DeveloperSaveButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private const float DEFAULT_VISIBILITY = 0.35f;
    
    private void Awake()
    {
        button.onClick.AddListener(Save);
    }

    public void SetIsSavable(bool state)
    {
        canvasGroup.interactable = state;
        canvasGroup.alpha = state ? 1 : DEFAULT_VISIBILITY;
    }

    public void Save()
    {
        // NOTIFY DeveloperARView
        SetIsSavable(false);
    }
}
