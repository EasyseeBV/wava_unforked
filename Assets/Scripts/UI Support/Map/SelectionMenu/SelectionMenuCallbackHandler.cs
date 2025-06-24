using UnityEngine;

public class SelectionMenuCallbackHandler : MonoBehaviour
{
    [SerializeField] private SelectionMenu selectionMenu;
    [SerializeField] private DraggableSelectionBar selectionBar;

    private bool subscribed = false;

    private void Start()
    {
        if (!subscribed && OnlineMapsControlBase.instance)
        {
            OnlineMapsControlBase.instance.OnMapClick += OnMapClicked;
            subscribed = true;
        }
    }

    private void OnEnable()
    {
        if (!subscribed && OnlineMapsControlBase.instance)
        {
            OnlineMapsControlBase.instance.OnMapClick += OnMapClicked;
            subscribed = true;
        }

        selectionBar.OnExpanding += OnExpanding;
        selectionBar.OnExpanded += OnSelectionMenuExpanded;
    }

    private void OnDisable()
    {
        if (subscribed && OnlineMapsControlBase.instance)
        {
            OnlineMapsControlBase.instance.OnMapClick -= OnMapClicked;
            subscribed = false;
        }
        
        selectionBar.OnExpanding -= OnExpanding;
        selectionBar.OnExpanded -= OnSelectionMenuExpanded;
    }
    
    private void OnMapClicked()
    {
        if (selectionMenu.SelectedHotspot == null) return;
        
        var cachedHotspot = selectionMenu.SelectedHotspot;
        if (cachedHotspot.inPlayerRange) return;
        
        selectionMenu.DeselectHotspot();
    }
    
    private void OnSelectionMenuExpanded(bool state)
    {
        if (state) selectionMenu.ShowExpandedDetails();
        else selectionMenu.ShowMinimalDetails();
    }
    
    private void OnExpanding(bool state)
    {
        if (state) selectionMenu.StartExpanding();
        else selectionMenu.StartCollapsing();
    }
}
