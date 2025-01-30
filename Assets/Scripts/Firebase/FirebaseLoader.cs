using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;
using Action = System.Action;

public class FirebaseLoader : MonoBehaviour
{
    // Firestore Instance
    public static FirebaseFirestore Firestore => _firestore;
    private static FirebaseFirestore _firestore = null;

    // Data Collections
    public static List<ArtworkData> Artworks { get; private set; } = new List<ArtworkData>();
    public static List<ArtistData> Artists { get; private set; } = new List<ArtistData>();
    public static List<ExhibitionData> Exhibitions { get; private set; } = new List<ExhibitionData>();

    // Caching Maps
    private static Dictionary<string, ArtistData> ArtistsMap = new Dictionary<string, ArtistData>();
    private static Dictionary<string, ArtworkData> ArtworksMap = new Dictionary<string, ArtworkData>();
    private static Dictionary<string, ExhibitionData> ExhibitionsMap = new Dictionary<string, ExhibitionData>();

    // References to Sprites
    [Header("Debugging")]
    [SerializeField] private Sprite artistIcon;
    [SerializeField] private List<Sprite> artworkImages = new List<Sprite>();
    [SerializeField] private List<Sprite> exhibitionImages = new List<Sprite>();

    private static Sprite artistIconRef;
    private static List<Sprite> artworkImagesRef = new List<Sprite>();
    private static List<Sprite> exhibitionImagesRef = new List<Sprite>();

    // Pagination
    private static DocumentSnapshot lastOpenedDocument = null;

    // Initialization Callback
    public static Action OnFirestoreInitialized;

    // SemaphoreSlim instances for concurrency control
    private static readonly SemaphoreSlim artistSemaphore = new SemaphoreSlim(1, 1);
    private static readonly SemaphoreSlim artworkSemaphore = new SemaphoreSlim(1, 1);

    private void Awake()
    {
        if (_firestore != null) return;

        // Assign Sprite References
        artistIconRef = artistIcon;
        artworkImagesRef = artworkImages;
        exhibitionImagesRef = exhibitionImages;

        InitializeFirebase();
    }

    /// <summary>
    /// Initializes Firebase and Firestore.
    /// </summary>
    private async void InitializeFirebase()
    {
        try
        {
            DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                _firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase initialized successfully.");

                //await LoadAllDataOnStart();
                
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

    #region Helper Methods for Fetching Single Items

    /// <summary>
    /// Retrieves an ArtistData by ID, fetching from Firestore if not present in the cache.
    /// </summary>
    /// <param name="artistId">The ID of the artist.</param>
    /// <returns>The ArtistData object or null if not found.</returns>
    public static async Task<ArtistData> GetArtistByIdAsync(string artistId)
    {
        // Check if artist is already in the map
        if (ArtistsMap.TryGetValue(artistId, out ArtistData existingArtist))
        {
            return existingArtist;
        }

        await artistSemaphore.WaitAsync();
        
        try
        {
            // Double-check after acquiring semaphore
            if (ArtistsMap.TryGetValue(artistId, out existingArtist))
            {
                return existingArtist;
            }

            // Fetch from Firestore
            DocumentSnapshot document = await _firestore.Collection("artists").Document(artistId).GetSnapshotAsync();
            if (document.Exists)
            {
                ArtistData artist = document.ConvertTo<ArtistData>();
                artist.icon = artistIconRef; // Assign the icon
                ArtistsMap[artistId] = artist; // Add to cache
                Artists.Add(artist); // Add to list
                Debug.Log($"Loaded artist '{artist.title}' from Firestore.");
                return artist;
            }
            else
            {
                Debug.LogWarning($"Artist document '{artistId}' does not exist in Firestore.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load artist '{artistId}': {e.Message}\n{e.StackTrace}");
            return null;
        }
        finally
        {
            artistSemaphore.Release();
        }
    }

    /// <summary>
    /// Retrieves an ArtworkData by ID, fetching from Firestore if not present in the cache.
    /// </summary>
    /// <param name="artworkId">The ID of the artwork.</param>
    /// <returns>The ArtworkData object or null if not found.</returns>
    public static async Task<ArtworkData> GetArtworkByIdAsync(string artworkId)
    {
        // Check if artwork is already in the map
        if (ArtworksMap.TryGetValue(artworkId, out ArtworkData existingArtwork))
        {
            return existingArtwork;
        }

        await artworkSemaphore.WaitAsync();
        
        try
        {
            // Double-check after acquiring semaphore
            if (ArtworksMap.TryGetValue(artworkId, out existingArtwork))
            {
                return existingArtwork;
            }

            // Fetch from Firestore
            DocumentSnapshot document = await _firestore.Collection("artworks").Document(artworkId).GetSnapshotAsync();
            if (document.Exists)
            {
                ArtworkData artwork = document.ConvertTo<ArtworkData>();
                artwork.artwork_images = new List<Sprite>(artworkImagesRef); // Assign artwork images
                ArtworksMap[artworkId] = artwork; // Add to cache
                Artworks.Add(artwork); // Add to list
                Debug.Log($"Loaded artwork '{artwork.title}' from Firestore.");
                return artwork;
            }
            else
            {
                Debug.LogWarning($"Artwork document '{artworkId}' does not exist in Firestore.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load artwork '{artworkId}': {e.Message}\n{e.StackTrace}");
            return null;
        }
        finally
        {
            artworkSemaphore.Release();
        }
    }

    private async Task LoadAllDataOnStart()
    {
        await LoadAllArtists();
        await LoadAllArtworks();
        await LoadAllExhibitions();
    }

    #endregion
    
    #region Loading All FirebaseData
    
    /// <summary>
    /// Loads all artists from Firestore and populates the cache and list.
    /// </summary>
    public static async Task LoadAllArtists()
    {
        try
        {
            if (_firestore == null)
            {
                Debug.LogError("Firebase Firestore has not been initialized.");
                return;
            }

            QuerySnapshot snapshot = await _firestore.Collection("artists").GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    ArtistData artist = document.ConvertTo<ArtistData>();
                    Debug.Log($"Loaded artist '{artist.title}'");
                    artist.icon = artistIconRef; // Assign the icon
                    Artists.Add(artist);
                    ArtistsMap[document.Id] = artist; // Populate the cache
                }
                else
                {
                    Debug.LogWarning($"Artist document '{document.Id}' does not exist.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load artists: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Loads all artworks from Firestore, assigns associated artists, and populates the cache and list.
    /// </summary>
    private static async Task LoadAllArtworks()
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
                    Debug.Log($"Loaded artwork: '{artwork.title}'");
                    artwork.artwork_images = new List<Sprite>(artworkImagesRef); // Assign artwork images
                    tempArtworks.Add(artwork);
                    ArtworksMap[artworkDoc.Id] = artwork; // Populate the cache
                }
                else
                {
                    Debug.LogWarning($"Artwork document '{artworkDoc.Id}' does not exist.");
                }
            }

            // Assign artists to artworks using the pre-loaded ArtistsMap or fetch if missing
            foreach (var artwork in tempArtworks)
            {
                foreach (var artistRef in artwork.artist_references)
                {
                    string artistId = artistRef.Id; // Extract the artist document ID

                    ArtistData artist = await GetArtistByIdAsync(artistId);
                    if (artist != null)
                    {
                        artwork.artists.Add(artist);
                        Debug.Log($"Assigned artist '{artist.title}' to artwork '{artwork.title}'");
                    }
                    else
                    {
                        Debug.LogWarning($"Artist with ID '{artistId}' could not be assigned to artwork '{artwork.title}'.");
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

    /// <summary>
    /// Loads all exhibitions from Firestore, assigns associated artists and artworks, and populates the list.
    /// </summary>
    private static async Task LoadAllExhibitions()
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
                    exhibition.exhibition_images = new List<Sprite>(exhibitionImagesRef); // Assign exhibition images
                    Debug.Log($"Loaded exhibition: '{exhibition.title}'");
                    tempExhibitions.Add(exhibition);
                    ExhibitionsMap[exhibitionDoc.Id] = exhibition;
                }
                else
                {
                    Debug.LogWarning($"Exhibition document '{exhibitionDoc.Id}' does not exist.");
                }
            }

            // Assign artists and artworks to exhibitions using the pre-loaded maps or fetch if missing
            foreach (var exhibition in tempExhibitions)
            {
                // Associate Artists
                foreach (var artistRef in exhibition.artist_references)
                {
                    string artistId = artistRef.Id;

                    ArtistData artist = await GetArtistByIdAsync(artistId);
                    if (artist != null)
                    {
                        exhibition.artists.Add(artist);
                        Debug.Log($"Assigned artist '{artist.title}' to exhibition '{exhibition.title}'");
                    }
                    else
                    {
                        Debug.LogWarning($"Artist with ID '{artistId}' could not be assigned to exhibition '{exhibition.title}'.");
                    }
                }

                // Associate Artworks
                foreach (var artworkRef in exhibition.artwork_references)
                {
                    string artworkId = artworkRef.Id;

                    ArtworkData artwork = await GetArtworkByIdAsync(artworkId);
                    if (artwork != null)
                    {
                        exhibition.artworks.Add(artwork);
                        Debug.Log($"Assigned artwork '{artwork.title}' to exhibition '{exhibition.title}'");
                    }
                    else
                    {
                        Debug.LogWarning($"Artwork with ID '{artworkId}' could not be assigned to exhibition '{exhibition.title}'.");
                    }
                }

                // Optionally load exhibition images if needed
                // await LoadExhibitionImages(exhibition);

                Exhibitions.Add(exhibition);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load exhibitions: {e.Message}\n{e.StackTrace}");
        }
    }

    #endregion

    #region Fetching

    /// <summary>
    /// Fetches a limited number of documents from a specified collection with pagination support.
    /// </summary>
    /// <typeparam name="T">The type of data to fetch (ArtworkData, ArtistData, ExhibitionData).</typeparam>
    /// <param name="limit">The maximum number of documents to fetch.</param>
    public static async Task FetchDocuments<T>(int limit, string sorting = "creation_time")
    {
        try
        {
            if (_firestore == null)
            {
                Debug.LogError("Firebase Firestore has not been initialized.");
                return;
            }

            string collectionName = typeof(T) switch
            {
                Type t when t == typeof(ArtworkData) => "artworks",
                Type t when t == typeof(ArtistData) => "artists",
                Type t when t == typeof(ExhibitionData) => "exhibitions",
                _ => "collection"
            };

            Query query = _firestore.Collection(collectionName).OrderBy(sorting).Limit(limit);
            if (lastOpenedDocument != null) query = query.StartAfter(lastOpenedDocument);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    if (typeof(T) == typeof(ArtworkData))
                    {
                        var data = document.ConvertTo<ArtworkData>();
                        Debug.Log($"Loaded artwork: '{data.title}'");
                        data.artwork_images = new List<Sprite>(artworkImagesRef);
                        Artworks.Add(data);
                        ArtworksMap[document.Id] = data;
                    }
                    else if (typeof(T) == typeof(ArtistData))
                    {
                        var data = document.ConvertTo<ArtistData>();
                        Debug.Log($"Loaded artist: '{data.title}'");
                        data.icon = artistIconRef;
                        Artists.Add(data);
                        ArtistsMap[document.Id] = data;
                    }
                    else if (typeof(T) == typeof(ExhibitionData))
                    {
                        var data = document.ConvertTo<ExhibitionData>();
                        Debug.Log($"Loaded exhibition: '{data.title}'");
                        data.exhibition_images = new List<Sprite>(exhibitionImagesRef);
                        Exhibitions.Add(data);
                    }

                    // Update lastOpenedDocument for pagination
                    lastOpenedDocument = document;
                }
                else
                {
                    Debug.LogWarning($"Document '{document.Id}' does not exist in collection '{collectionName}'.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch documents: {e.Message}\n{e.StackTrace}");
        }
    }
    
    public static async Task<T> FetchSingleDocument<T>(string collection, string sort, int limit) where T : class
    {
        Query query = _firestore.Collection(collection).OrderBy(sort).Limit(limit);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        var document = snapshot.Documents.FirstOrDefault();
        
        if (document == null) return null;

        if (typeof(T) == typeof(ArtworkData))
        {
            if (ArtworksMap.TryGetValue(document.Id, out var artwork))
            {
                Debug.Log($"Found artwork: '{artwork.title}'");
                return artwork as T;
            }
            
            var data = document?.ConvertTo<ArtworkData>();
            if (data == null)
            {
                Debug.LogWarning($"Could not find an Artwork data for document '{document.Id}'. With the sort request: '{sort}'.");
                return null;
            }
            
            Debug.Log($"Loaded artwork: '{data.title}'");
            data.artwork_images = new List<Sprite>(artworkImagesRef);
            Artworks.Add(data);
            ArtworksMap[document.Id] = data;
            
            return data as T;
        }
        else if (typeof(T) == typeof(ArtistData))
        {
            if (ArtistsMap.TryGetValue(document.Id, out var artist))
            {
                Debug.Log($"Found artist: '{artist.title}'");
                return artist as T;
            }
            
            var data = document?.ConvertTo<ArtistData>();
            if (data == null)
            {
                Debug.LogWarning($"Could not find an Artist data for document '{document.Id}'. With the sort request: '{sort}'.");
                return null;
            }
            
            Debug.Log($"Loaded artist: '{data.title}'");
            data.icon = artistIconRef;
            Artists.Add(data);
            ArtistsMap[document.Id] = data;
            
            return data as T;
        }
        else if (typeof(T) == typeof(ExhibitionData))
        {
            if (ExhibitionsMap.TryGetValue(document.Id, out var exhibition))
            {
                Debug.Log($"Found exhibition: '{exhibition.title}'");
                return exhibition as T;
            }
            
            var data = document?.ConvertTo<ExhibitionData>();
            if (data == null)
            {
                Debug.LogWarning($"Could not find an Exhibition data for document '{document.Id}'. With the sort request: '{sort}'.");
                return null;
            }
            
            Debug.Log($"Loaded exhibition: '{data.title}'");
            data.exhibition_images = new List<Sprite>(exhibitionImagesRef);
            Exhibitions.Add(data);
            ExhibitionsMap[document.Id] = data;
            
            return data as T;
        }
        
        return null;
    }

    public static async Task<List<T>> FetchMultipleDocuments<T>(string collection, string sort, int limit) where T : class
    {
        List<T> results = new List<T>();
        

        Query query = _firestore.Collection(collection).OrderBy(sort).Limit(limit);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        foreach (var document in snapshot.Documents)
        {
            if (typeof(T) == typeof(ArtworkData))
            {
                if (ArtworksMap.TryGetValue(document.Id, out var artwork))
                {
                    Debug.Log($"Found cached artwork: '{artwork.title}'");
                    results.Add(artwork as T);
                    continue;
                }

                var data = document.ConvertTo<ArtworkData>();
                if (data != null)
                {
                    Debug.Log($"Loaded artwork: '{data.title}'");
                    data.artwork_images = new List<Sprite>(artworkImagesRef);
                    Artworks.Add(data);
                    ArtworksMap[document.Id] = data;
                    results.Add(data as T);
                }
            }
            else if (typeof(T) == typeof(ArtistData))
            {
                if (ArtistsMap.TryGetValue(document.Id, out var artist))
                {
                    Debug.Log($"Found cached artist: '{artist.title}'");
                    results.Add(artist as T);
                    continue;
                }

                var data = document.ConvertTo<ArtistData>();
                if (data != null)
                {
                    Debug.Log($"Loaded artist: '{data.title}'");
                    data.icon = artistIconRef;
                    Artists.Add(data);
                    ArtistsMap[document.Id] = data;
                    results.Add(data as T);
                }
            }
            else if (typeof(T) == typeof(ExhibitionData))
            {
                if (ExhibitionsMap.TryGetValue(document.Id, out var exhibition))
                {
                    Debug.Log($"Found cached exhibition: '{exhibition.title}'");
                    results.Add(exhibition as T);
                    continue;
                }

                var data = document.ConvertTo<ExhibitionData>();
                if (data != null)
                {
                    Debug.Log($"Loaded exhibition: '{data.title}'");
                    data.exhibition_images = new List<Sprite>(exhibitionImagesRef);
                    Exhibitions.Add(data);
                    ExhibitionsMap[document.Id] = data;
                    results.Add(data as T);
                }
            }
        }

        return results;
    }


    #endregion

    #region Exhibition Images Loading

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
            foreach (var artist in exhibition.artists)
            {
                Debug.Log($"  Artist: {artist.title}");
            }
            foreach (var artwork in exhibition.artworks)
            {
                Debug.Log($"  Artwork: {artwork.title}");
            }
        }
        Debug.Log("----- End of Summary -----");
    }
    */

    #endregion
}
