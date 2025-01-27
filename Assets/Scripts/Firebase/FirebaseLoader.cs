using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseLoader : MonoBehaviour
{
    public static FirebaseFirestore Firestore => _firestore;
    private static FirebaseFirestore _firestore;
    
    public static List<ArtworkData> Artworks { get; private set; } = new List<ArtworkData>();
    public static List<ArtistData> Artists { get; private set; } = new List<ArtistData>();
    public static List<ExhibitionData> Exhibitions { get; private set; } = new List<ExhibitionData>();
    
    private Dictionary<string, ArtistData> ArtistsMap = new Dictionary<string, ArtistData>();
    private Dictionary<string, ArtworkData> ArtworksMap = new Dictionary<string, ArtworkData>();

    [Header("Debugging")]
    [SerializeField] private Sprite artistIcon;
    [SerializeField] private List<Sprite> artworkImages = new List<Sprite>();
    [SerializeField] private List<Sprite> exhibitionImages = new List<Sprite>();
    
    public static Action OnFirestoreInitialized;
    
    private void Awake()
    {
        if (_firestore != null) return;
        InitializeFirebase();
    }
    
    private async void InitializeFirebase()
    {
        try
        {
            DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                _firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase initialized successfully.");

                Debug.LogWarning("Loading all firebase content - this should be removed in the future");
                
                await LoadArtists();    // Await the completion of LoadArtists
                await LoadArtworks();   // Proceed to LoadArtworks after artists are loaded
                await LoadExhibitions();
                
                OnFirestoreInitialized?.Invoke();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase initialization failed: {e.Message}\n{e.StackTrace}");
        }
    }

    private async Task LoadArtists()
    {
        try
        {
            if (_firestore == null)
            {
                Debug.Log("Firebase database has not loaded");
                return;
            }
            
            QuerySnapshot snapshot = await _firestore.Collection("artists").GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    ArtistData artist = document.ConvertTo<ArtistData>();
                    Debug.Log($"Loaded artist {artist.title}");
                    artist.icon = artistIcon; // DEBUGGING
                    Artists.Add(artist);
                    ArtistsMap[document.Id] = artist; // Populate the dictionary
                }
                else
                {
                    Debug.LogWarning($"Artist document {document.Id} does not exist.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get artists: {e.Message}\n{e.StackTrace}");
        }
    }

    private async Task LoadArtworks()
    {
        try
        {
            if (_firestore == null)
            {
                Debug.LogError("Firebase Firestore has not been initialized.");
                return;
            }

            QuerySnapshot artworkSnapshot = await _firestore.Collection("artworks").GetSnapshotAsync();
            List<ArtworkData> tempArtworks = new List<ArtworkData>();

            foreach (DocumentSnapshot artworkDoc in artworkSnapshot.Documents)
            {
                if (artworkDoc.Exists)
                {
                    ArtworkData artwork = artworkDoc.ConvertTo<ArtworkData>();
                    Debug.Log($"Loaded artwork: {artwork.title}");
                    artwork.artwork_images = new List<Sprite>(artworkImages);
                    tempArtworks.Add(artwork);
                    ArtworksMap[artworkDoc.Id] = artwork;
                }
                else
                {
                    Debug.LogWarning($"Artwork document {artworkDoc.Id} does not exist.");
                }
            }

            // Assign artists to artworks using the pre-loaded ArtistsMap
            foreach (var artwork in tempArtworks)
            {
                foreach (var artistRef in artwork.artist_reference)
                {
                    string artistId = artistRef.Id; // Extract the artist document ID

                    if (ArtistsMap.TryGetValue(artistId, out ArtistData artist))
                    {
                        artwork.artists.Add(artist);
                        Debug.Log($"Assigned artist '{artist.title}' to artwork '{artwork.title}'");
                    }
                    else
                    {
                        Debug.LogWarning($"Artist with ID '{artistId}' not found in ArtistsMap.");
                    }
                }

                Artworks.Add(artwork);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load artworks: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private async Task LoadExhibitions()
    {
        try
        {
            if (_firestore == null)
            {
                Debug.LogError("Firebase Firestore has not been initialized.");
                return;
            }

            QuerySnapshot exhibitionSnapshot = await _firestore.Collection("exhibitions").GetSnapshotAsync();
            List<ExhibitionData> tempExhibitions = new List<ExhibitionData>();

            foreach (DocumentSnapshot exhibitionDoc in exhibitionSnapshot.Documents)
            {
                if (exhibitionDoc.Exists)
                {
                    ExhibitionData exhibition = exhibitionDoc.ConvertTo<ExhibitionData>();
                    exhibition.exhibition_images = new List<Sprite>(exhibitionImages);
                    Debug.Log($"Loaded exhibition: {exhibition.title}");
                    tempExhibitions.Add(exhibition);
                }
                else
                {
                    Debug.LogWarning($"Exhibition document {exhibitionDoc.Id} does not exist.");
                }
            }

            // Assign artists and artworks to exhibitions using the pre-loaded maps
            foreach (var exhibition in tempExhibitions)
            {
                // Associate Artists
                foreach (var artistRef in exhibition.artist_references)
                {
                    string artistId = artistRef.Id;

                    if (ArtistsMap.TryGetValue(artistId, out ArtistData artist))
                    {
                        exhibition.artists.Add(artist);
                        Debug.Log($"Assigned artist '{artist.title}' to exhibition '{exhibition.title}'");
                    }
                    else
                    {
                        Debug.LogWarning($"Artist with ID '{artistId}' not found in ArtistsMap.");
                    }
                }

                // Associate Artworks
                foreach (var artworkRef in exhibition.artwork_references)
                {
                    string artworkId = artworkRef.Id;

                    if (ArtworksMap.TryGetValue(artworkId, out ArtworkData artwork))
                    {
                        exhibition.artworks.Add(artwork);
                        Debug.Log($"Assigned artwork '{artwork.title}' to exhibition '{exhibition.title}'");
                    }
                    else
                    {
                        Debug.LogWarning($"Artwork with ID '{artworkId}' not found in ArtworksMap.");
                    }
                }
                
                // await LoadExhibitionImages(exhibition);

                Exhibitions.Add(exhibition);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load exhibitions: {e.Message}\n{e.StackTrace}");
        }
    }

    /*
    /// <summary>
    /// Loads exhibition images from URLs and converts them to Sprites.
    /// </summary>
    /// <param name="exhibition">The exhibition for which to load images.</param>
    private async Task LoadExhibitionImages(ExhibitionData exhibition)
    {
        foreach (var imageUrl in exhibition.exhibition_images)
        {
            try
            {
                UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    Sprite sprite = SpriteFromTexture2D(texture);
                    exhibition.exhibition_images.Add(sprite);
                    Debug.Log($"Loaded exhibition image from URL: {imageUrl}");
                }
                else
                {
                    Debug.LogWarning($"Failed to load image from URL '{imageUrl}': {request.error}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading exhibition image from URL '{imageUrl}': {e.Message}");
            }
        }
    }

    /// <summary>
    /// Converts a Texture2D to a Sprite.
    /// </summary>
    /// <param name="texture">The Texture2D to convert.</param>
    /// <returns>A Sprite created from the Texture2D.</returns>
    private Sprite SpriteFromTexture2D(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Logs the loaded data for verification.
    /// </summary>
    private void LogLoadedData()
    {
        Debug.Log("----- Loaded Data Summary -----");
        Debug.Log($"Artists Loaded: {Artists.Count}");
        Debug.Log($"Artworks Loaded: {Artworks.Count}");
        Debug.Log($"Exhibitions Loaded: {Exhibitions.Count}");

        // Optionally, log details
        foreach (var exhibition in Exhibitions)
        {
            Debug.Log($"Exhibition: {exhibition.title}");
            foreach (var artist in exhibition.artistData)
            {
                Debug.Log($"  Artist: {artist.title}");
            }
            foreach (var artwork in exhibition.artworkData)
            {
                Debug.Log($"  Artwork: {artwork.title}");
            }
        }
        Debug.Log("----- End of Summary -----");
    }
    */
}