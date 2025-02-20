using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class AppCache
{
    private static readonly string artistsCachePath = Path.Combine(Application.persistentDataPath, "artistsCache.json");
    private static readonly string artworksCachePath = Path.Combine(Application.persistentDataPath, "artworksCache.json");
    private static readonly string exhibitionsCachePath = Path.Combine(Application.persistentDataPath, "exhibitionsCache.json");
    
    // Path for storing media download info
    private static readonly string artworksMediaCachePath = Path.Combine(Application.persistentDataPath, "media", "artworksCache.json");
    
    // Folder for the actual downloaded files
    private static readonly string mediaFolder = Path.Combine(Application.persistentDataPath, "media");

    // In-memory cache for downloaded artwork media info
    public static List<ArtworkDownloadHolder> ArtworkDownloads = new List<ArtworkDownloadHolder>();

    // Ensures that the directory for a given file path exists.
    private static void EnsureDirectoryExists(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Debug.Log("Created new directory: " + directory);
        }
    }

    public static void LoadLocalCaches()
    {
        Debug.Log("Loading local caches...");
        EnsureDirectoryExists(artistsCachePath);
        EnsureDirectoryExists(artworksCachePath);
        EnsureDirectoryExists(exhibitionsCachePath);
        EnsureDirectoryExists(artworksMediaCachePath);
        EnsureDirectoryExists(mediaFolder);

        // Load Artists Cache
        if (File.Exists(artistsCachePath))
        {
            try 
            {
                string json = File.ReadAllText(artistsCachePath);
                ArtistDataHolderListWrapper wrapper = JsonUtility.FromJson<ArtistDataHolderListWrapper>(json);
                if (wrapper != null && wrapper.artists != null)
                {
                    foreach (var artistHolder in wrapper.artists)
                    {
                        ArtistData artist = ArtistDataHolder.ToArtistData(artistHolder);
                        FirebaseLoader.AddArtistData(artist);
                    }
                    Debug.Log($"Loaded {wrapper.artists.Count} artists from local cache.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load artists cache: " + e.Message);
            }
        }
        else Debug.Log("Artist Cache does not exist");

        // Load Artworks Cache
        if (File.Exists(artworksCachePath))
        {
            try 
            {
                string json = File.ReadAllText(artworksCachePath);
                ArtworkDataHolderListWrapper wrapper = JsonUtility.FromJson<ArtworkDataHolderListWrapper>(json);
                if (wrapper != null && wrapper.artworks != null)
                {
                    foreach (var artworkHolder in wrapper.artworks)
                    {
                        ArtworkData artwork = ArtworkDataHolder.FromHolder(artworkHolder);
                        
                        ArtworkDownloadHolder downloadHolder = ArtworkDownloads.FirstOrDefault(x => x.artwork_id == artwork.artwork_id);
                        if (downloadHolder is { mediaPaths: { Count: > 0 } })
                        {
                            List<Sprite> loadedSprites = new List<Sprite>();
                            foreach (string path in downloadHolder.mediaPaths)
                            {
                                if (File.Exists(path))
                                {
                                    byte[] imageData = File.ReadAllBytes(path);
                                    Texture2D tex = new Texture2D(2, 2);
                                    if (tex.LoadImage(imageData))
                                    {
                                        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                                        loadedSprites.Add(sprite);
                                    }
                                }
                            }
                            artwork.images = loadedSprites;
                        }
                        
                        FirebaseLoader.AddArtworkData(artwork);
                    }
                    Debug.Log($"Loaded {wrapper.artworks.Count} artworks from local cache.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load artworks cache: " + e.Message);
            }
        }
        else Debug.Log("Artwork Cache does not exist");

        // Load Exhibitions Cache
        if (File.Exists(exhibitionsCachePath))
        {
            try 
            {
                string json = File.ReadAllText(exhibitionsCachePath);
                ExhibitionDataHolderListWrapper wrapper = JsonUtility.FromJson<ExhibitionDataHolderListWrapper>(json);
                if (wrapper != null && wrapper.exhibitions != null)
                {
                    foreach (var exhibitionHolder in wrapper.exhibitions)
                    {
                        ExhibitionData exhibition = ExhibitionDataHolder.FromHolder(exhibitionHolder);
                        FirebaseLoader.AddExhibitionData(exhibition);
                    }
                    Debug.Log($"Loaded {wrapper.exhibitions.Count} exhibitions from local cache.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load exhibitions cache: " + e.Message);
            }
        }
        else Debug.Log("Exhibition Cache does not exist");

        // Load Artwork Downloads info
        LoadArtworkDownloads();
    }
    
    public static void SaveArtistsCache()
    {
        try 
        {
            EnsureDirectoryExists(artistsCachePath);
            List<ArtistDataHolder> holders = new List<ArtistDataHolder>();
            foreach(var artist in FirebaseLoader.Artists)
            {
                holders.Add(ArtistDataHolder.FromArtistData(artist));
            }
            ArtistDataHolderListWrapper wrapper = new ArtistDataHolderListWrapper { artists = holders };
            string json = JsonUtility.ToJson(wrapper);
            File.WriteAllText(artistsCachePath, json);
            Debug.Log("Saved artists cache to disk.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save artists cache: " + e.Message);
        }
    }

    public static void SaveArtworksCache()
    {
        try 
        {
            EnsureDirectoryExists(artworksCachePath);
            EnsureDirectoryExists(artworksMediaCachePath);
            List<ArtworkDataHolder> holders = new List<ArtworkDataHolder>();
            foreach(var artwork in FirebaseLoader.Artworks)
            {
                holders.Add(ArtworkDataHolder.ToHolder(artwork));
            }
            ArtworkDataHolderListWrapper wrapper = new ArtworkDataHolderListWrapper { artworks = holders };
            string json = JsonUtility.ToJson(wrapper);
            File.WriteAllText(artworksCachePath, json);
            Debug.Log("Saved artworks cache to disk.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save artworks cache: " + e.Message);
        }
    }

    public static void SaveExhibitionsCache()
    {
        try
        {
            EnsureDirectoryExists(exhibitionsCachePath);
            List<ExhibitionDataHolder> holders = new List<ExhibitionDataHolder>();
            foreach(var exhibition in FirebaseLoader.Exhibitions)
            {
                holders.Add(ExhibitionDataHolder.FromExhibitionData(exhibition));
            }
            ExhibitionDataHolderListWrapper wrapper = new ExhibitionDataHolderListWrapper { exhibitions = holders };
            string json = JsonUtility.ToJson(wrapper);
            File.WriteAllText(exhibitionsCachePath, json);
            Debug.Log("Saved exhibitions cache to disk.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save exhibitions cache: " + e.Message);
        }
    }

    #region Artwork Downloads

    // Loads the list of artwork download info from disk
    private static void LoadArtworkDownloads()
    {
        if (File.Exists(artworksMediaCachePath))
        {
            try
            {
                string json = File.ReadAllText(artworksMediaCachePath);
                ArtworkDownloadHolderListWrapper wrapper = JsonUtility.FromJson<ArtworkDownloadHolderListWrapper>(json);
                if (wrapper != null && wrapper.downloads != null)
                {
                    ArtworkDownloads = wrapper.downloads;
                    Debug.Log($"Loaded download info for {ArtworkDownloads.Count} artworks.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load artwork downloads cache: " + e.Message);
            }
        }
    }

    // Saves the current list of artwork download info to disk
    private static void SaveArtworkDownloads()
    {
        try
        {
            ArtworkDownloadHolderListWrapper wrapper = new ArtworkDownloadHolderListWrapper { downloads = ArtworkDownloads };
            string json = JsonUtility.ToJson(wrapper);
            File.WriteAllText(artworksMediaCachePath, json);
            Debug.Log("Saved artwork downloads info to disk.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save artwork downloads cache: " + e.Message);
        }
    }

    /// <summary>
    /// Downloads all artwork images for the given artwork.
    /// Each image is saved as {artwork_id}_{index}.jpg.
    /// </summary>
    public static async Task DownloadArtworkImages(ArtworkData artwork)
    {
        if (artwork == null || artwork.artwork_image_references == null || artwork.artwork_image_references.Count == 0)
        {
            Debug.LogWarning("No artwork image references found.");
            return;
        }

        // Ensure media folder exists
        EnsureDirectoryExists(Path.Combine(mediaFolder, "dummy.txt")); // dummy.txt trick to create the folder

        // Check if we already have a download record for this artwork
        ArtworkDownloadHolder holder = ArtworkDownloads.FirstOrDefault(x => x.artwork_id == artwork.artwork_id);
        if (holder == null)
        {
            holder = new ArtworkDownloadHolder
            {
                artwork_id = artwork.artwork_id,
                mediaPaths = new List<string>(),
                contentPath = "" // not used for images
            };
            ArtworkDownloads.Add(holder);
        }

        for (int i = 0; i < artwork.artwork_image_references.Count; i++)
        {
            string url = artwork.artwork_image_references[i];
            string fileName = $"{artwork.artwork_id}_{i}.jpg";
            string filePath = Path.Combine(mediaFolder, fileName);

            // Skip download if file already exists
            if (!File.Exists(filePath))
            {
                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
                request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to download image from {url}: {request.error}");
                    continue;
                }
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                byte[] imageBytes = texture.EncodeToJPG();
                File.WriteAllBytes(filePath, imageBytes);
                Debug.Log($"Downloaded and saved image: {filePath}");
            }
            else
            {
                Debug.Log($"Image already exists: {filePath}");
            }

            // Add file path to holder if not already added
            if (!holder.mediaPaths.Contains(filePath))
            {
                holder.mediaPaths.Add(filePath);
            }
        }

        // Save updated download info to disk
        SaveArtworkDownloads();
    }
    
    public static void DeleteDownloadedImagesForArtwork(string artworkId)
    {
        try
        {
            // Find the download holder for the specified artwork.
            ArtworkDownloadHolder holder = ArtworkDownloads.FirstOrDefault(x => x.artwork_id == artworkId);
            if (holder != null)
            {
                // Delete each downloaded image file.
                foreach (string filePath in holder.mediaPaths)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Debug.Log($"Deleted file: {filePath}");
                    }
                }
                // Remove the holder from the in-memory list and update the cache file.
                ArtworkDownloads.Remove(holder);
                SaveArtworkDownloads();
                Debug.Log($"Deleted all downloaded images for artwork: {artworkId}");
            }
            else
            {
                Debug.Log($"No downloaded images found for artwork: {artworkId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to delete downloaded images for artwork: " + e.Message);
        }
    }


    #endregion
}

// Wrapper classes for JSON serialization of download info
[Serializable]
public class ArtworkDownloadHolderListWrapper
{
    public List<ArtworkDownloadHolder> downloads;
}

[Serializable]
public class ArtistDataHolderListWrapper
{
    public List<ArtistDataHolder> artists;
}

[Serializable]
public class ArtworkDataHolderListWrapper
{
    public List<ArtworkDataHolder> artworks;
}

[Serializable]
public class ExhibitionDataHolderListWrapper
{
    public List<ExhibitionDataHolder> exhibitions;
}
