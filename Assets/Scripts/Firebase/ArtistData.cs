using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ArtistData
{
    [FirestoreProperty] public string Title { get; set; }
    [FirestoreProperty] public string Description { get; set; }
    [FirestoreProperty] public string Location { get; set; }
    [FirestoreProperty] public string Link { get; set; }
    [FirestoreProperty] public Sprite Thumbnail { get; set; }
    
    // Read Only Data
    [FirestoreProperty] public long CreationDateTime { get; set; }
    [FirestoreProperty] public long UpdateDateTime { get; set; }
}
