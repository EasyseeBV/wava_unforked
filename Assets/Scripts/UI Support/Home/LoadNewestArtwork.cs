using System;
using System.Collections.Generic;
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
            Debug.Log("fetching...");
            var artworkData = await FirebaseLoader.FetchMultipleDocuments<ArtworkData>("artworks", "creation_time", showCount);
            Debug.Log(artworkData.Count);
            foreach (var artwork in artworkData)
            {
                var card = Instantiate(galleryCard, parent);
                card.gameObject.SetActive(true);
                card.LoadARPoint(artwork);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching most recent exhibition: {e.Message}");
        }
    }
}