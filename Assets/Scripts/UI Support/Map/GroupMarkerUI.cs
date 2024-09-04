using System;
using UnityEngine;
using TMPro;

public class GroupMarkerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text markerSizeLabel;

    private Transform monitoredTransform;
    
    public void SetMarkerSize(int size) => markerSizeLabel.text = size.ToString();

    private void Awake()
    {
        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
    }
    
    public void MonitorTransform(Transform t)
    {
        monitoredTransform = t;
    }

    public void Scale()
    {
        transform.localScale = monitoredTransform.localScale;
    }

    public void Move(Vector3 pos)
    {
        transform.localPosition = new Vector3(pos.x, 0.5f, pos.z);
    }

    public void Translate(Vector3 t)
    {
        transform.localPosition += t;
    }
}
