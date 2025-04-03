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
    [SerializeField] private ARNamebar arNamebar;
    [SerializeField] private ARInfoPage arInfoPage;
    [SerializeField] private StatusText statusText;

    [Header("Firebase Preloaded elements")]
    [SerializeField] private ARObject arObjectPrefab;
    [SerializeField] private bool testContent;

    [Header("Debugging")]
    [SerializeField] private Transform outOfScreenLoadLocation;
    [SerializeField] private TMP_Text eventLabel;
    [SerializeField] private GameObject loadingPlane;
    [SerializeField] private ObjectSpawner objectSpawner;

    [Header("Presets")] // cleaned up in a future phase
    [SerializeField] private GameObject coin;
    [SerializeField] private GameObject bird;
    [SerializeField] private GameObject tree;
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
        if (statusText == null) statusText = FindObjectOfType<StatusText>();
        statusText?.SetText("");
    }

    private void Start()
    {
        //StartAR();
        LoadContent();
    }
    
#if UNITY_EDITOR
    private void Update()
    {
        if (testContent)
        {
            testContent = false;
            TryPlaceObject();
        }
    }
#endif
    
    #endregion
    
    // When you select a placement for the AR content
    private void OnTouch(GameObject obj)
    {
        Debug.Log("OnTouch");
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

        UIInfoController.Instance.SetDefaultText("Congratulations, the artwork is placed!");
    }

    // Artwork is ready - show the artwork
    private void OnArtworkReady()
    {
        Debug.Log("Showing Artwork");
        loadingPlane.SetActive(false);
        arObject.gameObject.SetActive(true);
        arObject.Show();
    }

    #region Content Loading

    private async void LoadContent()
    {
        if ((ArtworkToPlace?.content_list == null || ArtworkToPlace.content_list.Count == 0) && string.IsNullOrEmpty(ArtworkToPlace?.preset))
        {
            Debug.LogWarning("Artwork to place is missing or there is no content available.");
            return;
        } 

        hasContent = true;
        bool loadContent = true;
        
        if (!string.IsNullOrEmpty(ArtworkToPlace.preset) && ArtworkToPlace.preset != "None")
        {
            Debug.Log("Loading a preset: " + ArtworkToPlace.preset);
                
            switch (ArtworkToPlace.preset)
            {
                case "Bird Animation":
                {
                    var birdObj = Instantiate(bird, Vector3.zero, Quaternion.identity);
                    arObject.Add(birdObj);
                    contentDict.TryAdd(contentDict.Count, birdObj);
                    contentLoadedCount++;
                    break;
                }
                case "Coin Clicker":
                {
                    var coinObj = Instantiate(coin, Vector3.zero, Quaternion.identity);
                    arObject.Add(coinObj);
                    contentDict.TryAdd(contentDict.Count, coinObj);
                    contentLoadedCount++;
                    break;
                }
                case "Tree":
                    var treeObj = Instantiate(tree, Vector3.zero, Quaternion.identity);
                    arObject.Add(treeObj);
                    contentDict.TryAdd(contentDict.Count, treeObj);
                    contentLoadedCount++;
                    break;
                case "Monument":
                    loadContent = false;
                    
                    if (ArtworkToPlace.content_list.Count > 0)
                    {
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
                            statusText?.SetText("Downloading...");
                            var results = await FirebaseLoader.DownloadMedia(AppCache.ContentFolder, content.media_content);
                            path = results.localPath;
                        }
                        else if (File.Exists(localPath)) // if the file does exist, set the path to that location
                        {
                            path = localPath;
                        }
                        
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
                            onProgress: OnProgress,
                            onError: OnError,
                            wrapperGameObject: null,
                            assetLoaderOptions: _assetLoaderOptions
                        );
                        break;
                    }
                    
                    // load content
                    // on material load custom?
                    break;
            }

            if (contentLoadedCount >= contentTotalCount)
            {
                allContentLoaded = true;
                statusText?.gameObject.SetActive(false);
            }
        }

        if (!loadContent) return;

        for (int i = 0; i < ArtworkToPlace.content_list.Count; i++)
        {
            var content = ArtworkToPlace.content_list[i];
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
                statusText?.SetText("Downloading...");
                var results = await FirebaseLoader.DownloadMedia(AppCache.ContentFolder, content.media_content);
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
            
            statusText?.SetText("Loading...");
            
            switch (extension)
            {
                case ".mp4" or ".mvk" or ".mov": // video
                    Debug.Log($"content [{ArtworkToPlace.title}] contained a media piece");
                    Debug.LogWarning("Content positioning, scale and rotation still needs to be adjusted");
                
                    containsVideo = true;
                    arObject.Add(content, content.media_content, player =>
                    {
                        Debug.Log("Video prepared");
                        contentLoadedCount++;
                        if (contentLoadedCount >= contentTotalCount)
                        {
                            allContentLoaded = true;
                            statusText?.gameObject.SetActive(false);
                        }
                    });
                    break;
                
                case ".mp3": // audio
                    Debug.Log($"content [{ArtworkToPlace.title}] contained an audio piece");
                    StartCoroutine(DownloadAudioClip(path));
                    break;
                
                case ".fbx" or ".obj" or ".gltf" or ".gltf2" when storedLocal: // stored local model
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
                    var i1 = i;
                    AssetLoader.LoadModelFromFile(
                        path: path,
                        onLoad: OnLoad,
                        onMaterialsLoad: c => { OnMaterialsLoad(c, content, i1); },
                        onProgress: OnProgress,
                        onError: OnError,
                        wrapperGameObject: null,
                        assetLoaderOptions: _assetLoaderOptions
                    );
                    break;
                }
                
                case ".fbx" or ".obj" or ".gltf" or ".gltf2": // url model
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
                        onProgress: OnProgress,
                        onError: OnError,
                        wrapperGameObject: null,
                        assetLoaderOptions: _assetLoaderOptions,
                        fileExtension: extension
                    );
                    break;
                }
                
                case ".png" or ".jpg" or ".jpeg": // image
                    Debug.Log($"content [{ArtworkToPlace.title}] contained an image piece");
                    StartCoroutine(DownloadImageAsSprite(path, content, i));
                    break;
                
                default:
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
    
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        Debug.Log($"Loading Model. Progress: {progress:P}");
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

        if (contentLoadedCount >= contentTotalCount)
        {
            allContentLoaded = true;
            statusText?.gameObject.SetActive(false);
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

        if (contentLoadedCount >= contentTotalCount)
        {
            allContentLoaded = true;
            statusText?.gameObject.SetActive(false);
        }
    }

    
    #endregion
    
    private IEnumerator DownloadAudioClip(string url)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                contentTotalCount--;
                Debug.LogError("Error downloading audio: " + www.error);
            }
            else
            {
                Debug.Log("AudioClip downloaded");
                contentLoadedCount++;
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                arObject.Add(clip);
                if (contentTotalCount == 1)
                {
                    arObject.Show();
                }

                if (contentLoadedCount >= contentTotalCount)
                {
                    allContentLoaded = true;
                    statusText.gameObject.SetActive(false);
                }
            }
        }
    }
    
    private IEnumerator DownloadImageAsSprite(string imageUrl, MediaContentData content, int index)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download image: {uwr.error}");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                // Create a sprite with the texture. The pivot is set to the center.
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                var uiObj = arObject.Add(sprite, content);
                contentDict.TryAdd(index, uiObj);
                Debug.Log("Image downloaded and sprite created.");
                contentLoadedCount++;
                if (contentLoadedCount >= contentTotalCount)
                {
                    allContentLoaded = true;
                    statusText?.gameObject.SetActive(false);
                }
            }
        }
    }

    #endregion
}