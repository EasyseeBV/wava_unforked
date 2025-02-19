using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.IO;
using System.Linq;
using Messy.Definitions;
using TMPro;
using TriLibCore;
using Unity.XR.CoreUtils;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using UnityEngine.Video;

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
    [SerializeField] private ARVideoObject arVideoObject;
    [SerializeField] private ARModelObject arModelObject;
    [SerializeField] private bool testContent;

    [Header("Debugging")]
    [SerializeField] private Transform outOfScreenLoadLocation;
    [SerializeField] private TMP_Text eventLabel;
    [SerializeField] private GameObject loadingPlane;
    [Space] [SerializeField] private bool placeObject;
    
    private Pose placementPose;
    private ARRaycastHit foundHit;

    private bool placementPoseIsValid = false;

    private bool videoPlayerReady = false;
    private bool hasContent = false;
    private bool containsVideo = false;
    
    bool StartedAnimation;

    private GameObject cachedArtworkObject = null;
    
    private void Start()
    {   
        StartAR();
        LoadContent();
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
            
            if (placeObject)
            {
                placeObject = false;
                OnArtworkReady();
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
    }

    private void HandlePlacementSearching()
    {
        placementIndicator.SetActive(false);
        AnimationIndicator.SetActive(false);
        UIInfoController.Instance.SetText("", 3);

        bool stillLoading = false;
            
        if (hasContent)
        {
            if (cachedArtworkObject != null) OnArtworkReady(); // if the object is loaded into memory AND ready for use
            else
            {
                loadingPlane.transform.localPosition = placementPose.position + new Vector3(0, 0.75f, 0);;
                loadingPlane.transform.localRotation = Quaternion.Euler(90f, 0f, placementPose.rotation.eulerAngles.z);
                stillLoading = true;
            }
        }
        else PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
            
        Vector3 pos = PlacedObject.transform.position;
        pos.z += DistanceWhenActivated;
        PlacedObject.transform.position = pos;
        if (stillLoading) loadingPlane.transform.position = pos;
        StopAR();
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
        if(anchor==null)anchor = anchorManager.AttachAnchor((ARPlane)foundHit.trackable, foundHit.pose);
#endif

        if (videoPlayerReady)
        {
            arVideoObject.Play();
            cachedArtworkObject = arVideoObject.gameObject;
        }
        
        if (PlacedObject == null)
        {
            if (hasContent)
            {
                if (cachedArtworkObject != null) OnArtworkReady(); // if the object is loaded into memory AND ready for use
            }
            else
            {
                PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), placementPose.position, placementPose.rotation);
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

        if (ArtworkToPlace.media_content != null && Path.GetExtension(ArtworkToPlace.media_content) == ".mp3")
            UIInfoController.Instance.SetDefaultText("Adjust volume to hear artwork");
        else
            UIInfoController.Instance.SetDefaultText("Congratulations, the artwork is placed!");
    }

    private void OnArtworkReady()
    {
        loadingPlane.SetActive(false);
        
        if (!containsVideo) arModelObject.Show(ArtworkToPlace.transforms);
        
        PlacedObject = cachedArtworkObject;
        PlacedObject.SetActive(true);
        PlacedObject.transform.position = placementPose.position;
        PlacedObject.transform.rotation = placementPose.rotation;
                
        //Track object based on anchor
        if (anchor != null) PlacedObject.transform.parent = anchor.transform;
        
        LoadTopFinder(PlacedObject);
        
        Vector3 pos = PlacedObject.transform.position;
        pos.z += DistanceWhenActivated;
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
            UIInfoController.Instance.arTutorialManager.ShowPlaceHint();//SetText("Tap on the Wava button to let it appear", 0);
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

    private AssetLoaderOptions assetLoaderOptions;
    private void LoadContent()
    {
        if (ArtworkToPlace == null || ArtworkToPlace.media_content == null)
        {
            Debug.LogWarning("Artwork to place is missing or there is no content available.");
            return;
        }
        
        var uri = new Uri(ArtworkToPlace.media_content);
        string encodedPath = uri.AbsolutePath;
        string decodedPath = Uri.UnescapeDataString(encodedPath);
        string fileName = Path.GetFileName(decodedPath);
        var extension = Path.GetExtension(fileName);
        
        if (extension is ".mp4" or ".mvk") // video formats
        {
            Debug.Log($"content [{ArtworkToPlace.title}] was a media piece with the url: " + ArtworkToPlace.media_content);
            containsVideo = true;
            arVideoObject.PrepareVideo(ArtworkToPlace.media_content, player =>
            {
                Debug.Log("Video prepared");
                videoPlayerReady = true;
            });
            hasContent = true;
        }
        else if (extension is ".fbx" or ".obj" or ".gltf" or ".gltf2") // model format
        {
            if (assetLoaderOptions == null)
            {
                assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
            }

            Debug.Log("attempting to download: " + ArtworkToPlace.media_content);
            
            var webRequest = AssetDownloader.CreateWebRequest(ArtworkToPlace.media_content);
            containsVideo = false;
            
            AssetDownloader.LoadModelFromUri(
                webRequest,
                onLoad: OnLoad,
                onMaterialsLoad: OnMaterialsLoad,
                onProgress: OnProgress,
                onError: OnError,
                wrapperGameObject: null,
                assetLoaderOptions: assetLoaderOptions,
                fileExtension: extension
            );
            hasContent = true;
        }
        else if (extension is ".assetbundle") // unity asset bundle format 
        {
            containsVideo = false;
            Debug.LogWarning("AssetBundles are currently not supported");
        }
        else
        {
            containsVideo = false;
            Debug.LogError($"Could not load any media from the file format: {extension}");
        }
    }
    
    #region Model Loading Callbacks
    private void OnError(IContextualizedError obj)
    {
        Debug.LogError($"An error occurred while loading your model: {obj.GetInnerException()}");
    }
    
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        Debug.Log($"Loading Model. Progress: {progress:P}");
    }
    
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Model mesh and hierarchy loaded successfully. Proceeding to load materials...");
        arModelObject.Assign(assetLoaderContext.RootGameObject);
    }
    
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("All materials have been applied. The model is fully loaded.");
        var obj = assetLoaderContext.RootGameObject;
        obj.name = "Loaded Model";
        cachedArtworkObject = obj;
    }
    #endregion
}