using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;

public class NoArtworkHandler : MonoBehaviour
{
    [SerializeField] private GameObject content;

    [Header("References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button findArtworkButton;

    [Header("Dependencies")]
    [SerializeField] private OnlineMaps maps;
    
    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        findArtworkButton.onClick.AddListener(FindClosestArtwork);
    }

    public void Open()
    {
        content.gameObject.SetActive(true);
    }

    public void Close()
    {
        content.gameObject.SetActive(false);
    }

    private void FindClosestArtwork()
    {
        var marker = PlayerMarker.Instance.MapsMarker;
        double playerLongitude = marker.Longitude;
        double playerLatitude = marker.Latitude;

        double closestDistance = double.MaxValue;
        ArtworkData closestArtwork = null;

        foreach (var exhibition in FirebaseLoader.Exhibitions)
        {
            foreach (var artwork in exhibition.artworks)
            {
                double artworkLongitude = artwork.longitude;
                double artworkLatitude = artwork.latitude;

                // Calculate the distance using the Haversine formula
                double distance = CalculateDistance(playerLatitude, playerLongitude, artworkLatitude, artworkLongitude);

                // Check if this artwork is closer than the previously found one
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestArtwork = artwork;
                }
            }
        }

        if (closestArtwork != null)
        {
            maps.SetPosition(closestArtwork.longitude, closestArtwork.latitude);
        }
    }

    // Haversine formula to calculate the distance between two latitude/longitude points
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Radius of the Earth in kilometers
        double latDiff = DegreesToRadians(lat2 - lat1);
        double lonDiff = DegreesToRadians(lon2 - lon1);

        double a = Math.Sin(latDiff / 2) * Math.Sin(latDiff / 2) +
                   Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                   Math.Sin(lonDiff / 2) * Math.Sin(lonDiff / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c; // Distance in kilometers
    }

    private double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}
