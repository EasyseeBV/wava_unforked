using System;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using UnityEngine;

public class GroupMarkers : MonoBehaviour
{
    private enum Zoom
    {
        Close,
        Medium,
        Far
    }
    
    [Header("Settings")]
    [SerializeField] private Vector2 closeDistanceGroup;
    [SerializeField] private Vector2 mediumDistanceGroup;
    [SerializeField] private Vector2 farDistanceGroup;
    
    [Header("Prefabs")]
    [SerializeField] private GroupMarkerUI groupMarkerPrefab;

    [Header("Dependencies")]
    [SerializeField] private OnlineMaps map;

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
    
    private Transform groupMarkerTransform;
    [SerializeField] private List<MarkerGroup> markerGroups = new();
    
    private void Start()
    {
        map.OnChangeZoom += OnChangeZoom;
        map.OnChangePosition += OnChangePosition;
    }

    public void Group()
    {
        if (!groupMarkerTransform) groupMarkerTransform = FindObjectOfType<PlayerCollider>().transform.parent;
        if (!createGroupsOnInit) return;

        groupsVisible = true;
            
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

        foreach (var group in markerGroups.Where(group => group.artworkPoints.Count > 1))
        {
            group.groupMarkerObject = Instantiate(groupMarkerPrefab, group.gameSpaceMean, Quaternion.identity, groupMarkerTransform);
            group.groupMarkerObject.SetMarkerSize(group.artworkPoints.Count);
            group.groupMarkerObject.MonitorTransform(group.artworkPoints[0].marker.instance.transform);

            foreach (var arPoint in group.artworkPoints)
            {
                arPoint.Hotspot.Logo.enabled = false;
                arPoint.Hotspot.Shadow.SetActive(false);
                if(!arPoint.Hotspot.InPlayerRange) arPoint.Hotspot.BorderRingMesh.enabled = false;
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
    
    public void OnChangeZoom()
    {
        foreach (var group in markerGroups)
        {
            if (!group.groupMarkerObject) continue;
            
            if (!group.zoomedIn)
            {
                group.ShowArtworkPoints(false);  
                group.groupMarkerObject?.Scale();
                group.RecalculateMean(true);
            }
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
            
        markerGroups.Clear();
        Group();
    }

    public void OnChangePosition()
    {
        foreach (var group in markerGroups)
        {
            if (!group.groupMarkerObject) continue;

            group.RecalculateMean(true);
        }
    }
    
    // Testing purposes
    [Header("Debugging")] 
    [SerializeField] private bool createGroupsOnInit;
    [SerializeField] private bool createGroups;
    [SerializeField] private bool hideGroups;
    private bool groupsVisible = true;
    
    private void Update()
    {
        if (createGroups)
        {
            createGroups = false;
            
            markerGroups.Clear();
            Group();
        }

        if (groupsVisible == hideGroups)
        {
            groupsVisible = !hideGroups;

            foreach (var group in markerGroups)
            {
                group.ShowArtworkPoints(hideGroups);
            }
        }
    }
}
