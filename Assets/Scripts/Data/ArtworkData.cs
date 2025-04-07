using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ArtworkData : FirebaseData
{
    #region Firestore Properties
    // Main Data
    [FirestoreProperty] public string title { get; set; }
    [FirestoreProperty] public string description { get; set; }
    [FirestoreProperty] public bool published { get; set; }
    
    [FirestoreProperty] public List<DocumentReference> artist_references { get; set; } = new List<DocumentReference>();

    public List<ArtistData> artists { get; set; } = new List<ArtistData>();

    [FirestoreProperty] public int year { get; set; }
    [FirestoreProperty] public string location { get; set; }
    
    [FirestoreProperty] public List<string> artwork_image_references { get; set; } = new List<string>();
    
    // AR Settings
    [FirestoreProperty] public double latitude { get; set; }
    [FirestoreProperty] public double longitude { get; set; }
    [FirestoreProperty] public float max_distance { get; set; }
    
    // Read Only Data
    [FirestoreProperty] public Timestamp creation_time { get; set; }
    [FirestoreProperty] public Timestamp update_time { get; set; }
    
    // Content
    [FirestoreProperty] public List<MediaContentData> content_list { get; set; } = new List<MediaContentData>();
    [FirestoreProperty] public string preset { get; set; } = "None";
    [FirestoreProperty] public string alt_scene { get; set; }
    
    #endregion
    
    #region Loaded data
    
    public DateTime creation_date_time, update_date_time;
    
    public HotspotManager hotspot { get; set; } = null;
    public OnlineMapsMarker3D marker { get; set; } = new OnlineMapsMarker3D();

    private DataList loadedImages = new DataList();
    
    #endregion

    #region Methods

    public async Task<List<Sprite>> GetAllImages()
    {
        // all have been loaded already
        if (loadedImages.Count() >= artwork_image_references.Count)
        {
            return loadedImages.Get();
        }
        
        // load all
        var allImages = new List<Sprite>();
        bool save = false;
        foreach (var imageRef in artwork_image_references)
        {
            var results = await loadedImages.Get(this, imageRef);
            allImages.Add(results.sprite);
            if (results.requiresSave) save = true;
        }

        if (save) AppCache.SaveArtworksCache();
        
        return allImages;
    }

    public async Task<List<Sprite>> GetImages(int count)
    {
        var allImages = new List<Sprite>();
        bool save = false;
        for (int i = 0; i < Mathf.Clamp(artwork_image_references.Count, 0, count); i++)
        {
            var results = await loadedImages.Get(this, artwork_image_references[i]);
            allImages.Add(results.sprite);
            if (results.requiresSave) save = true;
        }
        
        if (save) AppCache.SaveArtworksCache();

        return allImages;
    }

    #endregion
}

[FirestoreData]
public class MediaContentData
{
    // Firebase download link for data
    [FirestoreProperty] public string media_content { get; set; }
    
    // Map for transforms
    [FirestoreProperty] public TransformsData transforms { get; set; } = new TransformsData();
}

[FirestoreData]
public class TransformsData
{
    // Nested map for position offsets
    [FirestoreProperty] public PositionOffset position_offset { get; set; } = new PositionOffset();
    
    // Rotation (direct value)
    [FirestoreProperty] public Rotation rotation { get; set; } = new Rotation();
    
    // Nested map for scale values
    [FirestoreProperty] public Scale scale { get; set; } = new Scale();
}

[FirestoreData]
public class PositionOffset
{
    [FirestoreProperty] public float x_offset { get; set; } = 0;
    [FirestoreProperty] public float y_offset { get; set; } = 0;
    [FirestoreProperty] public float z_offset { get; set; } = 0;
}

[FirestoreData]
public class Scale
{
    [FirestoreProperty] public float x_scale { get; set; } = 1;
    [FirestoreProperty] public float y_scale { get; set; } = 1;
    [FirestoreProperty] public float z_scale { get; set; } = 1;
}

[FirestoreData]
public class Rotation
{
    [FirestoreProperty] public float x_rotation { get; set; } = 0;
    [FirestoreProperty] public float y_rotation { get; set; } = 0;
    [FirestoreProperty] public float z_rotation { get; set; } = 0;
}