using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.Mappers;
using TriLibCore.URP.Mappers;
using UnityEngine;

public class DownloadManager : MonoBehaviour
{
    private static DownloadManager _instance;
    public static DownloadManager Instance => _instance;

    public static Dictionary<string, GameObject> LocalModels = new Dictionary<string, GameObject>();
    
    private AssetLoaderOptions _assetLoaderOptions;
    
    private void Awake() 
    {
        if (_instance == null) 
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else 
        {
            Destroy(gameObject);
        }
    }

    public async Task<(string localPath, bool downloaded)> BackgroundDownloadMedia(string storagePath, string path, ARDownloadBar downloadBar, int index = 0)
    {
        return await FirebaseLoader.DownloadMedia(storagePath, path, downloadBar, index);
    }

    public async Task LoadModels(List<MediaContentData> content, string artworkName)
    {
        foreach (var mediaContentData in content)
        {
            try
            {
                var path = await TryGetModelPath(mediaContentData);
                var extension = Path.GetExtension(path);

                if (!File.Exists(path))
                {
                    Debug.Log("File did not exist at path: " + path);
                    continue;
                }

                if (extension is not (".fbx" or ".obj" or ".gltf" or ".gltf2"))
                {
                    Debug.Log("Artwork content not loaded as it is not a model");
                    continue;
                }

                if (_assetLoaderOptions == null)
                {
                    _assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
                    _assetLoaderOptions.MaterialMappers = new MaterialMapper[]
                    {
                        ScriptableObject.CreateInstance<UniversalRPMaterialMapper>()
                    };
                    _assetLoaderOptions.AnimationType = AnimationType.Legacy;
                    _assetLoaderOptions.AutomaticallyPlayLegacyAnimations = true;
                }

                // Load the model from the local file path instead of downloading it.
                AssetLoader.LoadModelFromFile(
                    path: path,
                    onLoad: OnLoad,
                    onMaterialsLoad: c => { OnMaterialsLoad(c, artworkName); },
                    onProgress: OnProgress,
                    onError: OnError,
                    wrapperGameObject: null,
                    assetLoaderOptions: _assetLoaderOptions
                );
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private async Task<string> TryGetModelPath(MediaContentData content)
    {
        if (string.IsNullOrEmpty(content.media_content))
        {
            Debug.LogWarning($"Content is missing. Skipping.");
            return string.Empty;
        }
        
        var uri = new Uri(content.media_content);
        string encodedPath = uri.AbsolutePath;
        string decodedPath = Uri.UnescapeDataString(encodedPath);
        string fileName = Path.GetFileName(decodedPath);
        bool storedLocal = false;

        string path = content.media_content; // default the path to the firestore uri of the content
        string localPath = Path.Combine(AppCache.ContentFolder, fileName);
        
        // if the file does not exist locally, download it
        if (!File.Exists(localPath))
        {
            // failed to download handling needs to be done here
            if (FirebaseLoader.OfflineMode)
            {
                return String.Empty;
            }
            
            var results = await BackgroundDownloadMedia(AppCache.ContentFolder, content.media_content, null, 0);
            path = results.localPath;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Debug.Log("Content was downloaded and stored locally");
            }
        }
        else if (File.Exists(localPath)) // if the file does exist, set the path to that location
        {
            path = localPath;
            Debug.Log("Content was found locally");
        }

        return path;
    }

    #region Model Loading Callbacks
    private void OnError(IContextualizedError obj) => Debug.LogError($"An error occurred while loading your model: {obj.GetInnerException()}");
    
    private void OnProgress(AssetLoaderContext assetLoaderContext, float f) { }

    private void OnLoad(AssetLoaderContext assetLoaderContext) { }

    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext, string artworkName)
    {
        Debug.Log("All materials have been applied. The model is fully loaded.");
        var obj = assetLoaderContext.RootGameObject;
        obj.SetActive(false);
        obj.name = $"Loaded Model [{artworkName}]";
        obj.transform.SetParent(gameObject.transform);
        LocalModels.TryAdd(artworkName, obj);
    }
    
    #endregion
}