using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AlmostEngine.Screenshot;
using Firebase;
using Firebase.Firestore;
using Firebase.Messaging;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    
    // Collection Sizes
    public static long ArtworkCollectionSize { get; private set; } = -1;
    public static long ExhibitionCollectionSize { get; private set; } = -1;
    public static long ArtistCollectionSize { get; private set; } = -1;
    
    // Bool states
    public static bool ArtworkCollectionFull => Artworks.Count >= ArtworkCollectionSize;
    public static bool ExhibitionCollectionFull => Exhibitions.Count >= ExhibitionCollectionSize;
    public static bool ArtistCollectionFull => Artists.Count >= ArtistCollectionSize;
    
    private const int MAX_NOT_IN = 10;
    
    // Start up
    public static event Action<string> OnStartUpEventProcessed;
    
    public static bool SetupComplete { get; private set; } = false;
    private int connectAttempts = 0;
    private int maxConnectAttempts = 2;

    public static bool OfflineMode { get; private set; } = false;
    
    // Serialized Properties
    [Header("Settings")]
    [SerializeField] private bool loadArtworksOnStartup = false;
    [SerializeField] private bool loadExhibitionsOnStartup = false;
    [SerializeField] private bool loadArtistsOnStartup = false;
    [Space]
    [SerializeField] private bool downloadArtworkImagesOnStartup = false;
    [SerializeField] private bool downloadExhibitionImagesOnStartup = false;
    [SerializeField] private bool downloadArtistImagesOnStartup = false;
    [SerializeField] private bool downloadArtworkContentOnStartup = false;
    [Space]
    [SerializeField] private bool downloadHomeScreenContent = false;
    [SerializeField] private bool createLocalGallery = true;
    [Space]
    [SerializeField] private bool ignoreNotificationSubscription = false;
    [Space]
    [SerializeField] private bool startInOfflineMode = false;
    
    [Header("Setup Dependencies")]
    [SerializeField] private ScreenshotManager screenshotManager;
    
    #region Setup
    private void Awake()
    {
        if (_firestore != null) return;
        InitializeFirebase();
    }

    private void OnEnable()
    {
        if (Initialized && !ignoreNotificationSubscription)
        {
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        }
    }

    private void OnDisable()
    {
        if (Initialized && !ignoreNotificationSubscription)
        {
            Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
        }
    }

    /// <summary>
    /// Initializes Firebase and Firestore.
    /// </summary>
    private async void InitializeFirebase()
    {
        while (!Initialized)
        {
            if (!startInOfflineMode)
            {
                try
                {
                    OnStartUpEventProcessed?.Invoke("Trying to initialize Database...");
                    DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                    if (dependencyStatus == DependencyStatus.Available)
                    {
                        _firestore = FirebaseFirestore.DefaultInstance;
                        Debug.Log("Firebase initialized successfully.");

                        if (_firestore.Settings == null)
                        {
                            Debug.LogWarning("Firestore settings are empty");
                        }

                        await GetCollectionCountsAsync();
                        await AppCache.LoadLocalCaches();
                        await CheckForCacheUpdates();

                        OfflineMode = false;
                        Initialized = true;
                        OnFirestoreInitialized?.Invoke();
                        OnStartUpEventProcessed?.Invoke("Firebase initialized.");
                        ProcessSetup();
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
                
            }

            if (!Initialized)
            {
                connectAttempts++;
                if (connectAttempts > maxConnectAttempts)
                {
                    OnStartUpEventProcessed?.Invoke("Starting in offline mode...");
                    await AppCache.LoadLocalCaches();
                    SetupComplete = true;
                    Initialized = true;
                    OfflineMode = true;
                    OnFirestoreInitialized?.Invoke();
                    return;
                }
                
                OnStartUpEventProcessed?.Invoke("Failed to initialize database... retrying...");
                Debug.LogWarning("Retrying Firebase initialization in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Message from: " + e.Message);
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs e)
    {
        Debug.Log("Token received: " + e.Token);
    }

    private async Task ProcessSetup(bool reload = false)
    {
        if (loadArtistsOnStartup)
        {
            OnStartUpEventProcessed?.Invoke("Loading artist data...");
            await LoadRemainingArtists();
        }

        if (loadArtworksOnStartup)
        {
            OnStartUpEventProcessed?.Invoke("Loading artwork data...");
            await LoadRemainingArtworks(() => { OnStartUpEventProcessed?.Invoke("Loaded Artworks"); });
        }

        if (loadExhibitionsOnStartup)
        {
            OnStartUpEventProcessed?.Invoke("Loading exhibition data...");
            await LoadRemainingExhibitions(false);
            OnStartUpEventProcessed?.Invoke("Connecting exhibition data...");

            // Connect artworks
            foreach (var exhibition in Exhibitions)
            {
                await FillExhibitionArtworkData(exhibition, false);
            }

            // Connect artists
            foreach (var exhibition in Exhibitions)
            {
                foreach (var artist in Artists.Where(artist => exhibition.artist_references.Any(aref => aref.Id == artist.id)).Where(artist => !exhibition.artists.Contains(artist)))
                {
                    exhibition.artists.Add(artist);
                }
            }

            await AppCache.SaveExhibitionsCache();
        }

        if (createLocalGallery && screenshotManager != null)
        {
            string path = screenshotManager.GetExportPath();
            OnStartUpEventProcessed?.Invoke($"Loading local storage...");
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.png");
                List<Sprite> sprites = new List<Sprite>();

                foreach (var file in files)
                {
                    byte[] fileData = await File.ReadAllBytesAsync(file);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(fileData);
                    
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    sprite.name = Path.GetFileName(file);

                    AppCache.LocalGallery.Add(file, sprite);
        
                    sprites.Add(sprite);
                }
            }

            ARGalleryPage.StoragePath = path;
        }

        if (downloadHomeScreenContent)
        {
            OnStartUpEventProcessed?.Invoke($"Downloading new exhibitions...");
            var newestExhibition = Exhibitions.OrderByDescending(e => e.creation_date_time).FirstOrDefault();
            await newestExhibition?.GetAllImages()!;

            OnStartUpEventProcessed?.Invoke($"Downloading new artworks...");
            var newestArtworks = Artworks.OrderByDescending(a => a.creation_date_time).Take(2).ToList();
            foreach (var artwork in newestArtworks)
            {
                await artwork.GetAllImages();
            }
        }

        if (downloadArtworkImagesOnStartup)
        {
            int curr = 0;
            int total = Artworks.Sum(artwork => artwork.artwork_image_references.Count);

            foreach (var artwork in Artworks)
            {
                curr++;
                OnStartUpEventProcessed?.Invoke($"Download artwork images...  [{(curr / total * 100):F2}%]");
                await artwork.GetAllImages();
            }
        }
        
        if (downloadExhibitionImagesOnStartup)
        {
            int curr = 0;
            int total = Exhibitions.Sum(exhibition => exhibition.exhibition_image_references.Count);

            foreach (var exhibition in Exhibitions)
            {
                curr++;
                OnStartUpEventProcessed?.Invoke($"Download exhibition images... [{(curr / total * 100):F2}%]");
                await exhibition.GetAllImages();
            }
        }
        
        if (downloadArtistImagesOnStartup)
        {
            int curr = 0;
            int total = Artists.Count;

            foreach (var artist in Artists)
            {
                curr++;
                OnStartUpEventProcessed?.Invoke($"Download artist images... [{(curr / total * 100):F2}%]");
                await artist.GetIcon();
            }
        }
        
        OnStartUpEventProcessed?.Invoke(string.Empty);
        SetupComplete = true;
    }

    private async Task CheckForCacheUpdates()
    {
        Debug.Log("Checking for cache updates...");
        
        string lastFetchTimeStr = PlayerPrefs.GetString("lastFetchTime", DateTime.UtcNow.ToString("o"));
        
        if (!DateTime.TryParse(lastFetchTimeStr, out DateTime lastFetchTime))
        {
            lastFetchTime = DateTime.UtcNow;
        }
        
        Timestamp lastFetchTimestamp = Timestamp.FromDateTime(lastFetchTime);

        Query artworksQuery = _firestore.Collection("artworks").WhereGreaterThan("update_time", lastFetchTimestamp);
        Query exhibitionsQuery = _firestore.Collection("exhibitions").WhereGreaterThan("update_time", lastFetchTimestamp);
        Query artistsQuery = _firestore.Collection("artists").WhereGreaterThan("update_time", lastFetchTimestamp);

        int updated = 0;
        
        try
        {
            // Process artists
            OnStartUpEventProcessed?.Invoke("Updating artist cache...");
            QuerySnapshot artistsSnapshot = await artistsQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot doc in artistsSnapshot.Documents)
            {
                updated++;
                Debug.Log("Artist scheduled for update: " + doc.Id);
                
                if (ArtistsMap.TryGetValue(doc.Id, out var artistStored))
                {
                    // Update existing entry
                    ArtistData artist = doc.ConvertTo<ArtistData>();
                    artistStored.title = artist.title;
                    artistStored.description = artist.description;
                    artistStored.location = artist.location;
                    artistStored.link = artist.link;
                    artistStored.icon = artist.icon;
                    
                    artistStored.creation_time = artist.creation_time;
                    artistStored.update_time = artist.update_time;
                    
                    artistStored.creation_date_time = artist.creation_time.ToDateTime();
                    artistStored.update_date_time = artist.update_time.ToDateTime();
                    
                    await AppCache.SaveArtistsCache();
                }
                else
                {
                    await ReadArtistDocument(doc);
                    await AppCache.SaveArtistsCache();
                }
            }
            
            // Process artworks
            OnStartUpEventProcessed?.Invoke("Updating artwork cache...");
            QuerySnapshot artworksSnapshot = await artworksQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot doc in artworksSnapshot.Documents)
            {
                updated++;
                Debug.Log("Artwork scheduled for update: " + doc.Id);

                if (ArtworksMap.TryGetValue(doc.Id, out var artworkStored))
                {
                    // Update existing entry
                    ArtworkData artwork = doc.ConvertTo<ArtworkData>();
                    artworkStored.title = artwork.title;
                    artworkStored.description = artwork.description;
                    if (artworkStored.artist_references.Count > 0) artworkStored.artist_references = new List<DocumentReference>(artwork.artist_references);
                    artworkStored.artists.Clear();
                    foreach (var documentReference in artwork.artist_references)
                    {
                        artworkStored.artists.Add(await ReadArtistDocumentReference(documentReference));
                    }
                    artworkStored.year = artwork.year;
                    artworkStored.location = artwork.location;
                    artworkStored.published = artwork.published;
                    
                    artworkStored.artwork_image_references = new List<string>(artwork.artwork_image_references);
                    
                    artworkStored.latitude = artwork.latitude;
                    artworkStored.longitude = artwork.longitude;
                    artworkStored.max_distance = artwork.max_distance;
                    artworkStored.creation_time = artwork.creation_time;
                    artworkStored.update_time = artwork.update_time;
                    
                    artworkStored.creation_date_time = artwork.creation_time.ToDateTime();
                    artworkStored.update_date_time = artwork.update_time.ToDateTime();

                    artworkStored.content_list = new List<MediaContentData>(artwork.content_list);

                    foreach (var content in artwork.content_list)
                    {
                        var uri = new Uri(content.media_content);
                        string encodedPath = uri.AbsolutePath;
                        string decodedPath = Uri.UnescapeDataString(encodedPath);
                        string fileName = Path.GetFileName(decodedPath);
                        string localPath = Path.Combine(AppCache.ContentFolder, fileName);
            
                        // if the file does not exist locally, download it
                        if (File.Exists(localPath))
                        {
                            Debug.Log("Removing stored content: " + fileName);
                            File.Delete(localPath);
                        }
                    }
                    
                    artworkStored.preset = artwork.preset;
                    artworkStored.alt_scene = artwork.alt_scene;
                    
                    await AppCache.SaveArtworksCache();
                }
                else
                {
                    await ReadArtworkDocument(doc);
                    await AppCache.SaveArtworksCache();
                }
            }

            // Process exhibitions
            OnStartUpEventProcessed?.Invoke("Updating exhibition cache...");
            QuerySnapshot exhibitionsSnapshot = await exhibitionsQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot doc in exhibitionsSnapshot.Documents)
            {
                updated++;
                Debug.Log("Exhibition scheduled for update: " + doc.Id);
                
                if (ExhibitionsMap.TryGetValue(doc.Id, out var exhibitionStored))
                {
                    // Update existing entry
                    ExhibitionData exhibition = doc.ConvertTo<ExhibitionData>();
                    exhibitionStored.title = exhibition.title;
                    exhibitionStored.description = exhibition.description;
                    
                    if (exhibitionStored.artist_references.Count > 0) exhibitionStored.artist_references = new List<DocumentReference>(exhibition.artist_references);
                    exhibitionStored.artists.Clear();
                    foreach (var documentReference in exhibition.artist_references)
                    {
                        exhibitionStored.artists.Add(await ReadArtistDocumentReference(documentReference));
                    }
                    
                    exhibitionStored.artwork_ids.Clear();
                    foreach (var documentReference in exhibition.artwork_references)
                    {
                        exhibitionStored.artwork_ids.Add(documentReference.Id);
                        exhibitionStored.artworks.Add(await ReadArtworkDocument(await documentReference.GetSnapshotAsync()));
                    }
                    
                    exhibitionStored.year = exhibition.year;
                    exhibitionStored.location = exhibition.location;
                    exhibitionStored.published = exhibition.published;
                    exhibitionStored.color = exhibition.color;
                    exhibitionStored.exhibition_image_references = new List<string>(exhibition.exhibition_image_references);
                    
                    //exhibitionStored.publish_date = exhibition.publish_date;
                    exhibitionStored.creation_time = exhibition.creation_time;
                    exhibitionStored.update_time = exhibition.update_time;
                    
                    exhibitionStored.creation_date_time = exhibition.creation_time.ToDateTime();
                    exhibitionStored.update_date_time = exhibition.update_time.ToDateTime();
                    
                    await AppCache.SaveExhibitionsCache();
                }
                else
                {
                    await ReadExhibitionDocument(doc);
                    await AppCache.SaveExhibitionsCache();
                }

            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error fetching updates: " + ex.Message);
        }
        
        
        if (updated > 0) Debug.Log($"Updated [{updated}] local cached data");
        else Debug.Log("Local cache was already up to date");
        
        // Finally, update the last fetch time to the current time (in ISO8601 format).
        string newFetchTime = DateTime.UtcNow.ToString("o");
        PlayerPrefs.SetString("lastFetchTime", newFetchTime);
        PlayerPrefs.Save();
    }

    public async void AttemptToReconnect(Slider slider)
    {
        if (!OfflineMode) return;

        slider.maxValue = 100;

        Debug.Log("Trying to reconnect to servers");
        Initialized = false;

        while (!Initialized)
        {
            if (!startInOfflineMode)
            {
                try
                {
                    slider.value = 50;
                    DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                    if (dependencyStatus == DependencyStatus.Available)
                    {
                        _firestore = FirebaseFirestore.DefaultInstance;
                        Debug.Log("Firebase initialized successfully.");

                        if (_firestore.Settings == null)
                        {
                            Debug.LogWarning("Firestore settings are empty");
                        }

                        await GetCollectionCountsAsync();
                        await AppCache.LoadLocalCaches();
                        await CheckForCacheUpdates();

                        slider.value = 100;
                        
                        OfflineMode = false;
                        Initialized = true;
                        await ProcessSetup(true);
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

            }
        }

        if (!Initialized)
        {
            slider.value = 100;
            OfflineMode = true;
        }
    }

    #endregion
    
    #region Add Data

    public static void AddArtworkData(ArtworkData data)
    {
        if (ArtworksMap.ContainsKey(data.id))
        {
            Debug.LogWarning("Tried to add an exhibition with a document id already stored in the artwork map: " + data.title);
            return;
        }

        if (!AppSettings.DeveloperMode)
        {
            if (!data.published)
            {
                ArtworkCollectionSize--;
                Debug.LogWarning($"Removed an unpublished artwork from loading queue [{data.title}] because publishing was: {data.published}");
                return;
            }
        }
        
        Artworks.Add(data);
        ArtworksMap.TryAdd(data.id, data);
    }

    public static void AddArtistData(ArtistData data)
    {
        if (ArtistsMap.ContainsKey(data.id))
        {
            Debug.LogWarning("Tried to add an exhibition with a document id already stored in the artist map: " + data.title);
            return;
        }
        
        /*if (!AppSettings.DeveloperMode)
        {
            if (!data.published)
            {
                ArtistCollectionSize--;
                Debug.LogWarning("Removed an unpublished artwork from loading queue: " + data.title);
                return;
            }
        }*/
        
        Artists.Add(data);
        ArtistsMap.TryAdd(data.id, data);
    }

    public static void AddExhibitionData(ExhibitionData data)
    {
        if (ExhibitionsMap.ContainsKey(data.id))
        {
            Debug.LogWarning("Tried to add an exhibition with a document id already stored in the exhibition map: " + data.title);
            return;
        }
        
        if (!AppSettings.DeveloperMode)
        {
            if (!data.published)
            {
                ExhibitionCollectionSize--;
                Debug.LogWarning("Removed an unpublished exhibition from loading queue: " + data.title);
                return;
            }
        }
        
        Exhibitions.Add(data);
        ExhibitionsMap.TryAdd(data.id, data);
    }
    
    #endregion
    
    #region Convert Document to Data
    
    private static async Task<ExhibitionData> ReadExhibitionDocument(DocumentSnapshot document)
    {
        if (ExhibitionsMap.TryGetValue(document.Id, out var exhibitionDocument))
        {
            Debug.LogWarning("Reading an already existing [Exhibition] document...");
            return exhibitionDocument;    
        }

        Debug.Log($"exhibiton trying to convert: " + document.Id);
        ExhibitionData exhibition = document.ConvertTo<ExhibitionData>();
        exhibition.id = document.Id;
        //await LoadArtworkImages(exhibition);
        exhibition.creation_date_time = exhibition.creation_time.ToDateTime();
        exhibition.update_date_time = exhibition.update_time.ToDateTime();
        AddExhibitionData(exhibition);
        Debug.Log($"[Firebase] Loaded Exhibition: {exhibition.title} [{document.Id}]");
        
        return exhibition;
    }
    
    private static async Task<ArtworkData> ReadArtworkDocument(DocumentSnapshot document)
    {
        if (ArtworksMap.TryGetValue(document.Id, out var artworkDocument))
        {
            Debug.LogWarning("Reading an already existing [Artwork] document...");
            return artworkDocument;    
        }
        
        ArtworkData artwork = document.ConvertTo<ArtworkData>();
        artwork.id = document.Id;
        artwork.creation_date_time = artwork.creation_time.ToDateTime();
        artwork.update_date_time = artwork.update_time.ToDateTime();
        foreach (var documentReference in artwork.artist_references)
        {
            artwork.artists.Add(await ReadArtistDocumentReference(documentReference));
        }
        AddArtworkData(artwork);
        Debug.Log($"[Firebase] Loaded Artwork: {artwork.title} [{document.Id}]");
        
        return artwork;
    }
    
    private static async Task<ArtistData> ReadArtistDocument(DocumentSnapshot document)
    {
        if (ArtistsMap.TryGetValue(document.Id, out var artistDocument))
        {
            Debug.LogWarning("Reading an already existing [Artist] document...");
            return artistDocument;    
        }
        
        ArtistData artist = document.ConvertTo<ArtistData>();
        artist.id = document.Id;
        artist.creation_date_time = artist.creation_time.ToDateTime();
        artist.update_date_time = artist.update_time.ToDateTime();
        AddArtistData(artist);
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
        foreach (var exhibition in Exhibitions.Where(exhibition => exhibition.artworks.Any(artwork => artwork.id == artwork_id) || exhibition.artwork_ids.Any(id => artwork_id == id) || exhibition.artwork_references.Any(aRef => artwork_id == aRef.Id)))
        {
            return exhibition;
        }
        
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
            var exhibition = await ReadExhibitionDocument(exhibitionDoc);
            await FillExhibitionArtworkData(exhibition);
            return exhibition;
        }

        // If no exhibition is found, return null.
        return null;
    }
    
    #endregion

    #region Helper Methods for Fetching Single Items
    
    private async Task GetCollectionCountsAsync()
    {
        if (ArtworkCollectionSize != -1 && ExhibitionCollectionSize != -1 && ArtistCollectionSize != -1) return; // already found the sizes
        
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
    
    public static async Task LoadRemainingArtists()
    {
        if (ArtistCollectionFull)
        {
            Debug.Log("Trying to load all artists... but all artists were already loaded");
            return;
        }
        
        Debug.Log("Trying to load all artists");

        try
        {
            CollectionReference artistsRef = _firestore.Collection("artists");
            // Build list of loaded artist IDs from your map.
            List<string> loadedIds = new List<string>(ArtistsMap.Keys);
            List<Task<QuerySnapshot>> tasks = new List<Task<QuerySnapshot>>();
            List<ArtistData> newArtists = new List<ArtistData>();

            if (loadedIds.Count == 0)
            {
                // If nothing has been loaded yet, load the whole collection.
                QuerySnapshot snapshot = await artistsRef.GetSnapshotAsync();
                foreach (var document in snapshot.Documents)
                {
                    OnStartUpEventProcessed?.Invoke($"Loading artist data... {Artists.Count + 1} / {ArtistCollectionSize}");
                    string id = document.Id;
                    if (ArtistsMap.ContainsKey(id))
                        continue;

                    try
                    {
                        ArtistData artist = await ReadArtistDocument(document);
                        ArtistsMap.TryAdd(id, artist);
                        newArtists.Add(artist);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing document {id}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            else
            {
                // Firestore allows at most 10 elements in WhereNotIn, so batch accordingly.
                for (int i = 0; i < loadedIds.Count; i += 10)
                {
                    List<string> batch = loadedIds.GetRange(i, Mathf.Min(10, loadedIds.Count - i));
                    Query query = artistsRef.WhereNotIn(FieldPath.DocumentId, batch);
                    tasks.Add(query.GetSnapshotAsync());
                    OnStartUpEventProcessed?.Invoke($"Loading artist data... {Artists.Count + 1} / {ArtistCollectionSize}");
                }
                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    QuerySnapshot snapshot = task.Result;
                    foreach (var document in snapshot.Documents)
                    {
                        string id = document.Id;
                        if (ArtistsMap.ContainsKey(id))
                            continue;

                        try
                        {
                            ArtistData artist = await ReadArtistDocument(document);
                            ArtistsMap.TryAdd(id, artist);
                            newArtists.Add(artist);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error processing document {id}: {ex.Message}\n{ex.StackTrace}");
                        }
                    }
                }
            }

            Debug.Log($"Loaded {newArtists.Count} new artists.");
            if (newArtists.Count > 0) await AppCache.SaveArtistsCache();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load artists: {e.Message}\n{e.StackTrace}");
        }
    }
    
    public static async Task LoadRemainingArtworks(Action onComplete)
    {
        if (ArtworkCollectionFull)
        {
            Debug.Log("Trying to load all artworks... but all artworks were already loaded");
            onComplete?.Invoke();
            return;
        }
        
        Debug.Log("Trying to load all artworks");
        
        try
        {
            CollectionReference artworksRef = _firestore.Collection("artworks");
            // Build list of loaded artwork IDs.
            List<string> loadedIds = new List<string>(ArtworksMap.Keys);
            List<Task<QuerySnapshot>> tasks = new List<Task<QuerySnapshot>>();
            List<ArtworkData> newArtworks = new List<ArtworkData>();

            if (loadedIds.Count == 0)
            {
                // Load the full collection if none are loaded.
                QuerySnapshot snapshot = await artworksRef.GetSnapshotAsync();
                foreach (var document in snapshot.Documents)
                {
                    OnStartUpEventProcessed?.Invoke($"Loading artwork data... {Artworks.Count + 1} / {ArtworkCollectionSize}");
                    string id = document.Id;
                    if (ArtworksMap.ContainsKey(id))
                        continue;

                    try
                    {
                        ArtworkData artwork = await ReadArtworkDocument(document);
                        ArtworksMap.TryAdd(id, artwork);
                        newArtworks.Add(artwork);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing document {id}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            else
            {
                // Query in batches to avoid exceeding the WhereNotIn limit.
                for (int i = 0; i < loadedIds.Count; i += 10)
                {
                    List<string> batch = loadedIds.GetRange(i, Mathf.Min(10, loadedIds.Count - i));
                    Query query = artworksRef.WhereNotIn(FieldPath.DocumentId, batch);
                    tasks.Add(query.GetSnapshotAsync());
                    OnStartUpEventProcessed?.Invoke($"Loading artwork data... {Artworks.Count + 1} / {ArtworkCollectionSize}");
                }
                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    QuerySnapshot snapshot = task.Result;
                    foreach (var document in snapshot.Documents)
                    {
                        string id = document.Id;
                        if (ArtworksMap.ContainsKey(id))
                            continue;

                        try
                        {
                            ArtworkData artwork = await ReadArtworkDocument(document);
                            ArtworksMap.TryAdd(id, artwork);
                            newArtworks.Add(artwork);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error processing document {id}: {ex.Message}\n{ex.StackTrace}");
                        }
                    }
                }
            }

            Debug.Log($"Loaded {newArtworks.Count} new artworks.");
            if (newArtworks.Count > 0) AppCache.SaveArtworksCache();
            onComplete?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load artworks: {e.Message}\n{e.StackTrace}");
        }
    }

    public static async Task LoadRemainingExhibitions(bool save = true)
    {
        if (ExhibitionCollectionFull)
        {
            Debug.Log("Trying to load remaining exhibitions... but all exhibitions were already loaded");
            return;
        }
        
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
                    OnStartUpEventProcessed?.Invoke($"Loading exhibition data... {Exhibitions.Count + 1} / {ExhibitionCollectionSize}");
                }

                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    ProcessExhibitions(task.Result, newExhibitions);
                }
            }

            if (newExhibitions.Count > 0 && save) AppCache.SaveExhibitionsCache();

            Debug.Log($"Loaded {newExhibitions.Count} new newExhibitions.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load remaining artworks: {e.Message}\n{e.StackTrace}");
        }
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
        
        await AppCache.SaveExhibitionsCache();
    }

    public static async Task FillExhibitionArtworkData(ExhibitionData exhibition, bool save = true)
    {
        exhibition.artworks = new List<ArtworkData>();

        bool requiresSave = false;

        try
        {
            if (exhibition.artwork_references.Count > 0)
            {
                foreach (var artworkReference in exhibition.artwork_references)
                {
                    if (ArtworksMap.TryGetValue(artworkReference.Id, out var value)) exhibition.artworks.Add(value);
                    else
                    {
                        exhibition.artworks.Add(await ReadArtworkDocument(await artworkReference.GetSnapshotAsync()));
                        requiresSave = true;
                    }
                }
            }
            else if (exhibition.artwork_ids.Count > 0)
            {
                foreach (var id in exhibition.artwork_ids)
                {
                    if (ArtworksMap.TryGetValue(id, out var value)) exhibition.artworks.Add(value);
                    else
                    {
                        Debug.LogWarning("Resorted to loading from artwork id, but id was not in the artwork map");
                    }
                }
            }


            if (save && requiresSave)
            {
                await AppCache.SaveArtworksCache();
                await AppCache.SaveExhibitionsCache();
            }
        }
        catch (Exception e)
        {
            Debug.Log("Failed to add remaining artworks: " + e);
        }
    }

    #endregion

    #region Fetching

    /// <summary>
    /// Fetches a limited number of documents from a specified collection with pagination support.
    /// </summary>
    /// <typeparam name="T">The type of data to fetch (ArtworkData, ArtistData, ExhibitionData).</typeparam>
    /// <param name="limit">The maximum number of documents to fetch.</param>
    public static async Task<List<T>> FetchDocuments<T>(int limit, string sorting = "creation_time") where T : FirebaseData
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

            Debug.Log("[Firebase] Fetching new documents");

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

            Debug.Log($"[Firebase] Fetched {allFetchedDocuments.Count} new documents (single query)");

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

                Debug.Log($"[Firebase] Fetched {fetchedDocuments.Count} new documents");
                
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

    public static ArtworkData GetArtworkByID(string id)
    {
        return ArtworksMap.GetValueOrDefault(id);
    }
    
    #endregion

    #region Downloading Media

    public static async Task<(string localPath, bool downloaded)> DownloadMedia(string storagePath, string path, ARDownloadBar downloadBar, int index = 0)
    {
        Debug.Log("Attempting to download");
        
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("Tried loading an empty string...");
            if (downloadBar) downloadBar.FailedDownload();
            return (string.Empty, false);
        }

        try
        {
            // Ensure the media folder exists
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            // Determine file name from the URL
            Uri uri = new Uri(path);
            string fileName = Path.GetFileName(uri.LocalPath);

            // If we didn't get a valid file name, default to a GUID-based name with a .png extension.
            if (string.IsNullOrEmpty(fileName) || !fileName.Contains("."))
            {
                fileName = Guid.NewGuid().ToString() + ".png";
            }

            string localPath = Path.Combine(storagePath, fileName);

            // Optionally check if the file already exists
            if (File.Exists(localPath))
            {
                Debug.Log($"File already exists at {localPath}");
                return (localPath, false);
            }

            // Create the UnityWebRequest with a DownloadHandlerFile to save the file directly.
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                request.downloadHandler = new DownloadHandlerFile(localPath);
                
                // Begin the request
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                // While the download is in progress, update the TMP_Text with the progress percentage.
                while (!operation.isDone)
                {
                    float progressValue = request.downloadProgress; // value between 0 and 1
                    if (downloadBar)
                    {
                        downloadBar.UpdateProgress(index, (progressValue * 100f) / 2);
                    }
                    
                    // Yield control until the next frame so the UI can update.
                    await Task.Yield();
                }

                // Check if the download completed successfully.
                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (downloadBar)
                    {
                        downloadBar.UpdateProgress(index, 50);
                    }
                    Debug.Log("Downloaded: " + fileName);
                    return (localPath, true);
                }
                else
                {
                    Debug.LogError("Error downloading media: " + request.error);
                    if (downloadBar)
                    {
                        downloadBar.FailedDownload();
                    }
                    return (string.Empty, false);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error downloading media: {e.Message} | tried saving to local path: {storagePath}");
            if (downloadBar) downloadBar.FailedDownload();
        }

        return (string.Empty, false);
    }
    
    /// <summary>
    /// Download an image, fully decode it, and save as PNG.
    /// Guarantees you never get partial image bytes on disk.
    /// </summary>
    public static async Task<(string localPath, bool downloaded)> DownloadImage(
        string storagePath,
        string url,
        ARDownloadBar downloadBar,
        int index = 0)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("Empty image URL");
            downloadBar?.FailedDownload();
            return (string.Empty, false);
        }

        Directory.CreateDirectory(storagePath);

        Uri uri = new Uri(url);
        string fileName = Path.GetFileName(uri.LocalPath);
        if (string.IsNullOrEmpty(fileName) || !fileName.Contains("."))
            fileName = Guid.NewGuid().ToString() + ".png";

        string localPath = Path.Combine(storagePath, fileName);
        string tmpPath   = localPath + ".tmp";

        if (File.Exists(localPath))
            return (localPath, false);

        try
        {
            using (var req = UnityWebRequestTexture.GetTexture(url))
            {
                var op = req.SendWebRequest();
                while (!op.isDone)
                {
                    downloadBar?.UpdateProgress(index, (req.downloadProgress * 100f) / 2f);
                    await Task.Yield();
                }

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Image download failed: {req.error}");
                    downloadBar?.FailedDownload();
                    return (string.Empty, false);
                }

                try
                {
                    var tex = ((DownloadHandlerTexture)req.downloadHandler).texture;
                    byte[] pngBytes = tex.EncodeToPNG();

                    // Asynchronously write PNG bytes to disk:
                    await File.WriteAllBytesAsync(tmpPath, pngBytes);

                    // Atomically replace
                    if (File.Exists(localPath)) File.Delete(localPath);
                    File.Move(tmpPath, localPath);

                    downloadBar?.UpdateProgress(index, 50);
                    Debug.Log($"Image downloaded: {fileName}");
                    return (localPath, true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error writing image to disk: {e}");
                    downloadBar?.FailedDownload();
                    if (File.Exists(tmpPath)) File.Delete(tmpPath);
                    return (string.Empty, false);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error downloading image: {e}");
            downloadBar?.FailedDownload();
            if (File.Exists(tmpPath)) File.Delete(tmpPath);
            return (string.Empty, false);
        }
    }
    
    
    #endregion
}
