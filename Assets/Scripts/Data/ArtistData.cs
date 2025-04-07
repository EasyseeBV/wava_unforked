using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ArtistData : FirebaseData
{
    [FirestoreProperty] public string title { get; set; }
    [FirestoreProperty] public string description { get; set; }
    [FirestoreProperty] public string location { get; set; }
    [FirestoreProperty] public string link { get; set; }
    [FirestoreProperty] public string icon { get; set; }
    [FirestoreProperty] public bool published { get; set; }
    
    // Read Only Data
    [FirestoreProperty] public Timestamp creation_time { get; set; }
    [FirestoreProperty] public Timestamp update_time { get; set; }
    public DateTime creation_date_time, update_date_time;

    private DataList loadedIcon = new DataList();

    public async Task<Sprite> GetIcon()
    {
        if (loadedIcon.Count() <= 0)
        {
            var results = await loadedIcon.Get(this, icon);
            if (results.requiresSave) AppCache.SaveArtistsCache();
            return results.sprite;
        }
        
        var result = await loadedIcon.Get(this, icon);
        return result.sprite;
    }
}
