using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class LoadHighlightedArtist : MonoBehaviour
{
    [SerializeField] private ArtistContainer artistContainer;

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
            ArtistData artistData = null;
            if (FirebaseLoader.Artists.Count > 0)
                artistData = FirebaseLoader.Artists[Random.Range(0, FirebaseLoader.Artists.Count)];
            else
                artistData = await FirebaseLoader.FetchSingleDocument<ArtistData>("artists", "creation_time", 1);
            
            if (artistData != null)
            {
                artistContainer.Assign(artistData);
                loaded = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching most recent exhibition: {e.Message}");
        }
    }
}
