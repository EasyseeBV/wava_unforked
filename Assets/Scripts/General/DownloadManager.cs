using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.Mappers;
using TriLibCore.URP.Mappers;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadManager : MonoBehaviour
{
    private static DownloadManager _instance;
    public static DownloadManager Instance => _instance;

    public static Dictionary<string, GameObject> LocalModels = new Dictionary<string, GameObject>();
    
    private AssetLoaderOptions _assetLoaderOptions;

    private HashSet<string> _currentlyDownloadingURLs = new();

    public Action<ArtworkData> StartedArtworkDownloadProcess;

    public Action<ArtworkData> FinishedArtworkDownloadProcess;

    public Action<ExhibitionData> StartedExhibitionDownloadProcess;

    public Action<ExhibitionData> FinishedExhibitionDownloadProcess;

    public enum DownloadStatus { Unavailable, Downloadable, Downloading, Downloaded}

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

    public async Task<(string localPath, bool downloaded)> BackgroundDownloadMedia(string storagePath, string path, ARDownloadBar downloadBar, int index = 0, Action<float> progressChangedCallback = null, Action<UnityWebRequest.Result> resultCallback = null)
    {
        _currentlyDownloadingURLs.Add(path);
        var task = await FirebaseLoader.DownloadMedia(storagePath, path, downloadBar, index, progressChangedCallback, resultCallback);
        _currentlyDownloadingURLs.Remove(path);
        return task;
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

    public DownloadStatus GetDownloadStatusFor(MediaContentData mediaContent)
    {
        if (mediaContent == null)
            return DownloadStatus.Unavailable;

        var uri = new Uri(mediaContent.media_content);
        string encodedPath = uri.AbsolutePath;
        string decodedPath = Uri.UnescapeDataString(encodedPath);
        string fileName = Path.GetFileName(decodedPath);
        string localPath = Path.Combine(AppCache.ContentFolder, fileName);

        if (File.Exists(localPath))
            return DownloadStatus.Downloaded;

        if (_currentlyDownloadingURLs.Contains(mediaContent.media_content))
            return DownloadStatus.Downloading;

        return DownloadStatus.Downloadable;
    }

    public DownloadStatus GetDownloadStatusFor(ArtworkData artwork)
    {
        if (artwork == null)
            return DownloadStatus.Unavailable;

        if (artwork.content_list.Count == 0)
            return DownloadStatus.Downloaded;

        var someMediaIsDownloadable = false;

        for (int i = 0; i < artwork.content_list.Count; i++)
        {
            var media = artwork.content_list[i];

            var status = GetDownloadStatusFor(media);

            // The artwork is downloading if any media is downloading.
            if (status == DownloadStatus.Downloading)
                return DownloadStatus.Downloading;

            someMediaIsDownloadable |= status == DownloadStatus.Downloadable;
        }

        // The artwork is downloaded if all media are either downloaded or unavailable.
        if (!someMediaIsDownloadable)
            return DownloadStatus.Downloaded;

        return DownloadStatus.Downloadable;
    }

    public DownloadStatus GetDownloadStatusFor(ExhibitionData exhibition)
    {
        if (exhibition == null)
            return DownloadStatus.Unavailable;

        if (exhibition.artworks.Count == 0)
            return DownloadStatus.Downloaded;

        var someArtworkIsDownloadable = false;

        for (int i = 0; i < exhibition.artworks.Count; i++)
        {
            var artwork = exhibition.artworks[i];

            var status = GetDownloadStatusFor(artwork);

            // The exhibition is downloading if any artwork is downloading.
            if (status == DownloadStatus.Downloading)
                return DownloadStatus.Downloading;

            someArtworkIsDownloadable |= status == DownloadStatus.Downloadable;
        }

        // The exhibition is downloaded if all artworks are either downloaded or unavailable.
        if (!someArtworkIsDownloadable)
            return DownloadStatus.Downloaded;

        return DownloadStatus.Downloadable;
    }

    public static async Task DownloadMediaContent(MediaContentData media, Action<float> progressCallback = null, Action<UnityWebRequest.Result> resultCallback = null)
    {
        var downloadStatus = Instance.GetDownloadStatusFor(media);

        if (downloadStatus == DownloadStatus.Downloaded || downloadStatus == DownloadStatus.Unavailable)
        {
            progressCallback?.Invoke(1f);
            resultCallback?.Invoke(UnityWebRequest.Result.Success);
        } else
        {
            await Instance.BackgroundDownloadMedia(AppCache.ContentFolder, media.media_content, null, 0, progressCallback, resultCallback);
        }
    }

    public static async Task DownloadArtwork(ArtworkData artwork, Action<float> progressChangedCallback = null, Action<UnityWebRequest.Result> resultCallback = null)
    {
        Instance.StartedArtworkDownloadProcess?.Invoke(artwork);

        // Automatic success if artwork has no media.
        if (artwork.content_list.Count == 0)
        {
            progressChangedCallback?.Invoke(1f);
            resultCallback?.Invoke(UnityWebRequest.Result.Success);
            Instance.FinishedArtworkDownloadProcess?.Invoke(artwork);
            return;
        }

        var progresses = Enumerable.Repeat(0f, artwork.content_list.Count).ToList();

        var doneCount = 0;

        bool success = true;

        foreach (var content in artwork.content_list)
        {
            await DownloadMediaContent(content, (progress) =>
            {
                if (progressChangedCallback == null)
                    return;

                var totalprogress = progresses.Sum() / progresses.Count;

                progressChangedCallback.Invoke(totalprogress);
            }, (result) =>
            {
                doneCount++;

                if (result != UnityWebRequest.Result.Success)
                    success = false;

                if (doneCount == artwork.content_list.Count)
                {
                    resultCallback?.Invoke(success ? UnityWebRequest.Result.Success : UnityWebRequest.Result.ConnectionError);
                }
            });
        }

        Instance.FinishedArtworkDownloadProcess?.Invoke(artwork);
    }

    public static async Task DownloadExhibition(ExhibitionData exhibition, Action<float> progressChangedCallback = null, Action<UnityWebRequest.Result> resultCallback = null)
    {
        Instance.StartedExhibitionDownloadProcess?.Invoke(exhibition);

        // Automatic success if exhibition has no artworks.
        if (exhibition.artworks.Count == 0)
        {
            progressChangedCallback?.Invoke(1f);
            resultCallback?.Invoke(UnityWebRequest.Result.Success);
            Instance.FinishedExhibitionDownloadProcess.Invoke(exhibition);
            return;
        }

        var progresses = Enumerable.Repeat(0f, exhibition.artworks.Count).ToList(); ;

        var doneCount = 0;

        bool success = true;

        foreach (var artwork in exhibition.artworks)
        {
            await DownloadArtwork(artwork, (progress) =>
            {
                if (progressChangedCallback == null)
                    return;

                var totalprogress = progresses.Sum() / progresses.Count;

                progressChangedCallback.Invoke(totalprogress);
            }, (result) =>
            {
                doneCount++;

                if (result != UnityWebRequest.Result.Success)
                    success = false;

                if (doneCount == exhibition.artworks.Count)
                {
                    resultCallback?.Invoke(success ? UnityWebRequest.Result.Success : UnityWebRequest.Result.ConnectionError);
                }
            });
        }

        Instance.FinishedExhibitionDownloadProcess?.Invoke(exhibition);
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