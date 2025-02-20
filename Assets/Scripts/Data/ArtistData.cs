using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ArtistData
{
    [FirestoreProperty] public string title { get; set; }
    [FirestoreProperty] public string description { get; set; }
    [FirestoreProperty] public string location { get; set; }
    [FirestoreProperty] public string link { get; set; }
    [FirestoreProperty] public string icon { get; set; }
    public Sprite iconImage { get; set; } 
    
    // Read Only Data
    [FirestoreProperty] public Timestamp creation_time { get; set; }
    [FirestoreProperty] public Timestamp update_time { get; set; }
    public DateTime creation_date_time, update_date_time;
    
    // World data
    public string artist_id;
}
