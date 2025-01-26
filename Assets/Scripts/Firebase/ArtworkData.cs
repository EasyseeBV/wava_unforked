using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ArtworkData
{
    // Main Data
    [FirestoreProperty] public string Title { get; set; }
    [FirestoreProperty] public string Description { get; set; }
    [FirestoreProperty] public List<ArtistData> Artists { get; set; } = new List<ArtistData>();
    [FirestoreProperty] public string Year { get; set; }
    [FirestoreProperty] public string Location { get; set; }
    [FirestoreProperty] public string Thumbnail { get; set; } // used for reference to thumbnail image url?
    [FirestoreProperty] public List<Sprite> Images { get; set; } = new List<Sprite>();
    
    // AR Settings
    [FirestoreProperty] public double Latitude { get; set; }
    [FirestoreProperty] public double Longitude { get; set; }
    [FirestoreProperty] public double MaxDistance { get; set; }
    
    // Read Only Data
    [FirestoreProperty] public long CreationDateTime { get; set; }
    [FirestoreProperty] public long UpdateDateTime { get; set; }
}