using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMarker : MonoBehaviour
{
    public static PlayerMarker Instance;

    public OnlineMapsMarker3DInstance MapsMarker => GetComponent<OnlineMapsMarker3DInstance>();

    public double Longitude => MapsMarker.Longitude;
    public double Latitude => MapsMarker.Latitude;
    
    private void Awake()
    {
        Instance = this;
    }
}
