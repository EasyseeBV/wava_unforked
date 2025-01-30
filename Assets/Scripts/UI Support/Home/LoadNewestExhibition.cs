using System;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;


public class LoadNewestExhibition : MonoBehaviour
{
    [SerializeField] private ExhibitionCard exhibitionCard;
    [SerializeField] private ArtistContainer artistContainer;

    [Header("Debugging")]
    [SerializeField] private List<Sprite> exhibitionImages = new List<Sprite>();
    
    private bool loaded = false;
    
    private static List<ArtistData> cachedArtists;

    private void OnEnable() => FirebaseLoader.OnFirestoreInitialized += async () => await QueryMostRecent();

    private void OnDisable() => FirebaseLoader.OnFirestoreInitialized -= async () => await QueryMostRecent();

    private async void Start()
    {
        // if (FirebaseLoader.Exhibitions == null || FirebaseLoader.Exhibitions.Count <= 0) return;
        if (FirebaseLoader.Firestore == null) return;
        await QueryMostRecent();
    }

    private async Task QueryMostRecent()
    {
        if (loaded) return;
        
        try
        {
            var exhibition = await FirebaseLoader.FetchSingleDocument<ExhibitionData>("exhibitions", "creation_time", 1);
            if (exhibition != null)
            {
                exhibitionCard.Init(exhibition);
                loaded = true;

                if (FirebaseLoader.Artists.Count > 0)
                {
                    artistContainer.Assign(FirebaseLoader.Artists[Random.Range(0, FirebaseLoader.Artists.Count)]);
                }
                else
                {
                    ArtistData artistData = await FirebaseLoader.GetArtistByIdAsync(exhibition.artist_references[Random.Range(0, exhibition.artist_references.Count)].Id);
                    if (artistData != null)
                    {
                        artistContainer.Assign(artistData);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching most recent exhibition: {e.Message}");
        }
    }
}