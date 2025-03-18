using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.Networking;
using Action = System.Action;

public class FirebaseLoader : MonoBehaviour
{
    // Firestore Instance
    public static FirebaseFirestore Firestore => _firestore;
    private static FirebaseFirestore _firestore = null;
    
    public static bool Initialized { get; private set; } = false;

    // Data Collections
    public static List<ArtworkData> Artworks { get; set; } = new List<ArtworkData>();
    public static List<ArtistData> Artists { get; set; } = new List<ArtistData>();
    public static List<ExhibitionData> Exhibitions { get; set; } = new List<ExhibitionData>();

    // Caching Maps
    private static Dictionary<string, ArtistData> ArtistsMap = new Dictionary<string, ArtistData>();
    private static Dictionary<string, ArtworkData> ArtworksMap = new Dictionary<string, ArtworkData>();
    private static Dictionary<string, ExhibitionData> ExhibitionsMap = new Dictionary<string, ExhibitionData>();

    // Pagination
    private static DocumentSnapshot lastOpenedDocument = null;

    // Callbacks
    public static Action OnFirestoreInitialized;
    public static Action OnNewDocumentsFetched;

    // SemaphoreSlim instances for concurrency control
    private static readonly SemaphoreSlim artistSemaphore = new SemaphoreSlim(1, 1);
    private static readonly SemaphoreSlim artworkSemaphore = new SemaphoreSlim(1, 1);
    
    // Collection Sizes
    public static long ArtworkCollectionSize { get; private set; } = -1;
    public static long ExhibitionCollectionSize { get; private set; } = -1;
    public static long ArtistCollectionSize { get; private set; } = -1;
    
    // Bool states
    public static bool ArtworkCollectionFull => Artworks.Count >= ArtworkCollectionSize;
    public static bool ExhibitionCollectionFull => Exhibitions.Count >= ExhibitionCollectionSize;
    public static bool ArtistCollectionFull => Artists.Count >= ArtistCollectionSize;
    
    private const int MAX_NOT_IN = 10;

    private void Awake()
    {
        if (_firestore != null) return;
        InitializeFirebase();
    }

    /// <summary>
    /// Initializes Firebase and Firestore.
    /// </summary>
    private async void InitializeFirebase()
    {
        while (!Initialized)
        {
            try
            {
                DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus == DependencyStatus.Available)
                {
                    _firestore = FirebaseFirestore.DefaultInstance;
                    Debug.Log("Firebase initialized successfully.");

                    if (_firestore.Settings == null)
                    {
                        Debug.LogWarning("Firestore settings are empty");
                    }

                    AppCache.LoadLocalCaches();
                    await GetCollectionCountsAsync();

                    Initialized = true;
                    OnFirestoreInitialized?.Invoke();
                    break; // Exit loop upon successful initialization.
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

            if (!Initialized)
            {
                Debug.LogWarning("Retrying Firebase initialization in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
    
    #region Add Data

    public static void AddArtworkData(ArtworkData data)
    {
        if (ArtworksMap.ContainsKey(data.id))
        {
            Debug.LogWarning("Tried to add an exhibition with a document id already stored in the artwork map: " + data.title);
            return;
        }
        
        Artworks.Add(data);
        ArtworksMap[data.id] = data;
    }

    public static void AddArtistData(ArtistData data)
    {
        if (ArtistsMap.ContainsKey(data.id))
        {
            Debug.LogWarning("Tried to add an exhibition with a document id already stored in the artist map: " + data.title);
            return;
        }
        
        Artists.Add(data);
        ArtistsMap[data.id] = data;
    }

    public static void AddExhibitionData(ExhibitionData data)
    {
        if (ExhibitionsMap.ContainsKey(data.id))
        {
            Debug.LogWarning("Tried to add an exhibition with a document id already stored in the exhibition map: " + data.title);
            return;
        }
        
        Exhibitions.Add(data);
        ExhibitionsMap[data.id] = data;
    }
    
    #endregion
    
    #region Convert Document to Data
    
    private static async Task<ExhibitionData> ReadExhibitionDocument(DocumentSnapshot document)
    {
        if (ExhibitionsMap.ContainsKey(document.Id))
        {
            Debug.LogWarning("Reading an already existing [Exhibition] document...");
            return ExhibitionsMap[document.Id];    
        }
        
        ExhibitionData exhibition = document.ConvertTo<ExhibitionData>();
        exhibition.id = document.Id;
        //await LoadArtworkImages(exhibition);
        exhibition.creation_date_time = exhibition.creation_time.ToDateTime();
        exhibition.update_date_time = exhibition.update_time.ToDateTime();
        ExhibitionsMap.Add(document.Id, exhibition);
        Exhibitions.Add(exhibition);
        Debug.Log($"[Firebase] Loaded Exhibition: {document.Id}");
        
        return exhibition;
    }
    
    private static async Task<ArtworkData> ReadArtworkDocument(DocumentSnapshot document, bool loadImages = true)
    {
        if (ArtworksMap.ContainsKey(document.Id))
        {
            Debug.LogWarning("Reading an already existing [Artwork] document...");
            return ArtworksMap[document.Id];    
        }
        
        ArtworkData artwork = document.ConvertTo<ArtworkData>();
        artwork.id = document.Id;
        artwork.creation_date_time = artwork.creation_time.ToDateTime();
        artwork.update_date_time = artwork.update_time.ToDateTime();
        foreach (var documentReference in artwork.artist_references)
        {
            artwork.artists.Add(await ReadArtistDocumentReference(documentReference));
        }
        ArtworksMap.Add(document.Id, artwork);
        Artworks.Add(artwork);
        Debug.Log($"[Firebase] Loaded Artwork: {document.Id}");
        
        return artwork;
    }
    
    private static async Task<ArtistData> ReadArtistDocument(DocumentSnapshot document)
    {
        ArtistData artist = document.ConvertTo<ArtistData>();
        artist.id = document.Id;
        artist.creation_date_time = artist.creation_time.ToDateTime();
        artist.update_date_time = artist.update_time.ToDateTime();
        ArtistsMap[document.Id] = artist;
        Artists.Add(artist);
        Debug.Log($"[Firebase] Loaded artist: '{artist.title}'");
        
        return artist;
    }

    private static async Task<ArtistData> ReadArtistDocumentReference(DocumentReference document)
    {
        foreach (var artist in Artists.Where(artist => artist.id == document.Id))
        {
            return artist;
        }
        
        DocumentSnapshot snapshot = await document.GetSnapshotAsync();
        
        if (!snapshot.Exists)
        {
            Debug.LogError("Artist document does not exist for reference: " + document.Id);
            return null;
        }
        
        return await ReadArtistDocument(snapshot);
    }

    public static async Task<ExhibitionData> FindRelatedExhibition(string artwork_id)
    {
        // Get the Firestore database instance.
        var db = FirebaseFirestore.DefaultInstance;

        // Create a DocumentReference for the artwork.
        DocumentReference artworkRef = db.Collection("artworks").Document(artwork_id);

        // Build the query to find exhibitions that reference this artwork.
        Query query = db.Collection("exhibitions").WhereArrayContains("artwork_references", artworkRef);

        // Await the snapshot asynchronously.
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        // If at least one document is returned, convert the first document to ExhibitionData.
        if (snapshot != null && snapshot.Count > 0)
        {
            DocumentSnapshot exhibitionDoc = snapshot.Documents.FirstOrDefault();
            if (exhibitionDoc != null)
            {
                ExhibitionData exhibitionData = exhibitionDoc.ConvertTo<ExhibitionData>();
                return exhibitionData;
            }
        }

        // If no exhibition is found, return null.
        return null;
    }
    
    #endregion

    #region Helper Methods for Fetching Single Items
    
    private async Task GetCollectionCountsAsync()
    {
        try
        {
            var counts = await Task.WhenAll(
                GetCollectionCountAsync("artworks"),
                GetCollectionCountAsync("exhibitions"),
                GetCollectionCountAsync("artists")
            );

            ArtworkCollectionSize = counts[0];
            ExhibitionCollectionSize = counts[1];
            ArtistCollectionSize = counts[2];
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error fetching collection counts: {ex.Message}");
            throw;
        }
    }

    private async Task<long> GetCollectionCountAsync(string collectionName)
    {
        var collectionRef = _firestore.Collection(collectionName);
        var countQuery = collectionRef.Count;
        var snapshot = await countQuery.GetSnapshotAsync(AggregateSource.Server);
        return snapshot.Count;
    }

    #endregion
    
    #region Loading Chunks

    public static async Task LoadRemainingArtworks(Action onComplete)
    {
        Debug.Log("Trying to load all artworks");

        try
        {
            CollectionReference artworksRef = _firestore.Collection("artworks");
            QuerySnapshot snapshot = await artworksRef.GetSnapshotAsync();
            List<ArtworkData> newArtworks = new List<ArtworkData>();

            foreach (var document in snapshot.Documents)
            {
                string id = document.Id;
                // If the artwork already exists in the map, ignore it
                if (ArtworksMap.ContainsKey(id)) continue;
                
                try
                {
                    // Attempt to convert the document into your ArtworkData object
                    ArtworkData artwork = await ReadArtworkDocument(document, false);
                    ArtworksMap[id] = artwork;
                    newArtworks.Add(artwork);
                }
                catch (Exception ex)
                {
                    // Log error with the specific document ID
                    Debug.LogError($"Error processing document {id}: {ex.Message}\n{ex.StackTrace}");
                }
            }

            Debug.Log($"Loaded {newArtworks.Count} new artworks.");
            onComplete?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load artworks: {e.Message}\n{e.StackTrace}");
        }
    }

    public static async Task LoadRemainingExhibitions()
    {
        Debug.Log("Trying to load remaining exhibitions...");

        try
        {
            CollectionReference artworksRef = _firestore.Collection("exhibitions");
            List<string> loadedIds = new List<string>(ExhibitionsMap.Keys);

            List<Task<QuerySnapshot>> tasks = new List<Task<QuerySnapshot>>();
            List<ExhibitionData> newExhibitions = new List<ExhibitionData>();

            if (loadedIds.Count == 0)
            {
                QuerySnapshot snapshot = await artworksRef.GetSnapshotAsync();
                ProcessExhibitions(snapshot, newExhibitions);
            }
            else
            {
                // Firestore allows at most 10 elements in `WhereNotIn`
                for (int i = 0; i < loadedIds.Count; i += 10)
                {
                    List<string> batch = loadedIds.GetRange(i, Mathf.Min(10, loadedIds.Count - i));
                    Query query = artworksRef.WhereNotIn(FieldPath.DocumentId, batch);
                    tasks.Add(query.GetSnapshotAsync());
                }

                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    
                    ProcessExhibitions(task.Result, newExhibitions);
                }
            }

            Debug.Log($"Loaded {newExhibitions.Count} new newExhibitions.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load remaining artworks: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private static async void ProcessArtworks(QuerySnapshot snapshot, List<ArtworkData> newArtworks)
    {
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (!ArtworksMap.ContainsKey(document.Id))
            {
                newArtworks.Add(await ReadArtworkDocument(document));
            }
        }
        
        AppCache.SaveArtworksCache();
    }
    
    private static async void ProcessExhibitions(QuerySnapshot snapshot, List<ExhibitionData> newExhibitions)
    {
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (!ExhibitionsMap.ContainsKey(document.Id))
            {
                newExhibitions.Add(await ReadExhibitionDocument(document));
            }
        }
        
        AppCache.SaveArtworksCache();
    }

    public static async Task FillExhibitionArtworkData(ExhibitionData exhibition)
    {
        exhibition.artworks = new List<ArtworkData>();

        foreach (var artworkReference in exhibition.artwork_references)
        {
            if (ArtworksMap.TryGetValue(artworkReference.Id, out var value)) exhibition.artworks.Add(value);
            else exhibition.artworks.Add(await ReadArtworkDocument(await artworkReference.GetSnapshotAsync()));
        }

        if (exhibition.artworks.Count >= exhibition.artwork_references.Count) return;
        
        
    }

    #endregion

    #region Fetching

    /// <summary>
    /// Fetches a limited number of documents from a specified collection with pagination support.
    /// </summary>
    /// <typeparam name="T">The type of data to fetch (ArtworkData, ArtistData, ExhibitionData).</typeparam>
    /// <param name="limit">The maximum number of documents to fetch.</param>
    public static async Task<List<T>> FetchDocuments<T>(int limit, string sorting = "creation_time") where T : class
    {
        List<T> fetchedDocuments = new List<T>();

        try
        {
            if (_firestore == null)
            {
                Debug.LogError("Firebase Firestore has not been initialized.");
                return fetchedDocuments;
            }

            string collectionName = typeof(T) switch
            {
                Type t when t == typeof(ArtworkData) => "artworks",
                Type t when t == typeof(ArtistData) => "artists",
                Type t when t == typeof(ExhibitionData) => "exhibitions",
                _ => "collection"
            };

            // Determine the list of cached IDs based on type T
            List<string> cachedIds = typeof(T) switch
            {
                Type t when t == typeof(ArtworkData) => ArtworksMap.Keys.ToList(),
                Type t when t == typeof(ArtistData) => ArtistsMap.Keys.ToList(),
                Type t when t == typeof(ExhibitionData) => ExhibitionsMap.Keys.ToList(),
                _ => new List<string>()
            };

            bool hasArtworks = false, hasArtists = false, hasExhibitions = false;

            // Initialize Firestore collection reference
            CollectionReference collection = _firestore.Collection(collectionName);

            // Start building the base query
            Query baseQuery = collection.OrderBy(sorting);

            if (lastOpenedDocument != null)
            {
                baseQuery = baseQuery.StartAfter(lastOpenedDocument);
            }

            // Initialize a list to hold all fetched documents
            List<DocumentSnapshot> allFetchedDocuments = new List<DocumentSnapshot>();

            if (cachedIds.Count > 0)
            {
                // Exclude cached IDs using 'whereNotIn' in batches
                List<List<string>> batches = SplitList(cachedIds, MAX_NOT_IN);

                foreach (var batch in batches)
                {
                    Query query = baseQuery.WhereNotIn(FieldPath.DocumentId, batch).Limit(limit);
                    QuerySnapshot snapshot = await query.GetSnapshotAsync();

                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        if (document.Exists && !cachedIds.Contains(document.Id))
                        {
                            allFetchedDocuments.Add(document);
                            if (allFetchedDocuments.Count >= limit)
                            {
                                break;
                            }
                        }
                    }

                    if (allFetchedDocuments.Count >= limit)
                    {
                        break;
                    }
                }
            }
            else
            {
                // No cached IDs, perform a single query
                QuerySnapshot snapshot = await baseQuery.Limit(limit).GetSnapshotAsync();
                allFetchedDocuments.AddRange(snapshot.Documents);
            }

            // Process the fetched documents
            foreach (DocumentSnapshot document in allFetchedDocuments)
            {
                if (document.Exists)
                {
                    if (typeof(T) == typeof(ArtworkData))
                    {
                        ArtworkData data = null;
                        
                        data = await ReadArtworkDocument(document);
                        
                        fetchedDocuments.Add(data as T);
                        hasArtworks = true;
                    }
                    else if (typeof(T) == typeof(ArtistData))
                    {
                        var data = await ReadArtistDocument(document);
                        fetchedDocuments.Add(data as T);
                        hasArtists = true;
                    }
                    else if (typeof(T) == typeof(ExhibitionData))
                    {
                        var data = await ReadExhibitionDocument(document);
                        fetchedDocuments.Add(data as T);
                        hasExhibitions = true;
                    }

                    // Update lastOpenedDocument for pagination
                    lastOpenedDocument = document;

                    if (fetchedDocuments.Count >= limit)
                    {
                        break;
                    }
                }
                else
                {
                    Debug.LogWarning($"Document '{document.Id}' does not exist in collection '{collectionName}'.");
                }
                
                if (hasArtworks) AppCache.SaveArtworksCache();
                if (hasExhibitions) AppCache.SaveExhibitionsCache();
                if (hasArtists) AppCache.SaveArtistsCache();
            }
        }
        catch (FirebaseException fe)
        {
            Debug.LogError($"Firebase error while fetching documents: {fe.Message}\n{fe.StackTrace}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Unexpected error while fetching documents: {e.Message}\n{e.StackTrace}");
        }

        OnNewDocumentsFetched?.Invoke();
        return fetchedDocuments;
    }

    /// <summary>
    /// Splits a list into smaller batches.
    /// </summary>
    private static List<List<string>> SplitList(List<string> source, int batchSize)
    {
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / batchSize)
            .Select(g => g.Select(x => x.Value).ToList())
            .ToList();
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

            // If the stored data does not exist, read the document
            data = await ReadArtworkDocument(document);
            
            AppCache.SaveArtworksCache();
            
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

            data = await ReadArtistDocument(document);
            
            AppCache.SaveArtistsCache();
            
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
            
            data = await ReadExhibitionDocument(document);
            
            AppCache.SaveExhibitionsCache();
            
            return data as T;
        }
        
        return null;
    }

    public static async Task<List<T>> FetchMultipleDocuments<T>(string collection, string sort, int limit) where T : class
    {
        List<T> results = new List<T>();
        
        Query query = _firestore.Collection(collection).OrderBy(sort).Limit(limit);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        
        bool hasArtworks = false, hasArtists = false, hasExhibitions = false;

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
                    data = await ReadArtworkDocument(document);
                    results.Add(data as T);
                    hasArtworks = true;
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
                    data = await ReadArtistDocument(document);
                    results.Add(data as T);
                    hasArtists = true;
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
                    data = await ReadExhibitionDocument(document);
                    results.Add(data as T);
                    hasExhibitions = true;
                }
            }
        }
        
        if (hasArtworks) AppCache.SaveArtworksCache();
        if (hasExhibitions) AppCache.SaveExhibitionsCache();
        if (hasArtists) AppCache.SaveArtistsCache();

        return results;
    }

    public static ExhibitionData GetConnectedExhibition(ArtworkData artwork)
    {
        return (from exhibition in Exhibitions from artworkReference in exhibition.artwork_references where artworkReference.Id == artwork.id select exhibition).FirstOrDefault();
    }
    
    #endregion

    #region Downloading Media

    public static async Task<string> DownloadMedia(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("Tried loading an empty string...");
            return string.Empty;
        }

        try
        {
            // Ensure the media folder exists
            if (!Directory.Exists(AppCache.MediaFolder))
            {
                Directory.CreateDirectory(AppCache.MediaFolder);
            }

            // Determine file name from the URL
            Uri uri = new Uri(path);
            string fileName = Path.GetFileName(uri.LocalPath);

            // If we didn't get a valid file name, default to a GUID-based name with a .png extension.
            if (string.IsNullOrEmpty(fileName) || !fileName.Contains("."))
            {
                fileName = Guid.NewGuid().ToString() + ".png";
            }

            string localPath = Path.Combine(AppCache.MediaFolder, fileName);

            // Optionally check if the file already exists
            if (File.Exists(localPath))
            {
                Debug.Log($"File already exists at {localPath}");
                return localPath;
            }

            // Download the media data using HttpClient
            using HttpClient client = new HttpClient();
            byte[] data = await client.GetByteArrayAsync(path);
            await File.WriteAllBytesAsync(localPath, data);

            Debug.Log("Downloaded: " + fileName);

            return localPath;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error downloading media: {e.Message}");
        }

        return string.Empty;
    }

    #endregion
}
