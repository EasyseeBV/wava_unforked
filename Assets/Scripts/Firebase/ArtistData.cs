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
    [FirestoreProperty] public Sprite icon { get; set; }
    
    // Read Only Data
    [FirestoreProperty] public DateTime creation_time { get; set; } //  does not work
    [FirestoreProperty] public DateTime update_time { get; set; } //  does not work
}
