using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AutoPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Model to Place")]
    [SerializeField] private GameObject modelPrefab;

    [Header("Placement Settings")]
    [Tooltip("Minimum distance from camera to place the object")]
    [SerializeField] private float minPlacementDistance = 1.0f;

    private bool isPlaced = false;
    private float pivotYOffset = 0f;

    void Start()
    {
        // Cache AR components
        if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>();
        if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();

        // Calculate Y-offset from model pivot to its bottom (half-height)
        var renderer = modelPrefab.GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            pivotYOffset = renderer.bounds.size.y * 0.5f;
            Debug.Log($"[AutoPlacement] Calculated pivot Y offset: {pivotYOffset}");
        }
        else
        {
            Debug.LogWarning("[AutoPlacement] Model prefab has no MeshRenderer. No pivot offset applied.");
        }

        Debug.Log($"[AutoPlacement] Simplified mode: minPlacementDistance={minPlacementDistance}");
    }

    void Update()
    {
        if (isPlaced)
            return;

        Vector3 camPos = Camera.main.transform.position;

        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment != PlaneAlignment.HorizontalUp)
                continue;

            // Get world-space center of the plane
            Vector3 worldCenter = plane.transform.TransformPoint(plane.center);
            float dist = Vector3.Distance(camPos, worldCenter);
            Debug.Log($"[AutoPlacement] Found plane {plane.trackableId} at world center {worldCenter} (dist {dist})");

            if (dist < minPlacementDistance)
            {
                Debug.Log($"[AutoPlacement] Skipping plane {plane.trackableId}: too close (<{minPlacementDistance})");
                continue;
            }

            // Place at the first valid plane, offset by pivot Y
            Vector3 placePos = worldCenter;
            placePos.y += pivotYOffset;
            Debug.Log($"[AutoPlacement] Placing model on plane {plane.trackableId} at {placePos}");

            Instantiate(modelPrefab, placePos, Quaternion.identity);
            Debug.Log("[AutoPlacement] Model instantiated and placement complete.");

            isPlaced = true;
            break;
        }
    }
}
