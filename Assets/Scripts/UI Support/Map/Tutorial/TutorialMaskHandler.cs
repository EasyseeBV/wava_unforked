using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMaskHandler : MonoBehaviour
{
    [SerializeField] private Canvas maskCanvas;
    [SerializeField] private GameObject maskContent;
    [SerializeField] private GameObject tutorialMaskContent;
    [SerializeField] private RectTransform circleMask;
    
    [Header("Tutorial References")]
    [SerializeField] private RectTransform tutorialTriangleTop;
    [SerializeField] private RectTransform tutorialTriangleBottom;

    [Header("Content References")]
    [SerializeField] private RectTransform contentTop;
    [SerializeField] private RectTransform contentBottom;
    
    [Header("Alignment points")]
    [SerializeField] private GameObject alignmentPointTop;
    [SerializeField] private GameObject alignmentPointBottom;

    [Header("Alignment offsets")]
    [SerializeField] private float leftOffset = 55;
    [SerializeField] private float rightOffset = -55;
    
    [Header("Dependencies")]
    [SerializeField] private OnlineMaps maps;

    private void Awake()
    {
        maskContent.gameObject.SetActive(false);
        tutorialMaskContent.gameObject.SetActive(false);
        tutorialTriangleTop.gameObject.SetActive(false);
        tutorialTriangleBottom.gameObject.SetActive(false);
    }

    public void PlaceMask()
    {
        maskContent.gameObject.SetActive(true);
        tutorialMaskContent.gameObject.SetActive(true);
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
                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(hotspot.ARObject.transform.position);
                    screenPoint.z = 0f;
                    circleMask.position = screenPoint;

// 2) decide whether to use the top or bottom UI
                    RectTransform chosenTriangle;
                    GameObject    chosenAlignmentPoint;
                    RectTransform chosenContent;

// get the actual pixel-height of your content panels
                    float topHeight    = contentTop.rect.height * contentTop.lossyScale.y;
                    float bottomHeight = contentBottom.rect.height * contentBottom.lossyScale.y;

// if placing the *top* panel above the point would go off the top edge…
                    if (screenPoint.y + topHeight > Screen.height)
                    {
                        chosenTriangle        = tutorialTriangleBottom;
                        chosenAlignmentPoint  = alignmentPointBottom;
                        chosenContent         = contentBottom;
                    }
                    else
                    {
                        chosenTriangle        = tutorialTriangleTop;
                        chosenAlignmentPoint  = alignmentPointTop;
                        chosenContent         = contentTop;
                    }

// 3) decide if you need a left/right nudge
                    float halfWidth = (chosenContent.rect.width * chosenContent.lossyScale.x) / 2f;
                    float offsetX   = 0f;
                    if (screenPoint.x + halfWidth > Screen.width)
                        offsetX = leftOffset;    // nudge it leftward
                    else if (screenPoint.x - halfWidth < 0)
                        offsetX = rightOffset;   // nudge it rightward

// 4) enable only the one triangle, position it at the chosen alignment + offset
                    tutorialTriangleTop.gameObject.SetActive(chosenTriangle == tutorialTriangleTop);
                    tutorialTriangleBottom.gameObject.SetActive(chosenTriangle == tutorialTriangleBottom);

                    Vector3 targetPos = chosenAlignmentPoint.transform.position;
                    targetPos.x += offsetX;
                    chosenTriangle.position = targetPos;
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
