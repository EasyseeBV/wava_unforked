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
        string localPath = Path.Combine(AppCache.MediaFolder, fileName);
        
        if (File.Exists(localPath))
        {
            try
            {
                byte[] existingData = File.ReadAllBytes(localPath);
                var tmpTex = new Texture2D(2, 2);
                if (!tmpTex.LoadImage(existingData))
                {
                    Debug.LogWarning("Cached image corrupted; re-downloading");
                    File.Delete(localPath);
                    // fall through to download
                }
                else
                {
                    // it's good—create sprite and return
                    var goodSprite = Sprite.Create(
                        tmpTex,
                        new Rect(0,0,tmpTex.width, tmpTex.height),
                        new Vector2(0.5f,0.5f));
                    return (goodSprite, false);
                }
            }
            catch
            {
                File.Delete(localPath);
            }
        }
    
        // Download afresh
        var (downloadedPath, didDownload) =
            await DownloadManager.Instance.BackgroundDownloadImage(
                AppCache.MediaFolder,
                key,
                null);

        if (string.IsNullOrEmpty(downloadedPath))
        {
            Debug.LogError($"Failed to download image at {key}");
            return (null, false);
        }

        // record caching
        if (!firebaseData.cached.Contains(downloadedPath))
            firebaseData.cached.Add(downloadedPath);

        // load final sprite
        byte[] imageData = await File.ReadAllBytesAsync(downloadedPath);
        var texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        var sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));

        data.TryAdd(fileName, sprite);
        return (sprite, didDownload);
    }

    public int Count() => data.Count;

    public List<Sprite> Get()
    {
        return data.Values.ToList();
    } 
}
