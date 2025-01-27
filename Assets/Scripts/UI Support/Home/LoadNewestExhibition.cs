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
    
    private static List<ArtistData> cachedArtists;

    private void OnEnable() => FirebaseLoader.OnFirestoreInitialized += QueryMostRecent;
    private void OnDisable() => FirebaseLoader.OnFirestoreInitialized -= QueryMostRecent;
    

    private void Start()
    {
        // if (FirebaseLoader.Exhibitions == null || FirebaseLoader.Exhibitions.Count <= 0) return;
        if (FirebaseLoader.Firestore == null) return;
        QueryMostRecent();
    }

    private void QueryMostRecent()
    {
        // Create a query against the collection.
        Query query = FirebaseLoader.Firestore.Collection("exhibitions").OrderBy("creation_time").Limit(1);

        // Execute the query asynchronously
        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    Debug.Log($"Document ID: {document.Id}");
                    if (document.Exists)
                    {
                        var exhibition = document.ConvertTo<ExhibitionData>();
                        exhibition.exhibition_images = new List<Sprite>(exhibitionImages);
                        exhibitionCard.Init(exhibition);

                        if (cachedArtists == null || cachedArtists.Count <= 0)
                        {
                            cachedArtists = new List<ArtistData>();
                            
                            foreach (var artwork in exhibition.artworks)
                            {
                                cachedArtists.AddRange(artwork.artists);
                            }
                        }

                        if (cachedArtists.Count > 0)
                        {
                            artistContainer.Assign(cachedArtists[Random.Range(0, cachedArtists.Count)]);
                        }
                    }
                }

                if (snapshot.Count == 0)
                {
                    Debug.Log("No documents found in the collection.");
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve documents: {task.Exception}");
            }
        });
    }
}