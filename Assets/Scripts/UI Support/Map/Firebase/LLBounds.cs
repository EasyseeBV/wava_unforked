using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LLBounds : MonoBehaviour
{
    [SerializeField] private OnlineMaps maps;
    [Space] [SerializeField] private bool test;

    private void Awake()
    {
        _ = FirebaseLoader.LoadRemainingArtworks();
    }

    private void Update()
    {
        if (test)
        {
            test = false;
            GetBounds();
        }
    }

    private void GetBounds()
    {
        double minLon, minLat, maxLon, maxLat;
        maps.GetCorners(out minLon, out minLat, out maxLon, out maxLat);
        
        Debug.Log($"MinLat: {minLat}, MaxLat: {maxLat}, MinLon: {minLon}, MaxLon: {maxLon}");
        
        // query firebase for documents within the bounds
        /*
         *
         Query query = locationsRef
            .WhereGreaterThanOrEqualTo("latitude", minLat)
            .WhereLessThanOrEqualTo("latitude", maxLat)
            .WhereGreaterThanOrEqualTo("longitude", minLon)
            .WhereLessThanOrEqualTo("longitude", maxLon);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            Debug.Log($"Document {document.Id} found within bounds.");
        }
         */
    }
}

