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
using UnityEngine.XR.ARSubsystems;

public class ArTapper : MonoBehaviour
{
    public ARAnchorManager anchorManager;
    public ARAnchor anchor;
    
    public static ArtworkData ArtworkToPlace;

    public GameObject placementIndicator;
    public GameObject AnimationIndicator;
    public static bool PlaceDirectly = false;
    public static float DistanceWhenActivated;

    public GameObject PlacedObject;
    //private AssetReferenceGameObject assetReferenceGameObject;
    bool searching;

    [Header("References")]
    public ARSession arSession;
    public XROrigin arOrigin;
    public ARRaycastManager arRaycast;
    [SerializeField] private ARNamebar arNamebar;
    [SerializeField] private ARInfoPage arInfoPage;

    [Header("Firebase Preloaded elements")]
    [SerializeField] private ARObject arObjectPrefab;
    [SerializeField] private bool testContent;

    [Header("Debugging")]
    [SerializeField] private Transform outOfScreenLoadLocation;
    [SerializeField] private TMP_Text eventLabel;
    [SerializeField] private GameObject loadingPlane;

    [Header("Presets")] // cleaned up in a future phase
    [SerializeField] private GameObject coin;
    [SerializeField] private GameObject bird;

    private ARObject arObject;
    
    private Pose placementPose;
    private ARRaycastHit foundHit;

    private bool placementPoseIsValid = false;

    private bool videoPlayerReady = false;
    private bool hasContent = false;
    private bool containsVideo = false;
    
    bool StartedAnimation;
    
    private int contentTotalCount, contentLoadedCount = 0;
    private bool allContentLoaded = false;

    public Dictionary<int, GameObject> contentDict = new Dictionary<int, GameObject>();
    
    private void Start()
    {   
        arObject = Instantiate(arObjectPrefab);
        StartAR();
        LoadContent();

        Debug.Log("d: " + DistanceWhenActivated);
    }
    
    private void Update()
    {
        if (searching && PlaceDirectly)
        {
            HandlePlacementSearching();
        }

        if (searching)
        {
            UpdatePlacementPose();

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.P) && placementPoseIsValid) {
                StopAR();
                PlaceObject();
            }
                
#endif
            if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) 
                {
                    StopAR();
                    PlaceObject();
                }
            }
        }

        if (testContent)
        {
            testContent = false;
            //OnArtworkReady();
            StopAR();
            PlaceObject();
        }
    }

    private void HandlePlacementSearching()
    {
        placementIndicator.SetActive(false);
        AnimationIndicator.SetActive(false);
        UIInfoController.Instance.SetText("", 3);
            
        if (hasContent && allContentLoaded)
        {
            OnArtworkReady();
        }
        else if (hasContent && !allContentLoaded)
        {
            Vector3 _pos = PlacedObject.transform.position;
            //_pos.z += DistanceWhenActivated;
            loadingPlane.SetActive(true);
            loadingPlane.transform.position = _pos;
            
            StartCoroutine(WaitForLoad());
            StartCoroutine(WaitForLoad());
        }
        else if(!hasContent)
        {
            Debug.Log("no content found...");
            PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
        }
            
        Vector3 pos = PlacedObject.transform.position;
        //pos.z += DistanceWhenActivated;
        PlacedObject.transform.position = pos;
        StopAR();
    }

    private IEnumerator WaitForLoad()
    {
        yield return new WaitUntil(() => allContentLoaded);
        loadingPlane.gameObject.SetActive(false);
        OnArtworkReady();
    }

    public void StartAR()
    {   
        AnimationIndicator.SetActive(true);   
        placementIndicator.SetActive(false);
        searching = true;
        UIInfoController.Instance.SetScanText(
            "AR-Camera is scanning the area. Please <u>scan the floor in front of you.</u> <color=black>Learn more...</color>",
            "AR-Camera is scanning the area. Please scan the floor in front of you using your phone’s camera. The artwork will appear automatically once the scan is completed. <color=black>Show less...</color>");
    }

    public void StopAR()
    {
        AnimationIndicator.SetActive(false);
        placementIndicator.SetActive(false);
        UIInfoController.Instance.RemoveAllText();
        searching = false;
    }

    private void PlaceObject()
    {
#if !UNITY_EDITOR
        //Create an AR Anchor
        if(anchor==null) anchor = anchorManager.AttachAnchor((ARPlane)foundHit.trackable, foundHit.pose);
#endif
        
        if (PlacedObject == null)
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
                PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
                LoadTopFinder(PlacedObject);
            }
        }
        else
        {
            PlacedObject.SetActive(true);

            PlacedObject.transform.position = placementPose.position;
            PlacedObject.transform.rotation = placementPose.rotation;
        }

        UIInfoController.Instance.StartCameraTutorial();
        //Track object based on anchor
        //PlacedObject.transform.parent = anchor.transform;
        
        arNamebar.SetNamebarLabel(ArtworkToPlace.title);
        arInfoPage.CanOpen = true;

        UIInfoController.Instance.SetDefaultText("Congratulations, the artwork is placed!");
    }

    private void OnArtworkReady()
    {
        loadingPlane.SetActive(false);
        
        arObject.Show();
        
        PlacedObject = arObject.gameObject;
        PlacedObject.SetActive(true);
        PlacedObject.transform.position = placementPose.position;
        PlacedObject.transform.rotation = placementPose.rotation;
                
        //Track object based on anchor
        if (anchor != null) PlacedObject.transform.parent = anchor.transform;
        
        LoadTopFinder(PlacedObject);
        
        Vector3 pos = PlacedObject.transform.position;
        //pos.z += DistanceWhenActivated;
        PlacedObject.transform.position = pos;
        StopAR();
    }

    private void LoadTopFinder (GameObject ARObject)
    {
        ModelTopFinder topFinder = GetComponent<ModelTopFinder>();
        if (topFinder != null)
        {
            topFinder.prefabInstance = ARObject;
            topFinder.FindTopmostPointOfModels();
        }
    }

    private void UpdatePlacementPose() 
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space)) placementPoseIsValid = !placementPoseIsValid;
#else
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        arRaycast.Raycast(screenCenter, hits, TrackableType.Planes);
        placementPoseIsValid = hits.Count > 0;
#endif
        if (placementPoseIsValid) 
        {
            placementIndicator.SetActive(true);
            AnimationIndicator.SetActive(false);
            UIInfoController.Instance.arTutorialManager.ShowPlaceHint();// shows text popup to tell the player to place the object
#if !UNITY_EDITOR
            placementPose = hits[0].pose;

            //Save the hit
            foundHit = hits[0];

            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            loadingPlane.transform.localRotation = Quaternion.Euler(90f, 0f, placementPose.rotation.eulerAngles.z);
#endif
        } 
        else 
        {
            AnimationIndicator.SetActive(true);
            placementIndicator.SetActive(false);
            //UIInfoController.Instance.SetText("Scan the space around you.", 0);
            Vector3 curAngle = AnimationIndicator.transform.eulerAngles;
            curAngle.x = 0;
            curAngle.z = 0;
            AnimationIndicator.transform.eulerAngles = curAngle;
        }
    }
    
    private async void LoadContent()
    {
        if ((ArtworkToPlace?.content_list == null || ArtworkToPlace.content_list.Count == 0) && string.IsNullOrEmpty(ArtworkToPlace?.preset))
        {
            Debug.LogWarning("Artwork to place is missing or there is no content available.");
            return;
        } 

        hasContent = true;

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

            string localPath = content.media_content;
            string local = Path.Combine(AppCache.ContentFolder, fileName);
            if (!File.Exists(local))
            {
                var results = await FirebaseLoader.DownloadMedia(AppCache.ContentFolder, content.media_content);
                localPath = results.localPath;
                if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
                {
                    Debug.Log("Content was downloaded and stored locally");
                    storedLocal = true;
                }
            }
            else if (File.Exists(local))
            {
                localPath = local;
                storedLocal = true;
                Debug.Log("Content was found locally");
            }
            
            if (extension is ".mp4" or ".mvk" or ".mov") // video formats
            {
                Debug.Log($"content [{ArtworkToPlace.title}] contained a media piece");
                Debug.LogWarning("Content positioning, scale and rotation still needs to be adjusted");
                
                containsVideo = true;
                arObject.Add(content, localPath, player =>
                {
                    Debug.Log("Video prepared");
                    contentLoadedCount++;
                    if (contentLoadedCount >= contentTotalCount) allContentLoaded = true;
                });
            }
            else if (extension is ".mp3")
            {
                Debug.Log($"content [{ArtworkToPlace.title}] contained an audio piece");
                StartCoroutine(DownloadAudioClip(localPath));
            }
            else if (extension is ".fbx" or ".obj" or ".gltf" or ".gltf2") // model format
            {
                if (storedLocal)
                {
                    var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, false);
                    assetLoaderOptions.AnimationType = AnimationType.Legacy;

                    Debug.Log("attempting to load local model file: " + fileName);

                    // Load the model from the local file path instead of downloading it.
                    var i1 = i;
                    AssetLoader.LoadModelFromFile(
                        path: localPath,
                        onLoad: OnLoad,
                        onMaterialsLoad: c => { OnMaterialsLoad(c, content, i1); },
                        onProgress: OnProgress,
                        onError: OnError,
                        wrapperGameObject: null,
                        assetLoaderOptions: assetLoaderOptions
                    );
                }
                else
                {
                    var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
                    assetLoaderOptions.AnimationType = AnimationType.Legacy;
                    
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
                        assetLoaderOptions: assetLoaderOptions,
                        fileExtension: extension
                    );
                }
            }
            else if (extension is ".png" or ".jpg" or ".jpeg")
            {
                Debug.Log($"content [{ArtworkToPlace.title}] contained an image piece");
                StartCoroutine(DownloadImageAsSprite(localPath, content, i));
            }
            else
            {
                Debug.LogError($"Could not load any media from the file format: {extension}");
                contentTotalCount--;
            }
        }
        
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
            }
                
            if (contentLoadedCount >= contentTotalCount)
                allContentLoaded = true;
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
        
        if (contentLoadedCount >= contentTotalCount) allContentLoaded = true;
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
                
                if (contentLoadedCount >= contentTotalCount) allContentLoaded = true;
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
                    allContentLoaded = true;
            }
        }
    }
}