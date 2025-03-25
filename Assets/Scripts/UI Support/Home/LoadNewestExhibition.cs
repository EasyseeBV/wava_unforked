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
    
    private bool loaded = false;

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
            ExhibitionData exhibition = null;
            if (FirebaseLoader.ExhibitionCollectionFull)
            {
                Debug.Log("full collection.. loading from collection");
                exhibition = FirebaseLoader.Exhibitions.OrderByDescending(e => e.creation_date_time).FirstOrDefault();
            }

            if (exhibition == null)
            {
                exhibition = await FirebaseLoader.FetchSingleDocument<ExhibitionData>("exhibitions", "creation_time", 1);
                await FirebaseLoader.LoadRemainingExhibitions();
            }
            
            if (exhibition != null)
            {
                exhibitionCard.Init(exhibition);
                loaded = true;
            }
            else
            {
                Debug.Log("exhibition was null");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching most recent exhibition: {e.Message}");
        }
    }
}