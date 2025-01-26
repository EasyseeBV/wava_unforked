using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class FirebaseData
{
    [FirestoreProperty] public string ArtworkTitle { get; set; }
}