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

    [Header("Debugging")]
    [SerializeField] private Transform outOfScreenLoadLocation;
    [SerializeField] private TMP_Text eventLabel;
    [SerializeField] private GameObject loadingPlane;

    [Header("Firebase Preloaded elements")]
    [SerializeField] private ARVideoObject arVideoObject;
    
    private Pose placementPose;
    private ARRaycastHit foundHit;

    private bool placementPoseIsValid = false;

    private bool videoPlayerReady = false;
    private bool hasContent = false;
    
    bool StartedAnimation;

    private GameObject cachedArtworkObject = null;
    private List<VideoPlayer> cachedVideoPlayers = new();
    private event Action<GameObject> onArtworkReady = null;
    
    void Start()
    {   
        //arSession?.Reset(); // This casuses crashes on android, but it is needed in the future
        //arOrigin = FindObjectOfType<XROrigin>();
        //arRaycast = FindObjectOfType<ARRaycastManager>();
        //anchorManager = gameObject.GetComponent<ARAnchorManager>();
        
        StartAR();
        LoadContent();

        // being replaced with firebase loading
        //if (ArtworkToPlace?.ARObjectReference != null) LoadAssetFromReference(ArtworkToPlace?.ARObjectReference);
        //else eventLabel.text = "No AR object...";
    }

    public void StartAR()
    {   
        //OnDemandRendering.renderFrameInterval = 1;
        AnimationIndicator.SetActive(true);   
        placementIndicator.SetActive(false);
        //AR-Camera is scanning the area. Please scan the floor in front of you using your phone’s camera. The artwork will appear automatically once the scan is completed. Show less...
        searching = true;
        //UIInfoController.Instance.SetText("Tap on the Wava button to let it appear", 0);
        //UIInfoController.Instance.SetText("Scan the space around you", 0);
        UIInfoController.Instance.SetScanText(
            "AR-Camera is scanning the area. Please <u>scan the floor in front of you.</u> <color=black>Learn more...</color>",
            "AR-Camera is scanning the area. Please scan the floor in front of you using your phone’s camera. The artwork will appear automatically once the scan is completed. <color=black>Show less...</color>");
    }

    public void StopAR()
    {
        AnimationIndicator.SetActive(false);
        placementIndicator.SetActive(false);
        //UIInfoController.Instance.SetText("", 3);
        UIInfoController.Instance.RemoveAllText();
        searching = false;
        
        //if ARPointToPlace?.ARObject.slowDownUpdate
        //OnDemandRendering.renderFrameInterval = 6;
    }

    void Update()
    {
        if (searching && PlaceDirectly)
        {
            placementIndicator.SetActive(false);
            AnimationIndicator.SetActive(false);
            UIInfoController.Instance.SetText("", 3);

            bool stillLoading = false;
            
            // DISABLED
            if (hasContent)
            {
                if (cachedArtworkObject != null) OnArtworkReady(cachedArtworkObject); // if the addressable object is loaded into memory AND ready for use
                else if(onArtworkReady == null)
                {
                    //eventLabel.text = "Artwork still loading";
                    //loadingPlane.SetActive(true);
                    loadingPlane.transform.localPosition = placementPose.position + new Vector3(0, 0.75f, 0);;
                    loadingPlane.transform.localRotation = Quaternion.Euler(90f, 0f, placementPose.rotation.eulerAngles.z);
                    stillLoading = true;
                    onArtworkReady += OnArtworkReady; // if the object is still loading subscribe
                }
            }
            else PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
            
            Vector3 pos = PlacedObject.transform.position;
            pos.z += DistanceWhenActivated;
            PlacedObject.transform.position = pos;
            if (stillLoading) loadingPlane.transform.position = pos;
            StopAR();
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
            // DISABLED
            if (hasContent)
            {
                if (cachedArtworkObject != null) OnArtworkReady(cachedArtworkObject); // if the addressable object is loaded into memory AND ready for use
                else if(onArtworkReady == null)
                {
                    //eventLabel.text = "Artwork still loading";
                    //loadingPlane.SetActive(true);
                    loadingPlane.transform.localPosition = placementPose.position + new Vector3(0, 0.75f, 0);
                    loadingPlane.transform.localRotation = Quaternion.Euler(90f, 0f, placementPose.rotation.eulerAngles.z);
                    onArtworkReady += OnArtworkReady; // if the object is still loading subscribe
                }

                //PlacedObject = Instantiate(ARPointToPlace.ARObject, placementPose.position, placementPose.rotation);
                //assetReferenceGameObject = ARPointToPlace.ARObjectReference;
                //LoadAssetFromReference(ARPointToPlace.ARObjectReference);
                //PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
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
        
        //placementIndicator.SetActive(false);
        //AnimationIndicator.SetActive(false);

        if (ArtworkToPlace.media_content != null && Path.GetExtension(ArtworkToPlace.media_content) == ".mp3")
            UIInfoController.Instance.SetDefaultText("Adjust volume to hear artwork");
        else
            UIInfoController.Instance.SetDefaultText("Congratulations, the artwork is placed!");
    }

    private void OnArtworkReady(GameObject artworkObject)
    {
        onArtworkReady = null;

        loadingPlane.SetActive(false);
        //eventLabel.text = "Spawned object";
        
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

        if (cachedVideoPlayers.Count > 0)
        {
            foreach (var vp in cachedVideoPlayers)
            {
                vp.frame = 0;
                vp.Play();
            }
        }
    }

    public void LoadTopFinder (GameObject ARObject)
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

    public void OnDisable()
    {
        // DISABLED
        /*if (ArtworkToPlace?.ARObjectReference != null)
        {
            if(PlacedObject!=null) ReleaseAssetInstanceFromMemory(PlacedObject, ArtworkToPlace.ARObjectReference);
            ReleaseAssetFromMemory(ArtworkToPlace.ARObjectReference);
        }*/

        foreach (var videoPlayer in cachedVideoPlayers)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        onArtworkReady = null;
    }
    
    private void LoadContent()
    {
        if (ArtworkToPlace == null)
        {
            Debug.LogWarning("Artwork to place is missing");
            return;
        }

        if (!string.IsNullOrEmpty(ArtworkToPlace.media_content))
        {
            Debug.Log($"content [{ArtworkToPlace.title}] was a media piece with the url: " + ArtworkToPlace.media_content);
            arVideoObject.PrepareVideo(ArtworkToPlace.media_content, player =>
            {
                Debug.Log("Video prepared");
                videoPlayerReady = true;
            });
            hasContent = true;
        }
        else if (!string.IsNullOrEmpty(ArtworkToPlace.content_url))
        {
            Debug.Log($"content [{ArtworkToPlace.title}] was a direct model url: " + ArtworkToPlace.content_url);
            hasContent = true;
        }
        else if (!string.IsNullOrEmpty(ArtworkToPlace.preset))
        {
            Debug.Log($"content [{ArtworkToPlace.title}] was an enum: " + ArtworkToPlace.preset);
            hasContent = true;
        }
    }

    public void LoadAssetFromReference(AssetReferenceGameObject assetToLoad)
    {
        //eventLabel.text = "Loading artwork...";
        
        Addressables.LoadAssetAsync<GameObject>(assetToLoad).Completed += (asyncOperationHandle) =>
        {
            if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                cachedArtworkObject = asyncOperationHandle.Result;
               // eventLabel.text = "Loaded an artwork to memory";

                // Cache all video players
                cachedVideoPlayers = new();
                cachedVideoPlayers = GetComponentsInChildren<VideoPlayer>().ToList();
                if(cachedArtworkObject.TryGetComponent<VideoPlayer>(out var videoPlayer)) cachedVideoPlayers.Add(videoPlayer);
                
                if (cachedVideoPlayers.Count > 0)
                {
                    prepareCount = 0;
                   // eventLabel.text = "Preparing videos...";
                    foreach (var vp in cachedVideoPlayers)
                    {
                        vp.playOnAwake = false;
                        vp.prepareCompleted += OnVideoPrepared;
                        vp.Prepare();
                    }
                }
                
                cachedArtworkObject = Instantiate(cachedArtworkObject, outOfScreenLoadLocation.localPosition, Quaternion.identity);

                if (cachedVideoPlayers.Count > 0)
                {
                    
                }
                else
                {
                    cachedArtworkObject.SetActive(false);
                    onArtworkReady?.Invoke(cachedArtworkObject);
                }
                
                
                //PlacedObject = asyncOperationHandle.Result;
                //GameObject NewPlacedObject = Instantiate(PlacedObject, placementPose.position, placementPose.rotation);
                
                //Track object based on anchor
                /*if (anchor != null)
                    PlacedObject.transform.parent = anchor.transform;

                LoadTopFinder(NewPlacedObject);*/
            }
            else
            {
                //eventLabel.text = "No artwork";
                UIInfoController.Instance.SetDefaultText("No valid artwork found.");
            }
        };
    }

    private int prepareCount = 0;
    private void OnVideoPrepared(VideoPlayer vp)
    {
        prepareCount++;
        if (prepareCount >= cachedVideoPlayers.Count)
        {
            cachedArtworkObject.SetActive(true);
            onArtworkReady?.Invoke(cachedArtworkObject);
        }
    }

    public void ReleaseAssetInstanceFromMemory(GameObject objectToRelease, AssetReferenceGameObject assetReference)
    {
        assetReference.ReleaseInstance(objectToRelease);
    }
    
    public void ReleaseAssetFromMemory(AssetReferenceGameObject assetReference)
    {
        assetReference.ReleaseAsset();
    }
}

