using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMaskHandler : MonoBehaviour
{
    [SerializeField] private Canvas maskCanvas;
    [SerializeField] private GameObject maskContent;
    [SerializeField] private GameObject tutorialContent;
    [SerializeField] private RectTransform circleMask;
    [SerializeField] private RectTransform tutorialInfo;
    [SerializeField] private RectTransform tutorialTriangle;
    
    [Header("Dependencies")]
    [SerializeField] private OnlineMaps maps;

    public void PlaceMask()
    {
        maskContent.gameObject.SetActive(true);
        tutorialContent.gameObject.SetActive(true);
        FindClosestArtwork();
    }
    
    private void FindClosestArtwork()
    {
        Debug.Log("finding closest artwork...");
        var marker = PlayerMarker.Instance.MapsMarker;
        double playerLongitude = marker.Longitude;
        double playerLatitude = marker.Latitude;

        double closestDistance = double.MaxValue;
        ArtworkData closestArtwork = null;
        
        foreach (var artwork in FirebaseLoader.Artworks)
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

        if (closestArtwork != null)
        {
            if (maps.InMapView(closestArtwork.longitude, closestArtwork.latitude))
            {
                var hotspot = closestArtwork.hotspot;

                if (hotspot != null)
                {
                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(hotspot.transform.position);

                    // Optional: early-out if behind the camera
                    if (screenPoint.z < 0)
                    {
                        circleMask.gameObject.SetActive(false);
                        return;
                    }
                    else circleMask.gameObject.SetActive(true);

                    // 2) If your Canvas is Screen Space – Overlay, you can just use:
                    circleMask.position = screenPoint;
                }
            }
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
