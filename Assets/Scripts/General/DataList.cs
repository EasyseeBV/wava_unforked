using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class DataList
{
    private Dictionary<string, Sprite> data = new Dictionary<string, Sprite>();
    private FirebaseData cachedFirebaseData;
    
    public async Task<(Sprite sprite, bool requiresSave)> Get(FirebaseData firebaseData, string key)
    {
        cachedFirebaseData = firebaseData;
        
        Uri uri = new Uri(key);
        string fileName = Path.GetFileName(uri.LocalPath);
    
        if (data.TryGetValue(fileName, out var sprite))
        {
            return (sprite, false);
        }

        // Construct the local file path.
        string localPath = Path.Combine(AppCache.MediaFolder, fileName);
        
        bool requiresSave = false;
    
        // Check if the file is already cached locally. Otherwise, download it.
        if (!File.Exists(localPath))
        {
            var results = await FirebaseLoader.DownloadMedia(AppCache.MediaFolder, key, null);
            localPath = results.localPath;
            requiresSave = results.downloaded;
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
            {
                Debug.LogError($"Failed to obtain a valid file for key: {key}");
                return (null, false);
            }

            if (!firebaseData.cached.Contains(localPath))
            {
                firebaseData.cached.Add(localPath);
            }
        }
    
        try
        {
            // Read the file bytes.
            byte[] imageData = await File.ReadAllBytesAsync(localPath);
        
            // Create a texture and load image data.
            Texture2D texture = new Texture2D(2, 2); // size will be replaced by loaded image dimensions
            if (!texture.LoadImage(imageData))
            {
                Debug.LogError($"Failed to load image data from file: {localPath}");
                return (null, false);
            }
        
            // Create a Sprite from the texture.
            sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            // Cache the sprite.
            data.TryAdd(fileName, sprite);
            return (sprite, requiresSave);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading sprite from file {localPath}: {e.Message}");
            return (null, false);
        }
    }

    public int Count() => data.Count;

    public List<Sprite> Get()
    {
        return data.Values.ToList();
    } 
}
