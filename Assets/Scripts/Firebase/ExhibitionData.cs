using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ExhibitionData
{
    [FirestoreProperty] public string Title { get; set; }
    [FirestoreProperty] public string Description { get; set; }
    [FirestoreProperty] public ArtistData Artist { get; set; }
    [FirestoreProperty] public string Year { get; set; }
    [FirestoreProperty] public string Location { get; set; }
    [FirestoreProperty] public List<ArtworkData> Artworks { get; set; }
    [FirestoreProperty] public List<Sprite> ExhibitionImages { get; set; }
    
    // Read Only Data
    [FirestoreProperty] public long CreationDateTime { get; set; }
    [FirestoreProperty] public long UpdateDateTime { get; set; }
}
