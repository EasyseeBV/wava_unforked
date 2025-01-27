using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Extensions;
using Firebase.Firestore;
using Messy.Definitions;
using UnityEngine;

public class LoadNewestArtwork : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GalleryCard galleryCard;
    [SerializeField] private int showCount = 4;
    [SerializeField] private Transform parent;

    [Header("Debugging")]
    [SerializeField] private List<Sprite> debugSprites;
    
    private void OnEnable() => FirebaseLoader.OnFirestoreInitialized += QueryMostRecent;
    private void OnDisable() => FirebaseLoader.OnFirestoreInitialized -= QueryMostRecent;

    private void Start()
    {
        if (FirebaseLoader.Firestore == null) return;
        QueryMostRecent();
    }

    private void QueryMostRecent()
    {
        // Create a query against the collection.
        Query query = FirebaseLoader.Firestore.Collection("artworks").OrderBy("creation_time").Limit(showCount);

        // Execute the query asynchronously
        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        var artwork = document.ConvertTo<ArtworkData>();
                        artwork.artwork_images = new List<Sprite>();
                        artwork.artwork_images.Add(debugSprites[Random.Range(0, debugSprites.Count)]);
                        var card = Instantiate(galleryCard, parent);
                        card.gameObject.SetActive(true);
                        card.LoadARPoint(artwork);
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