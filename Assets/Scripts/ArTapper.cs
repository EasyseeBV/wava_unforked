using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using TriLibCore;
using Unity.XR.CoreUtils;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Video;
using UnityEngine.Events;
using System.Linq;
using Messy.Definitions;
using TriLibCore.Extensions;
using TriLibCore.General;
using TriLibCore.Mappers;
using TriLibCore.URP.Mappers;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;

public class ArTapper : MonoBehaviour
{
    public static ArtworkData ArtworkToPlace;
    
    public static bool PlaceDirectly = false;
    public static float DistanceWhenActivated;

    bool searching;

    [Header("References")]
    public ARSession arSession;
    public XROrigin arOrigin;
    public ARRaycastManager arRaycast;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private ARNamebar arNamebar;
    [SerializeField] private ARInfoPage arInfoPage;
    [SerializeField] private ARDownloadBar downloadBar;
    [SerializeField] private NoConnectionMapHandler noConnectionMapHandler;
    [SerializeField] private GameObject uiTutorialContainer;
    [SerializeField] private ARAnchorManager arAnchorManager;
    
    [Header("Firebase Preloaded elements")]
    [SerializeField] private ARObject arObjectPrefab;
    [SerializeField] private bool testContent;

    [Header("Dependencies")]
    [SerializeField] private Transform outOfScreenLoadLocation;
    [SerializeField] private TMP_Text eventLabel;
    [SerializeField] private GameObject loadingPlane;
    [SerializeField] private ObjectSpawner objectSpawner;
    [SerializeField] private ARInteractorSpawnTrigger interactorSpawnTrigger;

    [Header("Presets")] // cleaned up in a future phase
    [SerializeField] private GameObject coin;
    [SerializeField] private GameObject bird;
    [SerializeField] private GameObject tree;
    [SerializeField] private GameObject elevator;
    [SerializeField] private Material[] monuments;

    public ARObject arObject { get; private set; }
    
    private Pose placementPose;
    private ARRaycastHit foundHit;

    private bool placementPoseIsValid = false;

    // Content
    private bool videoPlayerReady = false;
    private bool hasContent = false;
    private bool containsVideo = false;
    
    bool StartedAnimation;
    
    private int contentTotalCount, contentLoadedCount = 0;
    private bool allContentLoaded = false;
    private AssetLoaderOptions _assetLoaderOptions;
    public Dictionary<int, GameObject> contentDict = new Dictionary<int, GameObject>();
    private static bool HideShadow = false;

    public static event Action<ArtworkData> OnArtworkPlaced;

    #region Unity Lifecycle
    
    private void OnEnable()
    {
        if (objectSpawner) objectSpawner.objectSpawned += OnTouch;
    }

    private void OnDisable()
    {
        if (objectSpawner) objectSpawner.objectSpawned -= OnTouch;
    }
    
    private void Awake()
    {
        if (arObjectPrefab) arObject = Instantiate(arObjectPrefab);
        
        if (!objectSpawner) objectSpawner = FindObjectOfType<ObjectSpawner>();
        if (objectSpawner) objectSpawner.arObject = arObject.gameObject;
        
        if (!interactorSpawnTrigger) interactorSpawnTrigger = FindObjectOfType<ARInteractorSpawnTrigger>();
        if (interactorSpawnTrigger)
        {
            foreach (var content in ArtworkToPlace.content_list)
            {
                interactorSpawnTrigger.ContentOffsets.Add(new Vector2(content.transforms.position_offset.x_offset, content.transforms.position_offset.z_offset));
            }
        }
            
        if (downloadBar == null) downloadBar = FindObjectOfType<ARDownloadBar>();
        HideShadow = false;
    }

    private void Start()
    {
        //StartAR();
        LoadContent();
    }
    
    private void Update()
    {
#if UNITY_EDITOR
        if (testContent)
        {
            testContent = false;
            TryPlaceObject();
        }
#endif
        
    }
    
    #endregion
    
    // When you select a placement for the AR content
    private void OnTouch(GameObject obj)
    {
        Debug.Log("OnTouch");
        // object placement is handled from the method that invokes this callback
        TryPlaceObject();
    }

    // If the placement was triggered, but the content hasn't been loaded in yet
    private IEnumerator WaitForLoad()
    {
        yield return new WaitUntil(() => allContentLoaded);
        loadingPlane.gameObject.SetActive(false);
        OnArtworkReady();
    }
    
    private void TryPlaceObject()
    {
        if (hasContent && allContentLoaded)
        {
            OnArtworkReady();
        }
        else if (hasContent && !allContentLoaded)
        {
            StartCoroutine(WaitForLoad());
        }
        else if(!hasContent)
        {
            Debug.Log("no content found...");
        }
        
        arNamebar.SetNamebarLabel(ArtworkToPlace.title);
        arInfoPage.CanOpen = true;
    }

    // Artwork is ready - show the artwork
    private void OnArtworkReady()
    {
        Debug.Log("Showing Artwork");
        loadingPlane.SetActive(false);
        //arObject.gameObject.AddComponent<ARAnchor>();
        //arPlaneManager.enabled = false;
        arObject.gameObject.SetActive(true);
        arObject.Show(HideShadow);

        arPlaneManager.planePrefab = null;
        foreach (var trackable in arPlaneManager.trackables)
        {
            trackable.gameObject.SetActive(false);
            /*MeshRenderer meshRenderer = trackable.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.material = shadowCatcherMaterial;
            
            LineRenderer lineRenderer = trackable.GetComponent<LineRenderer>();
            if (lineRenderer != null) lineRenderer.enabled = false;*/
        }
        
        OnArtworkPlaced?.Invoke(ArtworkToPlace);
        
        UIInfoController.Instance.SetDefaultText("Congratulations, the artwork is placed!");
    }

    #region Content Loading

    private IEnumerator ShowLoadingBar()
    {
        yield return new WaitForSeconds(2f);

        if (allContentLoaded || !hasContent) yield return null;
        else
        {
            downloadBar.Show();
        }
    }

    private async void LoadContent()
    {
        if ((ArtworkToPlace?.content_list == null || ArtworkToPlace.content_list.Count == 0) && string.IsNullOrEmpty(ArtworkToPlace?.preset))
        {
            Debug.LogWarning("Artwork to place is missing or there is no content available.");
            return;
        }

        StartCoroutine(ShowLoadingBar());

        hasContent = true;
        bool loadContent = true;
        bool hasPreset = false;

        // estimate total size of all content to be loaded for the donwload bar
        int totalContentEstimate = 0;
        if (!string.IsNullOrEmpty(ArtworkToPlace.preset) && ArtworkToPlace.preset != "None")
        {
            hasPreset = true;
            downloadBar.hasPresets = true;
        }
        if (ArtworkToPlace.content_list != null && ArtworkToPlace.content_list.Count > 0)
            totalContentEstimate += ArtworkToPlace.content_list.Count;
        if (totalContentEstimate > 0) downloadBar.gameObject.SetActive(true);
        downloadBar.SetSize(totalContentEstimate);
        
        // load preset content
        if (hasPreset)
        {
            Debug.Log("Loading a preset: " + ArtworkToPlace.preset);
                
            switch (ArtworkToPlace.preset)
            {
                case "Bird Animation":
                {
                    contentTotalCount++;
                    var birdObj = Instantiate(bird, Vector3.zero, Quaternion.identity);
                    arObject.Add(birdObj);
                    contentDict.TryAdd(contentDict.Count, birdObj);
                    contentLoadedCount++;
                    break;
                }
                case "Coin Clicker":
                {
                    contentTotalCount++;
                    var coinObj = Instantiate(coin, Vector3.zero, Quaternion.identity);
                    arObject.Add(coinObj);
                    contentDict.TryAdd(contentDict.Count, coinObj);
                    contentLoadedCount++;
                    break;
                }
                case "Tree":
                    contentTotalCount++;
                    var treeObj = Instantiate(tree, Vector3.zero, Quaternion.identity);
                    arObject.Add(treeObj);
                    contentDict.TryAdd(contentDict.Count, treeObj);
                    contentLoadedCount++;
                    HideShadow = true;
                    break;
                case "Elevator":
                    contentTotalCount++;
                    var elevatorObj = Instantiate(elevator);
                    arObject.Add(elevatorObj);
                    contentDict.TryAdd(contentDict.Count, elevatorObj);
                    contentLoadedCount++;
                    break;
                case "Monument":
                    loadContent = false;
                    
                    if (ArtworkToPlace.content_list.Count > 0)
                    {
                        contentTotalCount++;
                        
                        // separate out this code later
                        var content = ArtworkToPlace.content_list[0];
                        var uri = new Uri(content.media_content);
                        string encodedPath = uri.AbsolutePath;
                        string decodedPath = Uri.UnescapeDataString(encodedPath);
                        string fileName = Path.GetFileName(decodedPath);
                        containsVideo = false;
                        bool storedLocal = false;

                        string path = content.media_content; // default the path to the firestore uri of the content
                        string localPath = Path.Combine(AppCache.ContentFolder, fileName);
            
                        // if the file does not exist locally, download it
                        if (!File.Exists(localPath))
                        {
                            if (FirebaseLoader.OfflineMode)
                            {
                                noConnectionMapHandler.TryDisplay();
                                //uiTutorialContainer.gameObject.SetActive(false);
                                return;
                            }
                            
                            var result = await DownloadManager.Instance.BackgroundDownloadMedia(AppCache.ContentFolder, content.media_content, downloadBar, 0);
                            path = result.localPath;
                        }
                        else if (File.Exists(localPath)) // if the file does exist, set the path to that location
                        {
                            path = localPath;
                        }
                        
                        if (DownloadManager.LocalModels.TryGetValue(ArtworkToPlace.id, out var localModel))
                        {
                            Debug.Log("Loading model from local cache.");
                            contentLoadedCount++;
                            var obj = Instantiate(localModel, arObject.transform);
                            contentDict.TryAdd(contentDict.Count, obj);
                            obj.name = "Loaded Model (Cache)";
                            arObject.Add(obj, content);

                            if (contentLoadedCount >= contentTotalCount)
                            {
                                allContentLoaded = true;
                                downloadBar?.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            if (_assetLoaderOptions == null)
                            {
                                _assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
                                _assetLoaderOptions.MaterialMappers = new MaterialMapper[]
                                {
                                    ScriptableObject.CreateInstance<UniversalRPMaterialMapper>()
                                };
                            }
                    
                            Debug.Log("attempting to load local model file: " + fileName);

                            // Load the model from the local file path instead of downloading it.
                            AssetLoader.LoadModelFromFile(
                                path: path,
                                onLoad: OnLoad,
                                onMaterialsLoad: c => { OnMaterialsLoad(c, content, contentDict.Count, monuments); },
                                onProgress: (c, progress) => { OnProgress(c, progress, 0); },
                                onError: OnError,
                                wrapperGameObject: null,
                                assetLoaderOptions: _assetLoaderOptions
                            );    
                        }
                        
                        break;
                    }
                    
                    // load content
                    // on material load custom?
                    break;
            }

            if (contentLoadedCount >= contentTotalCount)
            {
                allContentLoaded = true;
                downloadBar?.gameObject.SetActive(false);
            }
        }

        if (!loadContent) return;

        for (int i = 0; i < ArtworkToPlace.content_list.Count; i++)
        {
            var content = ArtworkToPlace.content_list[i];

            if (string.IsNullOrEmpty(content.media_content))
            {
                Debug.LogWarning($"Content is missing from index [{i}] on artwork [{ArtworkToPlace.title}]. Skipping.");
                continue;
            }
            
            contentTotalCount++;
            var uri = new Uri(content.media_content);
            string encodedPath = uri.AbsolutePath;
            string decodedPath = Uri.UnescapeDataString(encodedPath);
            string fileName = Path.GetFileName(decodedPath);
            var extension = Path.GetExtension(fileName);
            containsVideo = false;
            bool storedLocal = false;

            string path = content.media_content; // default the path to the firestore uri of the content
            string localPath = Path.Combine(AppCache.ContentFolder, fileName);
            
            // if the file does not exist locally, download it
            if (!File.Exists(localPath))
            {
                // failed to download handling needs to be done here
                if (FirebaseLoader.OfflineMode)
                {
                    noConnectionMapHandler.TryDisplay();
                    //uiTutorialContainer.gameObject.SetActive(false);
                    return;
                }
                
                var results = await DownloadManager.Instance.BackgroundDownloadMedia(AppCache.ContentFolder, content.media_content, downloadBar, i);
                path = results.localPath;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    Debug.Log("Content was downloaded and stored locally");
                    storedLocal = true;
                }
            }
            else if (File.Exists(localPath)) // if the file does exist, set the path to that location
            {
                path = localPath;
                storedLocal = true;
                Debug.Log("Content was found locally");
            }
            
            switch (extension)
            {
                case ".mp4" or ".mvk" or ".mov": // video
                    Debug.Log($"content [{ArtworkToPlace.title}] contained a media piece");
                
                    containsVideo = true;
                    
                    var videoPlayer = arObject.Add(content, "file:///" + path, player =>
                    {
                        Debug.Log("Video prepared");
                        contentLoadedCount++;
                        if (contentLoadedCount >= contentTotalCount)
                        {
                            allContentLoaded = true;
                            downloadBar?.gameObject.SetActive(false);
                        }
                    });
                    contentDict.TryAdd(i, videoPlayer.gameObject);
                    break;
                
                case ".mp3": // audio
                    Debug.Log($"content [{ArtworkToPlace.title}] contained an audio piece");
                    StartCoroutine(LoadAudioClip(path, content, i));
                    break;
                
                case ".fbx" or ".obj" or ".gltf" or ".gltf2" when storedLocal: // stored local model
                {
                    if (DownloadManager.LocalModels.TryGetValue(ArtworkToPlace.id, out var localModel))
                    {
                        Debug.Log("Loading model from local cache.");
                        contentLoadedCount++;
                        var obj = Instantiate(localModel, arObject.transform);
                        contentDict.TryAdd(i, obj);
                        obj.name = "Loaded Model (Cache)";
                        arObject.Add(obj, content);

                        if (contentLoadedCount >= contentTotalCount)
                        {
                            allContentLoaded = true;
                            downloadBar?.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
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
                    
                        Debug.Log("attempting to load local model file: " + fileName);

                        // Load the model from the local file path instead of downloading it.
                        var i1 = i;
                        AssetLoader.LoadModelFromFile(
                            path: path,
                            onLoad: OnLoad,
                            onMaterialsLoad: c => { OnMaterialsLoad(c, content, i1); },
                            onProgress: (c, progress) => { OnProgress(c, progress, i1); },
                            onError: OnError,
                            wrapperGameObject: null,
                            assetLoaderOptions: _assetLoaderOptions
                        );    
                    }
                    
                    break;
                }
                
                case ".fbx" or ".obj" or ".gltf" or ".gltf2": // url model
                {
                    if (DownloadManager.LocalModels.TryGetValue(path, out var localModel))
                    {
                        Debug.Log("Loading model from local cache.");
                        contentLoadedCount++;
                        var obj = Instantiate(localModel, arObject.transform);
                        contentDict.TryAdd(i, obj);
                        obj.name = "Loaded Model (Cache)";
                        arObject.Add(obj, content);

                        if (contentLoadedCount >= contentTotalCount)
                        {
                            allContentLoaded = true;
                            downloadBar?.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (_assetLoaderOptions == null)
                        {
                            _assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
                            _assetLoaderOptions.MaterialMappers = new MaterialMapper[]
                            {
                                ScriptableObject.CreateInstance<UniversalRPMaterialMapper>()
                            };
                        }
                        
                        Debug.Log("attempting to download model: " + fileName);
                        
                        var webRequest = AssetDownloader.CreateWebRequest(content.media_content);
    
                        var i1 = i;
                        AssetDownloader.LoadModelFromUri(
                            webRequest,
                            onLoad: OnLoad,
                            onMaterialsLoad: c => { OnMaterialsLoad(c, content, i1); },
                            onProgress: (c, progress) => { OnProgress(c, progress, i1); },
                            onError: OnError,
                            wrapperGameObject: null,
                            assetLoaderOptions: _assetLoaderOptions,
                            fileExtension: extension
                        );
                    }
                    
                    break;
                }
                
                case ".png" or ".jpg" or ".jpeg": // image
                    StartCoroutine(LoadSprite(path, content, i));
                    break;
                
                default:
                    downloadBar.FailedLoad();
                    // reattempt download here
                    Debug.LogError($"Could not load any media from the file format: {extension}");
                    contentTotalCount--;
                    break;
            }
        }
    }
    
    #region Model Loading Callbacks
    private void OnError(IContextualizedError obj)
    {
        Debug.LogError($"An error occurred while loading your model: {obj.GetInnerException()}");
        contentTotalCount--;
        if (contentLoadedCount >= contentTotalCount) allContentLoaded = true;
    }
    
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress, int index)
    {
        Debug.Log($"Loading Model. Progress: {progress:P}");
        downloadBar.UpdateProgress(index, ((progress * 100) / 2) + 50);
    }
    
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Model mesh and hierarchy loaded successfully. Proceeding to load materials...");
    }
    
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext, MediaContentData mediaContentData, int index)
    {
        Debug.Log("All materials have been applied. The model is fully loaded.");
        contentLoadedCount++;
        var obj = assetLoaderContext.RootGameObject;
        contentDict.TryAdd(index, obj);
        obj.name = "Loaded Model";
        arObject.Add(obj, mediaContentData);

        var cachedObj = Instantiate(obj, DownloadManager.Instance.gameObject.transform);
        DownloadManager.LocalModels.TryAdd(ArtworkToPlace.id, cachedObj);

        if (contentLoadedCount >= contentTotalCount)
        {
            allContentLoaded = true;
            downloadBar?.gameObject.SetActive(false);
        }
    }
    
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext, MediaContentData mediaContentData, int index, Material[] materials)
    {
        Debug.Log("All materials have been applied. The model is fully loaded.");
        contentLoadedCount++;
        var obj = assetLoaderContext.RootGameObject;
        
        var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers is { Length: > 1 })
        {
            meshRenderers[0].material = materials[0];
            meshRenderers[1].material = materials[1];
        }
        
        contentDict.TryAdd(index, obj);
        obj.name = "Loaded Model";
        arObject.Add(obj, mediaContentData);
        
        var cachedObj = Instantiate(obj, DownloadManager.Instance.gameObject.transform);
        DownloadManager.LocalModels.TryAdd(ArtworkToPlace.id, cachedObj);

        if (contentLoadedCount >= contentTotalCount)
        {
            allContentLoaded = true;
            downloadBar?.gameObject.SetActive(false);
        }
    }
    
    #endregion

    private IEnumerator LoadAudioClip(string filePath, MediaContentData content, int index)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File does not exist at path: " + filePath);
            yield break;
        }

        string uri = "file:///" + filePath;

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
        {
            // Enable streaming before sending
            var dh = (DownloadHandlerAudioClip)request.downloadHandler;
            dh.streamAudio = true;

            // Kick off the request
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        
            // Update your progress bar until it's done
            while (!operation.isDone)
            {
                // request.downloadProgress goes 0 → 1
                downloadBar.UpdateProgress(index, (request.downloadProgress * 100) / 2 + 50);
                yield return null;
            }

            // Did we hit an error?
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading audio clip: " + request.error);
                yield break;
            }

            // Grab the clip
            AudioClip audioClip = dh.audioClip;
            if (audioClip == null)
            {
                Debug.LogError("Failed to load audio clip from file");
                yield break;
            }

            // Hook it into your AR system
            var uiObj = arObject.Add(audioClip, content).gameObject;
            contentDict.TryAdd(index, uiObj);
            Debug.Log("Audio clip loaded successfully.");

            // Track overall progress
            contentLoadedCount++;
            if (contentLoadedCount >= contentTotalCount)
            {
                allContentLoaded = true;
                downloadBar?.gameObject.SetActive(false);
            }
        }
    }


    private IEnumerator LoadSprite(string filePath, MediaContentData content, int index)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File does not exist at path: " + filePath);
            yield return null;
        }
        
        byte[] bytes = File.ReadAllBytes(filePath);
        
        Texture2D texture = new Texture2D(2, 2);
        if (!texture.LoadImage(bytes))
        {
            Debug.LogError("Failed to load image data from file");
            downloadBar.FailedDownload();
            yield return null;
        }
        
        Sprite loadedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        
        // AR content loading
        var uiObj = arObject.Add(loadedSprite, content);
        contentDict.TryAdd(index, uiObj);
        Debug.Log("Image downloaded and sprite created.");
        downloadBar.UpdateProgress(index, 100);
        contentLoadedCount++;
        if (contentLoadedCount >= contentTotalCount)
        {
            allContentLoaded = true;
            downloadBar?.gameObject.SetActive(false);
        }

        yield return null;
    }

    #endregion
}