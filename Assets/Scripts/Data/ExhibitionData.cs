using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ExhibitionData : FirebaseData
{
    [FirestoreProperty] public string title { get; set; }
    [FirestoreProperty] public string description { get; set; }
    
    [FirestoreProperty] public List<DocumentReference> artist_references { get; set; } = new List<DocumentReference>();
    public List<ArtistData> artists { get; set; } = new List<ArtistData>();
    
    [FirestoreProperty] public List<DocumentReference> artwork_references { get; set; } = new List<DocumentReference>();
    public List<ArtworkData> artworks { get; set; } = new List<ArtworkData>();
    
    [FirestoreProperty] public int year { get; set; }
    [FirestoreProperty] public string location { get; set; }
    
    [FirestoreProperty] public bool published { get; set; }
    [FirestoreProperty] public string color { get; set; }
    
    [FirestoreProperty] public List<string> exhibition_image_references { get; set; } = new List<string>();
    
    // Read Only Data
    [FirestoreProperty] public Timestamp creation_time { get; set; }
    [FirestoreProperty] public Timestamp update_time { get; set; }
    public DateTime creation_date_time, update_date_time;
    
    private DataList loadedImages = new DataList();
    
    public async Task<List<Sprite>> GetAllImages()
    {
        // all have been loaded already
        if (loadedImages.Count() >= exhibition_image_references.Count)
        {
            return loadedImages.Get();
        }
        
        // load all
        var allImages = new List<Sprite>();
        foreach (var imageRef in exhibition_image_references)
        {
            var spr = await loadedImages.Get(this, imageRef);
            allImages.Add(spr);
        }

        AppCache.SaveExhibitionsCache();
        
        return allImages;
    }

    public async Task<List<Sprite>> GetImages(int count)
    {
        var allImages = new List<Sprite>();
        for (int i = 0; i < Mathf.Clamp(exhibition_image_references.Count, 0, count); i++)
        {
            var spr = await loadedImages.Get(this, exhibition_image_references[i]);
            allImages.Add(spr);
        }
        
        AppCache.SaveExhibitionsCache();

        return allImages;
    }
}
