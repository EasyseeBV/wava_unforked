using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseLoader : MonoBehaviour
{
    private FirebaseFirestore _firestore;

    // List to store loaded artworks
    public List<FirebaseData> Artworks { get; private set; } = new List<FirebaseData>();

    private void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                _firestore = FirebaseFirestore.DefaultInstance;
                LoadArtworks();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }

    // Method to load artworks from the Firestore collection
    public void LoadArtworks()
    {
        _firestore.Collection("artworks").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error loading artworks: " + task.Exception);
                return;
            }

            var querySnapshot = task.Result;
            Artworks.Clear();

            foreach (var document in querySnapshot.Documents)
            {
                FirebaseData artwork = document.ConvertTo<FirebaseData>();
                Artworks.Add(artwork);
                Debug.Log("Loaded artwork: " + artwork.ArtworkTitle);
            }
        });
    }
}