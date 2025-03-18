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
    
    public async Task<Sprite> Get(FirebaseData firebaseData, string key)
    {
        cachedFirebaseData = firebaseData;
        
        Uri uri = new Uri(key);
        string fileName = Path.GetFileName(uri.LocalPath);
    
        if (data.TryGetValue(fileName, out var sprite))
        {
            return sprite;
        }

        // Construct the local file path.
        string localPath = Path.Combine(AppCache.MediaFolder, fileName);
    
        // Check if the file is already cached locally. Otherwise, download it.
        if (!File.Exists(localPath))
        {
            localPath = await FirebaseLoader.DownloadMedia(key);
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
            {
                Debug.LogError($"Failed to obtain a valid file for key: {key}");
                return null;
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
                return null;
            }
        
            // Create a Sprite from the texture.
            sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            // Cache the sprite.
            data.TryAdd(fileName, sprite);
            return sprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading sprite from file {localPath}: {e.Message}");
            return null;
        }

        return null;
    }

    public int Count() => data.Count;

    public List<Sprite> Get()
    {
        return data.Values.ToList();
    } 
}
