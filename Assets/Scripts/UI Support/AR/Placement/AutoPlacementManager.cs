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
    [Tooltip("Prefab of the AR model to instantiate")]
    [SerializeField] private GameObject modelPrefab;

    [Header("Placement Settings")]
    [Tooltip("Minimum distance from camera to place the object")]
    [SerializeField] private float minPlacementDistance = 1.0f;
    [Tooltip("Maximum distance from camera to place the object")]
    [SerializeField] private float maxPlacementDistance = 3.0f;

    [Header("Debugging")]
    [SerializeField] private GameObject[] disableOnPlacement;

    private bool isPlaced = false;
    private Vector3 modelBoundsSize;

    private void Start()
    {
        // Cache AR components if not set in inspector
        if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>();
        if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();

        // Calculate the size of the model from its MeshRenderer
        var renderer = modelPrefab.GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            modelBoundsSize = renderer.bounds.size;
        }
        else
        {
            Debug.LogWarning("Model prefab has no MeshRenderer. Using default size of (1,1,1).");
            modelBoundsSize = Vector3.one;
        }
    }

    private void Update()
    {
        if (isPlaced)
            return;

        // Collect all horizontal-up planes big enough to fit the model
        List<ARPlane> candidates = new List<ARPlane>();
        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp &&
                plane.size.x >= modelBoundsSize.x &&
                plane.size.y >= modelBoundsSize.z)
            {
                candidates.Add(plane);
            }
        }

        if (candidates.Count == 0)
            return;

        // Find the best plane based on camera distance
        var camPos = Camera.main.transform.position;
        ARPlane bestPlane = null;
        float bestDist = float.MaxValue;

        foreach (var plane in candidates)
        {
            float dist = Vector3.Distance(camPos, plane.center);
            if (dist >= minPlacementDistance && dist <= maxPlacementDistance && dist < bestDist)
            {
                bestDist = dist;
                bestPlane = plane;
            }
        }

        if (bestPlane == null)
            return;

        // Compute placement position: center of plane, elevated by half the model height
        Vector3 placePos = bestPlane.center;
        placePos.y = bestPlane.transform.position.y + modelBoundsSize.y / 2f;

        // Instantiate the model
        Instantiate(modelPrefab, placePos, Quaternion.identity);
        isPlaced = true;

        foreach (var go in disableOnPlacement)
        {
            go.SetActive(false);
        }
    }
}
