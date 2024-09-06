using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using UnityEngine;


/// <summary>
/// Will be converting the marker group system to this tomorrow
/// </summary>
public class GroupMarkerHandler : MonoBehaviour
{
    private enum Zoom
    {
        Close,
        Medium,
        Far
    }
    
    [SerializeField] private GroupMarkerUI template;
    
    [Header("Settings")]
    [SerializeField] private Vector2 closeDistanceGroup;
    [SerializeField] private Vector2 mediumDistanceGroup;
    [SerializeField] private Vector2 farDistanceGroup;
    
    [Header("Dependencies")]
    [SerializeField] private OnlineMaps map;
    [SerializeField] private OnlineMapsMarker3DManager mapManager;
    
    private Transform groupMarkerTransform;
    private List<MarkerGroup> markerGroups = new();
    
    private Zoom zoom;
    private float zoomTolerance
    {
        get
        {
            switch (zoom)
            {
                case Zoom.Close:
                    return closeDistanceGroup.y;
                case Zoom.Medium:
                    return mediumDistanceGroup.y;
                case Zoom.Far:
                    return farDistanceGroup.y;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void OnEnable()
    {
        ARMapPointMaker.OnHotspotsSpawned += Group;
        OnlineMaps.instance.OnChangeZoom += OnChangeZoom;
    }

    private void OnDisable()
    {
        ARMapPointMaker.OnHotspotsSpawned -= Group;
        if(OnlineMaps.instance != null) OnlineMaps.instance.OnChangeZoom -= OnChangeZoom;
    }
    
    private void Group()
    {
        foreach (var artwork in ARInfoManager.ExhibitionsSO.SelectMany(exhibition => exhibition.ArtWorks))
        {
            TryAddArtwork(artwork);
        }

        foreach (var group in markerGroups)
        {
            foreach (var point in group.artworkPoints)
            {
                point.marker.updateMarker = true;
                point.marker.markerGroup = group;
            }
        }

        int indexed = 0;
        
        foreach (var group in markerGroups.Where(group => group.artworkPoints.Count > 1))
        {
            indexed++;
            
            var control = OnlineMapsTileSetControl.instance;
            var marker = control.marker3DManager.Create(group.longitude, group.latitude, template.gameObject);
            
            group.groupMarkerObject = marker.instance.GetComponent<GroupMarkerUI>();
            group.groupMarkerObject.marker = marker;
            group.groupMarkerObject.marker.isGroupMarker = true;
            group.groupMarkerObject.marker.markerGroup = group;
            group.groupMarkerObject.marker.sizeType = OnlineMapsMarker3D.SizeType.realWorld;
            group.groupMarkerObject.marker.scale = 1150f * ((Screen.width + Screen.height) / 2f) / Mathf.Pow(2, (map.zoom + map.zoomScale) * 0.85f);
            
            group.groupMarkerObject.SetMarkerSize(group.artworkPoints.Count);
            
            // deprecated?
            group.groupMarkerObject.MonitorTransform(group.artworkPoints[0].marker.instance.transform);

            foreach (var arPoint in group.artworkPoints)
            {
                arPoint.Hotspot.Logo.enabled = false;
                arPoint.Hotspot.Shadow.SetActive(false);
                arPoint.Hotspot.BorderRingMesh.enabled = false;
            }
        }
    }
    
    private void TryAddArtwork(ARPointSO artwork)
    {
        foreach (var group in markerGroups)
        {
            if (HaversineDistance(new Vector2((float)artwork.Latitude, (float)artwork.Longitude), group.mean) <= zoomTolerance)
            {
                group.Add(artwork);
                artwork.Hotspot.markerGroup = group;
                return;
            }
        }

        MarkerGroup markerGroup = new MarkerGroup(artwork);
        markerGroups.Add(markerGroup);
        artwork.Hotspot.markerGroup = markerGroup;
    }
    
    private double HaversineDistance(Vector2 point1, Vector2 point2)
    {
        double R = 6371e3; // Earth radius in meters

        double lat1Rad = Mathf.Deg2Rad * point1.x;
        double lat2Rad = Mathf.Deg2Rad * point2.x;
        double deltaLatRad = Mathf.Deg2Rad * (point2.x - point1.x);
        double deltaLonRad = Mathf.Deg2Rad * (point2.y - point1.y);

        double a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
    
    private void OnChangeZoom() 
    {
        foreach (var group in markerGroups.Where(group => group.artworkPoints.Count > 1))
        {
            group.groupMarkerObject.marker.scale = 1150f * ((Screen.width + Screen.height) / 2f) / Mathf.Pow(2, (map.zoom + map.zoomScale) * 0.85f);
        }
        
        if (markerGroups.Count > 0 && markerGroups[0].artworkPoints.Count > 0)
        {
            int zoomLevel = (int)markerGroups[0].artworkPoints[0].Hotspot.ZoomLevel;
            ZoomGrouping(zoomLevel);
        }
    }
    

    private void ZoomGrouping(int _zoom)
    {
        if (_zoom >= closeDistanceGroup.x)
        {
            if (zoom == Zoom.Close) return;
                    
            zoom = Zoom.Close;
        }
        else if (_zoom >= mediumDistanceGroup.x)
        {
            if (zoom == Zoom.Medium) return;
                    
            zoom = Zoom.Medium;
        }
        else
        {
            if (zoom == Zoom.Far) return;
                    
            zoom = Zoom.Far;
        }
            
        foreach (var group in markerGroups)
        {
            group.ShowArtworkPoints(true);
            group.groupMarkerObject?.gameObject.SetActive(false);
        }
        
        foreach (var markerGroup in markerGroups)
        {
            if (markerGroup.groupMarkerObject?.marker?.instance != null)
            {
                markerGroup.groupMarkerObject.marker.enabled = false;
                Destroy(markerGroup.groupMarkerObject?.marker?.instance);
            }
        }
        
        markerGroups.Clear();
        
        Group();
    }

}
