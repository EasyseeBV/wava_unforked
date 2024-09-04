using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using Messy.Definitions;
using Unity.XR.CoreUtils;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

public class ArTapper : MonoBehaviour
{
    public ARAnchorManager anchorManager;
    public ARAnchor anchor;
    
    //Replaced ARPoint with ARPointSO
    public static ARPointSO ARPointToPlace;

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
    
    private Pose placementPose;
    private ARRaycastHit foundHit;

    private bool placementPoseIsValid = false;

    bool StartedAnimation;
    
    void Start()
    {   
        //arSession?.Reset(); // This casuses crashes on android, but it is needed in the future
        //arOrigin = FindObjectOfType<XROrigin>();
        //arRaycast = FindObjectOfType<ARRaycastManager>();
        //anchorManager = gameObject.GetComponent<ARAnchorManager>();
        StartAR();
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
            
            //Replaced ARObject with ARObjectReference
            if (ARPointToPlace.ARObjectReference != null)
            {
                //assetReferenceGameObject = ARPointToPlace.ARObjectReference;
                LoadAssetFromReference(ARPointToPlace.ARObjectReference);
                //PlacedObject = Instantiate(ARPointToPlace.ARObject);  
                //PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
            }
    
            else
                PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
                //Debug.Log("Still null");
            Vector3 pos = PlacedObject.transform.position;
            pos.z += DistanceWhenActivated;
            PlacedObject.transform.position = pos;
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
                if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) {
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
        if(anchor==null)
            anchor = anchorManager.AttachAnchor((ARPlane)foundHit.trackable, foundHit.pose);
#endif

        if (PlacedObject == null)
        {
            if (ARPointToPlace.ARObjectReference != null)
            {
                //PlacedObject = Instantiate(ARPointToPlace.ARObject, placementPose.position, placementPose.rotation);
                //assetReferenceGameObject = ARPointToPlace.ARObjectReference;
                LoadAssetFromReference(ARPointToPlace.ARObjectReference);
                //PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
            }

            else
            {
                PlacedObject = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), placementPose.position, placementPose.rotation);
                LoadTopFinder(PlacedObject);
            }
            //Debug.Log("Still null");
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
        
        arNamebar.SetNamebarLabel(ARPointToPlace.Title);
        arInfoPage.CanOpen = true;
        
        //placementIndicator.SetActive(false);
        //AnimationIndicator.SetActive(false);

        if (ARPointToPlace.IsAudio)
            UIInfoController.Instance.SetDefaultText("Adjust volume to hear artwork");
        else
            UIInfoController.Instance.SetDefaultText("Congratulations, the artwork is placed!");
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

    private void UpdatePlacementPose() {


#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
            placementPoseIsValid = !placementPoseIsValid;
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
        if (ARPointToPlace?.ARObjectReference != null)
        {
            if(PlacedObject!=null)
                ReleaseAssetInstanceFromMemory(PlacedObject, ARPointToPlace.ARObjectReference);
            
            ReleaseAssetFromMemory(ARPointToPlace.ARObjectReference);
        }
    }


    public void LoadAssetFromReference(AssetReferenceGameObject assetToLoad)
    {
        //assetToLoad.InstantiateAsync().Completed += (asyncOperation) => PlacedObject = asyncOperation.Result;
        //Track object based on anchor
        //PlacedObject.transform.parent = anchor.transform;
        
        //assetToLoad.LoadAssetAsync<GameObject>().Completed += (asyncOperationHandle) =>
        Addressables.LoadAssetAsync<GameObject>(assetToLoad).Completed += (asyncOperationHandle) =>
        {
            if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                PlacedObject = asyncOperationHandle.Result;
                GameObject NewPlacedObject = Instantiate(PlacedObject, placementPose.position, placementPose.rotation);
                
                //Track object based on anchor
                if (anchor != null)
                    PlacedObject.transform.parent = anchor.transform;

                LoadTopFinder(NewPlacedObject);
            }
            else
            {   
                //Do we want some kind of message like this?
                UIInfoController.Instance.SetDefaultText("No valid artwork found.");
                //Debug.Log("Asset not found!");
            }
        };
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

