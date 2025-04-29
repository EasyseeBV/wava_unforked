using System;
using UnityEngine;
using TMPro;

public class GroupMarkerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text markerSizeLabel;
    
    public OnlineMapsMarker3D marker;

    private Transform monitoredTransform;
    
    public void SetMarkerSize(int size) => markerSizeLabel.text = size.ToString();

    private void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, 0.2f, transform.localPosition.z);
    }

    public void MonitorTransform(Transform t)
    {
        monitoredTransform = t;
    }
}
