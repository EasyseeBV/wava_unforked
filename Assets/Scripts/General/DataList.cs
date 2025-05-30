using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class DataList
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3, 3);
    private Dictionary<string, Sprite> data = new Dictionary<string, Sprite>();
    private FirebaseData cachedFirebaseData;
    
    public async Task<(Sprite sprite, bool requiresSave)> Get(FirebaseData firebaseData, string key)
    {
        await _semaphore.WaitAsync();
        try
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
                var results = await FirebaseLoader.DownloadImage(AppCache.MediaFolder, key, null);
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
                using var uwr = UnityWebRequestTexture.GetTexture("file://" + localPath, true);
                var op = uwr.SendWebRequest();
                
                // yield to Unity each frame until done
                while (!op.isDone) await Task.Yield();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"UWR error loading texture from {localPath}: {uwr.error}");
                    return (null, false);
                }

                // Grab the Texture2D that was downloaded
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    
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
        finally
        {
            // Let the next waiting call proceed
            _semaphore.Release();
        }
    }

    public int Count() => data.Count;

    public List<Sprite> Get()
    {
        return data.Values.ToList();
    } 
}
