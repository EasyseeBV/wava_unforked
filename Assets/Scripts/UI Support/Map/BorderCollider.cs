using UnityEngine;

public class BorderCollider : MonoBehaviour
{
    [SerializeField]
    float scaleFactor = 0.00005f;

    private void OnEnable()
    {
        UpdateScale();

        OnlineMaps.instance.OnChangeZoom += UpdateScale;
    }
    
    private void OnDisable()
    {
        OnlineMaps.instance.OnChangeZoom -= UpdateScale;
    }

    private void UpdateScale()
    {
        Debug.Log("Updated scale!");

        // Update distance indicator size.
        var distanceIndicator = transform;
        var parentLossyScale = distanceIndicator.parent != null ? distanceIndicator.parent.lossyScale : Vector3.one;

        //OnlineMaps.instance.zoom + 

        var indicatorScale = Mathf.Pow(2, OnlineMaps.instance.floatZoom) * scaleFactor;

        distanceIndicator.localScale = new Vector3(
            indicatorScale / parentLossyScale.x,
            indicatorScale / parentLossyScale.y,
            indicatorScale / parentLossyScale.z);
    }
}
