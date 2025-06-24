using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtworkAvailabilityObserver : MonoBehaviour
{
    [SerializeField] private float thresholdMeters = 100f;
    [SerializeField] private float minutes = 3;
    [SerializeField] private SelectionMenu selectionMenu;
    
    private Vector2 _initialPos;
    private Coroutine _timerCoroutine;
    
    private void Start()
    {
        if (OnlineMapsLocationService.instance == null) Debug.Log("OnlineMapsLocationService was null - delayed assigning");
        StartCoroutine(WaitForMaps());
        OnlineMapsLocationService.instance.OnLocationChanged += OnChangedLocation;
    }
    
    private IEnumerator WaitForMaps()
    {
        yield return new WaitForSeconds(5f);
        
        if (OnlineMapsLocationService.instance == null) Debug.Log("OnlineMapsLocationService was null - delayed assigning");
        StartCoroutine(WaitForMaps());
        OnlineMapsLocationService.instance.OnLocationChanged += OnChangedLocation;
    }

    private void OnChangedLocation(Vector2 pos)
    {
        var player = PlayerMarker.Instance;
        var longitude = player.Longitude;
        var latitude = player.Latitude;
        
        if (_timerCoroutine == null)
        {
            _initialPos = new Vector2((float)longitude, (float)latitude);
            _timerCoroutine = StartCoroutine(TimerCoroutine());
            return;
        }

        // already have a timer running: check distance from initial
        float distance = HaversineDistance(_initialPos, new Vector2((float)longitude, (float)latitude));
        if (distance >= thresholdMeters)
        {
            StopCoroutine(_timerCoroutine);
            _initialPos = pos;
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(minutes * 60f);
        selectionMenu.Build();
        _timerCoroutine = null;
    }

    /// <summary>
    /// Computes the great‐circle distance between two lat/lon coords using the Haversine formula.
    /// Input Vector2: x = longitude, y = latitude (both in degrees).
    /// Returns distance in meters.
    /// </summary>
    private static float HaversineDistance(Vector2 a, Vector2 b)
    {
        const float R = 6371000f; // Earth radius in meters

        float lat1 = a.y * Mathf.Deg2Rad;
        float lon1 = a.x * Mathf.Deg2Rad;
        float lat2 = b.y * Mathf.Deg2Rad;
        float lon2 = b.x * Mathf.Deg2Rad;

        float dLat = lat2 - lat1;
        float dLon = lon2 - lon1;

        float sinDLat = Mathf.Sin(dLat * 0.5f);
        float sinDLon = Mathf.Sin(dLon * 0.5f);

        float h = sinDLat * sinDLat +
                  Mathf.Cos(lat1) * Mathf.Cos(lat2) *
                  sinDLon * sinDLon;

        float c = 2f * Mathf.Atan2(Mathf.Sqrt(h), Mathf.Sqrt(1f - h));

        return R * c;
    }
}