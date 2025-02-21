using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LoadNewestArtwork : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GalleryCard galleryCard;
    [SerializeField] private int showCount = 4;
    [SerializeField] private Transform parent;

    [Header("Debugging")]
    [SerializeField] private List<Sprite> debugSprites;
    
    private bool loaded = false;
    
    private void OnEnable() => FirebaseLoader.OnFirestoreInitialized += async () => await QueryMostRecent();
    private void OnDisable() => FirebaseLoader.OnFirestoreInitialized -= async () => await QueryMostRecent();

    private async void Start()
    {
        if (FirebaseLoader.Firestore == null) return;
        await QueryMostRecent();
    }

    private async Task QueryMostRecent()
    {
        if (loaded) return;
        
        try
        {
            List<ArtworkData> artworkData = new List<ArtworkData>();
            if (FirebaseLoader.ArtworkCollectionFull)
            {
                artworkData = new List<ArtworkData>(
                    FirebaseLoader.Artworks
                        .OrderBy(e => e.update_date_time)
                        .Take(showCount)
                );
            }
            else artworkData = await FirebaseLoader.FetchMultipleDocuments<ArtworkData>("artworks", "update_time", showCount);
            
            
            foreach (var artwork in artworkData)
            {
                if (galleryCard == null) return;
                var card = Instantiate(galleryCard, parent);
                card.gameObject.SetActive(true);
                card.LoadARPoint(artwork);
                loaded = true;
            }
            
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching most recent artwork: {e.Message}");
        }
    }
}