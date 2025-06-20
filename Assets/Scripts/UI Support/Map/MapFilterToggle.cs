using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapFilterToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private GameObject filterArea;
    
    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Dependencies")]
    [SerializeField] private SelectionMenu selectionMenu;

    private bool boot = false;
    private static readonly int displayKey = Animator.StringToHash("Display");

    private void Awake()
    {
        button.onClick.AddListener(ToggleFilter);
        filterArea.SetActive(false);
        boot = false;
    }

    public void Close()
    {
        if (!filterArea.activeInHierarchy) return;
        animator.SetBool(displayKey, false);
    }

    public void ToggleFilter()
    {
        bool state = !filterArea.activeInHierarchy;

        if (state)
        {
            filterArea.SetActive(true);
        }
        
        animator.SetBool(displayKey, state);
    }

    public void OnAnimationComplete()
    {
        filterArea.SetActive(false);
    }

    private void OnValidate()
    {
        if (button) button = GetComponent<Button>();
    }
}
