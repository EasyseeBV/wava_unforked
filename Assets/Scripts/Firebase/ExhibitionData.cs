using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ExhibitionData
{
    [FirestoreProperty] public string title { get; set; }
    [FirestoreProperty] public string description { get; set; }
    
    [FirestoreProperty] public List<DocumentReference> artist_references { get; set; } = new List<DocumentReference>();
    public List<ArtistData> artists { get; set; } = new List<ArtistData>();
    
    [FirestoreProperty] public List<DocumentReference> artwork_references { get; set; } = new List<DocumentReference>();
    public List<ArtworkData> artworks { get; set; } = new List<ArtworkData>();
    
    [FirestoreProperty] public int year { get; set; }
    [FirestoreProperty] public string location { get; set; }
    [FirestoreProperty] public List<Sprite> exhibition_images { get; set; }
    
    // Read Only Data
    [FirestoreProperty] public DateTime creation_time { get; set; }
    [FirestoreProperty] public DateTime update_time { get; set; }
    
    // World Data
    public string exhibition_id;
}
