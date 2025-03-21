using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class AppCache
{
    // Paths for storing FirebaseData's
    private static readonly string artistDataCachePath = Path.Combine(Application.persistentDataPath, "artistsCache.json");
    private static readonly string artworkDataCachePath = Path.Combine(Application.persistentDataPath, "artworksCache.json");
    private static readonly string exhibitionDataCachePath = Path.Combine(Application.persistentDataPath, "exhibitionsCache.json");
    
    // Folder for the actual downloaded files
    public static readonly string MediaFolder = Path.Combine(Application.persistentDataPath, "media");
    public static readonly string ContentFolder = Path.Combine(Application.persistentDataPath, "content");
    
    public static bool Loaded { get; private set; } = false;
    
    #region Loading local cache
    
    public static async Task LoadLocalCaches()
    {
        Debug.Log("Loading local caches...");
        
        // Directory checking
        await EnsureDirectoryExists(artistDataCachePath);
        await EnsureDirectoryExists(artworkDataCachePath);
        await EnsureDirectoryExists(exhibitionDataCachePath);
        await EnsureDirectoryExists(MediaFolder);
        
        // Load FirebaseData
        await LoadArtistCache();
        await LoadArtworkCache();
        await LoadExhibitionCache();

        Debug.Log("Local cache loaded");
        Loaded = true;
    }

    private static async Task LoadArtistCache()
    {
        if (File.Exists(artistDataCachePath))
        {
            try 
            {
                string json = await File.ReadAllTextAsync(artistDataCachePath);
                ArtistDataHolderListWrapper wrapper = JsonUtility.FromJson<ArtistDataHolderListWrapper>(json);
                if (wrapper is { artists: not null })
                {
                    foreach (var artist in wrapper.artists.Select(ArtistDataHolder.ToArtistData))
                    {
                        FirebaseLoader.AddArtistData(artist);
                    }

                    Debug.Log($"Loaded [{wrapper.artists.Count}] artists from local cache.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load artists cache: " + e.Message);
            }
        }
        else Debug.Log("Artist cache does not exist");
    }
    
    private static async Task LoadArtworkCache()
    {
        if (File.Exists(artworkDataCachePath))
        {
            try 
            {
                string json = await File.ReadAllTextAsync(artworkDataCachePath);
                ArtworkDataHolderListWrapper wrapper = JsonUtility.FromJson<ArtworkDataHolderListWrapper>(json);
                if (wrapper != null && wrapper.artworks != null)
                {
                    foreach (var artwork in wrapper.artworks.Select(ArtworkDataHolder.FromHolder))
                    {
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
    }
    
    private static async Task LoadExhibitionCache()
    {
        if (File.Exists(exhibitionDataCachePath))
        {
            try 
            {
                string json = await File.ReadAllTextAsync(exhibitionDataCachePath);
                ExhibitionDataHolderListWrapper wrapper = JsonUtility.FromJson<ExhibitionDataHolderListWrapper>(json);
                if (wrapper is { exhibitions: not null })
                {
                    foreach (var exhibition in wrapper.exhibitions.Select(ExhibitionDataHolder.FromHolder))
                    {
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
    }
    
    #endregion
    
    #region Save Data
    
    public static async Task SaveArtistsCache()
    {
        try 
        {
            await EnsureDirectoryExists(artistDataCachePath);
            List<ArtistDataHolder> holders = FirebaseLoader.Artists.Select(artist => ArtistDataHolder.FromArtistData(artist)).ToList();
            ArtistDataHolderListWrapper wrapper = new ArtistDataHolderListWrapper { artists = holders };
            string json = JsonUtility.ToJson(wrapper, true);
            await File.WriteAllTextAsync(artistDataCachePath, json);
            Debug.Log("Saved artists cache to disk.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save artists cache: " + e.Message);
        }
    }

    public static async Task SaveArtworksCache()
    {
        try 
        {
            await EnsureDirectoryExists(artworkDataCachePath);
            List<ArtworkDataHolder> holders = FirebaseLoader.Artworks.Select(ArtworkDataHolder.ToHolder).ToList();
            ArtworkDataHolderListWrapper wrapper = new ArtworkDataHolderListWrapper { artworks = holders };
            string json = JsonUtility.ToJson(wrapper, true);
            await File.WriteAllTextAsync(artworkDataCachePath, json);
            Debug.Log("Saved artworks cache to disk.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save artworks cache: " + e.Message);
        }
    }

    public static async Task SaveExhibitionsCache()
    {
        try
        {
            await EnsureDirectoryExists(exhibitionDataCachePath);
            List<ExhibitionDataHolder> holders = FirebaseLoader.Exhibitions.Select(exhibition => ExhibitionDataHolder.FromExhibitionData(exhibition)).ToList();
            ExhibitionDataHolderListWrapper wrapper = new ExhibitionDataHolderListWrapper { exhibitions = holders };
            string json = JsonUtility.ToJson(wrapper, true);
            await File.WriteAllTextAsync(exhibitionDataCachePath, json);
            Debug.Log("Saved exhibitions cache to disk.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save exhibitions cache: " + e.Message);
        }
    }
    
    #endregion

    #region Helper

    // Ensures that the directory for a given file path exists.
    private static async Task EnsureDirectoryExists(string filePath)
    {
        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                if (directory != null)
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log("Created new directory: " + directory);
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"Failed to validate directory [{filePath}] with error: {e}");
        }
    }

    #endregion
}

// Wrapper classes for JSON serialization of download info
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
