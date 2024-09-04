using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    public static PlayerCollider Instance;

    public OnlineMapsMarker3DInstance MapsMarker
    {
        get => GetComponent<OnlineMapsMarker3DInstance>();
    }
    
    private void Awake()
    {
        Instance = this;
    }
}
