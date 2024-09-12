using System;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class MarkerGroup
{
    public GroupMarkerUI groupMarkerObject;
    public List<ARPointSO> artworkPoints;
    public Vector2 mean; // Using Vector2 to store latitude and longitude
    public bool zoomedIn = false;

    public double latitude => mean.x;
    public double longitude => mean.y;

    public bool SelectedGroup
    {
        get => selectedGroup;
        set
        {
            if (selectedGroup != value)
            {
                ShowArtworkPoints(value);
                selectedGroup = value;
            }
        }
    }

    private bool selectedGroup = false;
    private bool showingArtwork = false;

    public MarkerGroup(ARPointSO artwork)
    {
        artworkPoints = new List<ARPointSO>();
        artworkPoints.Add(artwork);
        mean = new Vector2((float)artwork.Latitude, (float)artwork.Longitude);
    }

    public void Add(ARPointSO artwork)
    {
        artworkPoints.Add(artwork);
        RecalculateMean();
    }

    public void RecalculateMean(bool updateTransform = false)
    {
        // Calculate mean latitude and longitude
        double totalLat = 0.0;
        double totalLon = 0.0;
        float totalX = 0f;
        float totalZ = 0f;

        foreach (var artworkPoint in artworkPoints)
        {
            totalLat += artworkPoint.Latitude;
            totalLon += artworkPoint.Longitude;
            totalX += artworkPoint.marker.instance.transform.localPosition.x;
            totalZ += artworkPoint.marker.instance.transform.localPosition.z;
        }

        mean = new Vector2((float)(totalLat / artworkPoints.Count), (float)(totalLon / artworkPoints.Count));
    }

    public void ShowArtworkPoints(bool state, bool onlyForGroups = false)
    {
        if (onlyForGroups && artworkPoints.Count <= 1) return;
        
        if (SelectedGroup) return;
        if (showingArtwork == state) return;

        groupMarkerObject?.gameObject.SetActive(!state);

        foreach (var point in artworkPoints)
        {
            point.Hotspot.Logo.enabled = state;
            point.Hotspot.Shadow.SetActive(state);
            point.Hotspot.Parent.SetActive(state);

            if (!state) continue;
            //point.Hotspot.BorderRingMesh.enabled = point.Hotspot.InPlayerRange || point.Hotspot.selected;
        
        }

        showingArtwork = state;
    }

    public void TryToggleMarker()
    {
        if (artworkPoints?.Count <= 1) return;
        
        switch (zoomedIn)
        {
            case true when showingArtwork == false:
                ShowArtworkPoints(true);
                return;
            case true:
                return;
        }

        int inactive = artworkPoints.Count(artworkPoint => !artworkPoint.marker.instance.gameObject.activeInHierarchy);
        bool showArtwork = (float)inactive / artworkPoints.Count > 0.45f;
        ShowArtworkPoints(showArtwork);
    }

    public void CheckForValidZoom()
    {
        if (artworkPoints?.Count <= 1) return;
        
        if (zoomedIn)
        {
            ShowArtworkPoints(true);
            groupMarkerObject?.gameObject.SetActive(false);
        }
    }
}