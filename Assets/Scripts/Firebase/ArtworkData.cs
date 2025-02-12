using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ArtworkData
{
    // Main Data
    [FirestoreProperty] public string title { get; set; }
    [FirestoreProperty] public string description { get; set; }
    
    [FirestoreProperty] public List<DocumentReference> artist_references { get; set; } = new List<DocumentReference>();
    public List<ArtistData> artists { get; set; } = new List<ArtistData>();
    
    [FirestoreProperty] public int year { get; set; }
    [FirestoreProperty] public string location { get; set; }
    
    [FirestoreProperty] public List<string> artwork_image_references { get; set; } = new List<string>();
    public List<Sprite> images { get; set; } = new List<Sprite>();
    
    // AR Settings
    [FirestoreProperty] public double latitude { get; set; }
    [FirestoreProperty] public double longitude { get; set; }
    [FirestoreProperty] public float max_distance { get; set; }

    //[FirestoreProperty]
    public bool place_right { get; set; }
    
    // Read Only Data
    [FirestoreProperty] public DateTime creation_time { get; set; }
    [FirestoreProperty] public DateTime update_time { get; set; }
    
    // Content
    [FirestoreProperty] public string media_content { get; set; } // media references
    [FirestoreProperty] public string content_url { get; set; } // direct references
    [FirestoreProperty] public string preset { get; set; } // preset enum name
    
    // World Data
    public string artwork_id { get; set; }
    public HotspotManager hotspot { get; set; } = null;
    public OnlineMapsMarker3D marker { get; set; } = new OnlineMapsMarker3D();
}