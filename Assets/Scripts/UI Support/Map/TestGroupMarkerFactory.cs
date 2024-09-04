using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Will be converting the marker group system to this tomorrow
/// </summary>
public class TestGroupMarkerFactory : MonoBehaviour
{
    [SerializeField] private GameObject template;

    [Header("Debug")]
    public double longitude;
    public double latitude;

    private OnlineMapsMarker3D marker;
    
    private void Start()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        yield return new WaitForSeconds(0.5f);
        
        var map = OnlineMaps.instance;
        var control = OnlineMapsTileSetControl.instance;
        marker = control.marker3DManager.Create(longitude, latitude, template);
        marker.sizeType = OnlineMapsMarker3D.SizeType.realWorld;
        marker.scale = 1000f / map.zoom;
        marker.instance.name = "Real-time marker test";
    }
}
